namespace RestoCore.Application.Common.Mediator;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

public interface IRequestInvoker<TResponse>
{
    Task<TResponse> InvokeAsync(
        IServiceProvider serviceProvider,
        object request,
        CancellationToken cancellationToken);
}

internal sealed class RequestInvoker<TRequest, TResponse> : IRequestInvoker<TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> InvokeAsync(
        IServiceProvider serviceProvider,
        object request,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>();
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type '{typeof(TRequest).FullName}'.");
        }

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse().ToList();

        RequestHandlerDelegate<TRequest, TResponse> pipeline = (req, ct) => handler.Handle(req, ct);

        foreach (var behavior in behaviors)
        {
            var currentNext = pipeline;
            var currentBehavior = behavior;
            pipeline = (req, ct) => currentBehavior.Handle(req, currentNext, ct);
        }

        return await pipeline(typedRequest, cancellationToken);
    }
}
