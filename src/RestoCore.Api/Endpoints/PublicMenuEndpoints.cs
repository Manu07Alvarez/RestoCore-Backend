namespace RestoCore.Api.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using RestoCore.Application.Features.PublicMenu.Queries;

public static class PublicMenuEndpoints
{
    public static void MapPublicMenuEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tenants/{tenant_slug}/menu")
                       .WithTags("Public Menu");

        group.MapGet("/", async (string tenant_slug, string? table_token, IMediator mediator, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache, HttpContext context, CancellationToken ct) =>
        {
            var cacheKey = $"menu_etag:{tenant_slug}:{table_token ?? "default"}";

            // If client sends If-None-Match, check cached ETag in Redis first for sub-5ms response
            if (context.Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch))
            {
                var cachedEtag = await cache.GetStringAsync(cacheKey, ct);
                if (!string.IsNullOrEmpty(cachedEtag) && cachedEtag == ifNoneMatch)
                {
                    return Results.StatusCode(StatusCodes.Status304NotModified);
                }
            }

            var (response, etag) = await mediator.Send(new GetPublicMenuQuery(tenant_slug, table_token), ct);

            // Cache computed ETag in Redis with 60s TTL
            await cache.SetStringAsync(cacheKey, etag, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            }, ct);

            if (context.Request.Headers.TryGetValue("If-None-Match", out var clientEtag) && clientEtag == etag)
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
