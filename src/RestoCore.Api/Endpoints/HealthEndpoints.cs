namespace RestoCore.Api.Endpoints;

using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/healthz", () => Results.Ok(new
        {
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow
        })).AllowAnonymous().WithTags("Health");

        app.MapGet("/ready", async (IApplicationDbContext context) =>
        {
            try
            {
                var canConnect = await context.Tenants.AnyAsync();
                return Results.Ok(new
                {
                    status = "Ready",
                    database = "Connected",
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    status = "Unhealthy",
                    database = "Disconnected",
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }).AllowAnonymous().WithTags("Health");

        return app;
    }
}
