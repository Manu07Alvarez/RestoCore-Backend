namespace RestoCore.Api.Endpoints;

using RestoCore.Application.Common.Mediator;
using Microsoft.AspNetCore.Mvc;
using RestoCore.Application.Features.Kitchen.Commands;

public static class KitchenEndpoints
{
    public static IEndpointRouteBuilder MapKitchenEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/kitchen")
            .RequireAuthorization("KitchenOnly")
            .WithTags("Kitchen Operations");

        group.MapPatch("/items/{id:guid}/availability", async (Guid id, [FromBody] ToggleAvailabilityRequest body, ISender sender) =>
        {
            var command = new ToggleItemAvailabilityCommand(id, body.IsAvailable);
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("ToggleItemAvailability")
        .Produces<ItemAvailabilityResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

public record ToggleAvailabilityRequest(bool IsAvailable);
