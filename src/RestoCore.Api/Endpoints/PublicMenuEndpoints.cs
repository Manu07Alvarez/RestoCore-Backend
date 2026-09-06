namespace RestoCore.Api.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RestoCore.Application.Features.PublicMenu.Queries;

public static class PublicMenuEndpoints
{
    public static void MapPublicMenuEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tenants/{tenant_slug}/menu")
                       .WithTags("Public Menu");

        group.MapGet("/", async (string tenant_slug, string? table_token, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var (response, etag) = await mediator.Send(new GetPublicMenuQuery(tenant_slug, table_token), ct);

            // Conditional HTTP GET caching
            if (context.Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && ifNoneMatch == etag)
            {
                return Results.StatusCode(StatusCodes.Status304NotModified);
            }

            context.Response.Headers.ETag = etag;
            context.Response.Headers.CacheControl = "public, max-age=60, stale-while-revalidate=300";

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetPublicMenu")
        .WithSummary("Retrieve complete public digital menu for a restaurant");

        // Alias for QR short URLs: /r/{tenant_slug}
        app.MapGet("/r/{tenant_slug}", (string tenant_slug, string? table_token) =>
        {
            var target = $"/api/v1/tenants/{tenant_slug}/menu";
            if (!string.IsNullOrEmpty(table_token))
            {
                target += $"?table_token={Uri.EscapeDataString(table_token)}";
            }
            return Results.Redirect(target);
        }).AllowAnonymous();
    }
}
