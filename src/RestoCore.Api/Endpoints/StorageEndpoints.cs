namespace RestoCore.Api.Endpoints;

using RestoCore.Application.Common.Mediator;
using Microsoft.AspNetCore.Mvc;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Features.Storage.Commands;

public static class StorageEndpoints
{
    public static IEndpointRouteBuilder MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/images")
            .RequireAuthorization("TenantAdminOnly")
            .WithTags("Admin - Storage");

        group.MapPost("/presigned-url", async ([FromBody] GeneratePresignedUploadCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("GeneratePresignedUploadUrl")
        .Produces<PresignedUploadResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
