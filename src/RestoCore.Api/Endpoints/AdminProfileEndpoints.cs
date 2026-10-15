namespace RestoCore.Api.Endpoints;

using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Tenants.Commands;
using RestoCore.Application.Features.Tenants.DTOs;
using RestoCore.Application.Features.Tenants.Queries;

public static class AdminProfileEndpoints
{
    public static IEndpointRouteBuilder MapAdminProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/tenant")
            .RequireAuthorization("TenantAdminOnly")
            .WithTags("Admin - Restaurant Profile");

        group.MapGet("/profile", async (ISender sender) =>
        {
            var result = await sender.Send(new GetTenantProfileQuery());
            return Results.Ok(result);
        })
        .WithName("GetTenantProfile")
        .Produces<TenantProfileResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/profile", async (UpdateTenantProfileCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateTenantProfile")
        .Produces<TenantProfileResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPatch("/profile", async (PatchTenantProfileCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("PatchTenantProfile")
        .Produces<TenantProfileResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
