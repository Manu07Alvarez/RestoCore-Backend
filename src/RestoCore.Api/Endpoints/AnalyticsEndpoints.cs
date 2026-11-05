namespace RestoCore.Api.Endpoints;

using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Analytics.Commands;
using RestoCore.Application.Features.Analytics.DTOs;
using RestoCore.Application.Features.Analytics.Queries;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/analytics/collect", async (AnalyticsEventRequest request, ISender sender, CancellationToken ct) =>
        {
            try
            {
                await sender.Send(new CollectAnalyticsEventCommand(
                    request.EventType,
                    request.TenantSlug,
                    request.ItemId,
                    request.CategoryId,
                    request.DeviceType
                ), ct);

                return Results.Accepted();
            }
            catch (ArgumentException ex)
            {
                return Results.Problem(
                    title: "Invalid Analytics Event",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.Problem(
                    title: "Tenant Not Found",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }
        })
        .AllowAnonymous()
        .WithTags("Analytics - Ingestion")
        .WithName("collectAnalyticsEvent")
        .Produces(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        var group = app.MapGroup("/api/v1/admin/analytics")
            .RequireAuthorization("AnalyticsReadOnly")
            .WithTags("Admin - Analytics");

        group.MapGet("/dashboard", async (string? from, string? to, ISender sender, CancellationToken ct) =>
        {
            try
            {
                var result = await sender.Send(new GetAnalyticsDashboardQuery(from, to), ct);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.Problem(
                    title: "Invalid Date Range",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }
        })
        .WithName("getAnalyticsDashboard")
        .Produces<AnalyticsDashboardResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }
}
