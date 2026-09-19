namespace RestoCore.Api.Endpoints;

using Microsoft.AspNetCore.Authorization;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.MenuLayout.Commands;
using RestoCore.Application.Features.MenuLayout.DTOs;

public static class AdminMenuLayoutEndpoints
{
    public static IEndpointRouteBuilder MapAdminMenuLayoutEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/menu")
            .RequireAuthorization("TenantAdminOnly")
            .WithTags("Admin - Menu & Canvas Layout");

        group.MapPut("/layout", async (LayoutConfigDto layout, ISender sender) =>
        {
            var command = new UpdateMenuLayoutCommand(layout);
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateMenuLayout")
        .Produces<LayoutConfigDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/publish", async (MenuPublishRequest? request, ISender sender) =>
        {
            var command = new PublishMenuCommand(request);
            var result = await sender.Send(command);
            return Results.Accepted($"/api/v1/admin/menu/jobs/{result.JobId}", result);
        })
        .WithName("PublishMenu")
        .Produces<MenuPublishJobResponse>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}