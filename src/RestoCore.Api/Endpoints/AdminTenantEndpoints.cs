namespace RestoCore.Api.Endpoints;

using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Tenants.Commands;

public static class AdminTenantEndpoints
{
    public static IEndpointRouteBuilder MapAdminTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/tenants")
            .RequireAuthorization("SuperAdminOnly")
            .WithTags("Platform SuperAdmin - Tenants");

        group.MapPost("/", async (ProvisionTenantCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/api/v1/admin/tenants/{result.Id}", result);
        })
        .WithName("ProvisionTenant")
        .Produces<TenantSummaryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);

        return app;
    }
}
