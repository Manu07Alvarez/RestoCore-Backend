namespace RestoCore.Application.Common.Mediator;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Common.Behaviors;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, object> InvokerCache = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var invoker = (IRequestInvoker<TResponse>)InvokerCache.GetOrAdd(
            requestType,
            static type =>
            {
                var responseInterface = type.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
                var responseType = responseInterface.GetGenericArguments()[0];
                var invokerType = typeof(RequestInvoker<,>).MakeGenericType(type, responseType);
                return Activator.CreateInstance(invokerType)!;
            });

        return await invoker.InvokeAsync(_serviceProvider, request, cancellationToken);
    }
}

public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationMediator(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<ISender>(sp => sp.GetRequiredService<IMediator>());
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var handlerInterfaceType = typeof(IRequestHandler<,>);
        var validatorInterfaceType = typeof(FluentValidation.IValidator<>);

        var types = assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface).ToList();

        var handlers = types
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i })
            .Where(m => m.Interface.IsGenericType && m.Interface.GetGenericTypeDefinition() == handlerInterfaceType);

        foreach (var handler in handlers)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        var validators = types
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i })
            .Where(m => m.Interface.IsGenericType && m.Interface.GetGenericTypeDefinition() == validatorInterfaceType);

        foreach (var validator in validators)
        {
            services.AddScoped(validator.Interface, validator.Implementation);
        }

        return services;
    }
}
