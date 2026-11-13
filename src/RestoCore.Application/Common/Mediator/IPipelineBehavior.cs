namespace RestoCore.Application.Common.Mediator;

using System.Threading;
using System.Threading.Tasks;

public delegate Task<TResponse> RequestHandlerDelegate<in TRequest, TResponse>(
    TRequest request,
    CancellationToken cancellationToken);

public interface IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken);
}
