namespace RestoCore.Api.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Orders.Commands;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders");

        group.MapPost("", async (
            SubmitOrderCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var response = await mediator.Send(command, ct);
            return Results.Accepted($"/api/v1/orders/{response.OrderId}", response);
        })
        .WithName("SubmitOrder")
        .WithSummary("Ingest a new order asynchronously (HTTP 202)")
        .WithDescription("Appends order to tenant's Redis FIFO stream for sequential processing.")
        .Produces<SubmitOrderResponse>(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
