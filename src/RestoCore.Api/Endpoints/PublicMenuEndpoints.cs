namespace RestoCore.Api.Endpoints;

using RestoCore.Application.Common.Mediator;
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
            var versionCacheKey = $"menu_version:{tenant_slug}";
            string version = "1";
            string? cachedEtag = null;
            string? cachedPayload = null;

            try
            {
                version = await cache.GetStringAsync(versionCacheKey, ct) ?? "1";
                var etagCacheKey = $"menu_etag:{tenant_slug}:{version}:{table_token ?? "default"}";
                var payloadCacheKey = $"menu_payload:{tenant_slug}:{version}:{table_token ?? "default"}";

                cachedEtag = await cache.GetStringAsync(etagCacheKey, ct);
                cachedPayload = await cache.GetStringAsync(payloadCacheKey, ct);
            }
            catch
            {
                // Resilient fallback: continue with dynamic mediator execution
            }

            if (!string.IsNullOrEmpty(cachedEtag) && !string.IsNullOrEmpty(cachedPayload))
            {
                if (context.Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && ifNoneMatch == cachedEtag)
                {
                    return Results.StatusCode(StatusCodes.Status304NotModified);
                }

                context.Response.Headers.ETag = cachedEtag;
                context.Response.Headers.CacheControl = "public, max-age=60, stale-while-revalidate=300";
                return Results.Content(cachedPayload, "application/json");
            }

            var (response, etag) = await mediator.Send(new GetPublicMenuQuery(tenant_slug, table_token), ct);
            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

            try
            {
                var etagCacheKey = $"menu_etag:{tenant_slug}:{version}:{table_token ?? "default"}";
                var payloadCacheKey = $"menu_payload:{tenant_slug}:{version}:{table_token ?? "default"}";

                // Cache computed ETag and serialized JSON payload in Redis with 60s TTL
                var cacheOptions = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                await cache.SetStringAsync(etagCacheKey, etag, cacheOptions, ct);
                await cache.SetStringAsync(payloadCacheKey, jsonPayload, cacheOptions, ct);
            }
            catch
            {
                // Non-blocking cache write failure
            }

            if (context.Request.Headers.TryGetValue("If-None-Match", out var incomingEtag) && incomingEtag == etag)
            {
                return Results.StatusCode(StatusCodes.Status304NotModified);
            }

            context.Response.Headers.ETag = etag;
            context.Response.Headers.CacheControl = "public, max-age=60, stale-while-revalidate=300";

            return Results.Content(jsonPayload, "application/json");
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
