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

        // Automatic FluentValidation Execution
        var validatorType = typeof(FluentValidation.IValidator<>).MakeGenericType(requestType);
        var validator = _serviceProvider.GetService(validatorType);
        if (validator != null)
        {
            var validateMethod = validatorType.GetMethod("ValidateAsync", new[] { requestType, typeof(CancellationToken) });
            if (validateMethod != null)
            {
                var valTask = (Task)validateMethod.Invoke(validator, new object[] { request, cancellationToken })!;
                await valTask.ConfigureAwait(false);
                var resultProp = valTask.GetType().GetProperty("Result");
                var validationResult = resultProp?.GetValue(valTask) as FluentValidation.Results.ValidationResult;
                if (validationResult != null && !validationResult.IsValid)
                {
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }
            }
        }

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
