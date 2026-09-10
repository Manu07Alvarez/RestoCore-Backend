namespace RestoCore.Application.Common.Mediator;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type '{requestType.FullName}'.");
        }

        var method = handlerType.GetMethod("Handle", new[] { requestType, typeof(CancellationToken) });
        if (method == null)
        {
            throw new InvalidOperationException($"Handle method not found on handler '{handlerType.FullName}'.");
        }

        var task = (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
        return await task;
    }
}

public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationMediator(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<ISender>(sp => sp.GetRequiredService<IMediator>());

        var handlerInterfaceType = typeof(IRequestHandler<,>);

        var handlers = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i })
            .Where(m => m.Interface.IsGenericType && m.Interface.GetGenericTypeDefinition() == handlerInterfaceType);

        foreach (var handler in handlers)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        return services;
    }
}
