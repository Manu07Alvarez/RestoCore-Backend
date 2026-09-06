namespace RestoCore.Api.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using RestoCore.Application.Features.QrCodes.Queries;

public static class AdminTableEndpoints
{
    public static IEndpointRouteBuilder MapAdminTableEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/tables")
            .RequireAuthorization("TenantAdminOnly")
            .WithTags("Admin - Tables & QR");

        group.MapGet("/{id:guid}/qr", async (
            Guid id,
            [FromQuery] string? format,
            HttpContext httpContext,
            ISender sender) =>
        {
            var requestedFormat = format?.ToLowerInvariant() == "png" ? "png" : "svg";
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            var result = await sender.Send(new GetTableQrQuery(id, requestedFormat, baseUrl));
            return Results.File(result.Data, result.ContentType);
        })
        .WithName("GetTableQrCode")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
