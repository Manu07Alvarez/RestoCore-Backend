namespace RestoCore.Api.Endpoints;

using RestoCore.Application.Common.Mediator;
using Microsoft.AspNetCore.Authorization;
using RestoCore.Application.Features.Categories.Commands;
using RestoCore.Application.Features.MenuItems.Commands;
using RestoCore.Application.Features.Tenants.Commands;

public static class AdminCatalogEndpoints
{
    public static IEndpointRouteBuilder MapAdminCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin")
            .RequireAuthorization("TenantAdminOnly")
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

        group.MapPut("/items/{id:guid}", async (Guid id, UpdateMenuItemCommand command, ISender sender) =>
        {
            var actualCommand = command with { Id = id };
            var result = await sender.Send(actualCommand);
            return Results.Ok(result);
        })
        .WithName("UpdateMenuItem")
        .Produces<MenuItemResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPatch("/items/{id:guid}", async (Guid id, PatchMenuItemCommand command, ISender sender) =>
        {
            var actualCommand = command with { Id = id };
            var result = await sender.Send(actualCommand);
            return Results.Ok(result);
        })
        .WithName("PatchMenuItem")
        .Produces<MenuItemResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/items/{id:guid}", async (Guid id, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(new DeleteMenuItemCommand(id));
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { detail = ex.Message });
            }
        })
        .WithName("DeleteMenuItem")
        .Produces<RestoCore.Application.Features.MenuItems.DTOs.MenuItemArchiveResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/items/{id:guid}/restore", async (Guid id, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(new RestoreMenuItemCommand(id));
                return Results.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { detail = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { detail = ex.Message });
            }
        })
        .WithName("RestoreMenuItem")
        .Produces<MenuItemResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
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
