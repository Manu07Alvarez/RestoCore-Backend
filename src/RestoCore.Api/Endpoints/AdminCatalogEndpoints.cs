namespace RestoCore.Api.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using RestoCore.Application.Features.Categories.Commands;
using RestoCore.Application.Features.MenuItems.Commands;
using RestoCore.Application.Features.Tenants.Commands;

public static class AdminCatalogEndpoints
{
    public static IEndpointRouteBuilder MapAdminCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin")
            .RequireAuthorization("OpaPolicy")
            .WithTags("Admin - Catalog");

        group.MapPost("/categories", async (CreateCategoryCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/api/v1/admin/categories/{result.Id}", result);
        })
        .WithName("CreateCategory")
        .Produces<CategoryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/items", async (CreateMenuItemCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/api/v1/admin/items/{result.Id}", result);
        })
        .WithName("CreateMenuItem")
        .Produces<MenuItemResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/items/{id:guid}/modifiers", async (Guid id, AddModifierGroupCommand command, ISender sender) =>
        {
            var actualCommand = command with { MenuItemId = id };
            var result = await sender.Send(actualCommand);
            return Results.Created($"/api/v1/admin/items/{id}/modifiers/{result.Id}", result);
        })
        .WithName("AddModifierGroup")
        .Produces<ModifierGroupResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/branding", async (UpdateBrandingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateBranding")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
