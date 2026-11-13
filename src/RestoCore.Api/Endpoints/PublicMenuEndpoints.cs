namespace RestoCore.Api.Endpoints;

using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RestoCore.Application.Common.Caching;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.PublicMenu.Queries;

public static class PublicMenuEndpoints
{
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

    public static void MapPublicMenuEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tenants/{tenant_slug}/menu")
                       .WithTags("Public Menu");

        group.MapGet("/", async (
            string tenant_slug,
            string? table_token,
            IMediator mediator,
            IPublicMenuCacheService publicMenuCache,
            HttpContext context,
            CancellationToken ct) =>
        {
            var entry = await publicMenuCache.GetOrCreateMenuAsync(
                tenant_slug,
                table_token,
                async cancel =>
                {
                    var (response, etag) = await mediator.Send(new GetPublicMenuQuery(tenant_slug, table_token), cancel);
                    var jsonPayload = JsonSerializer.Serialize(response, WebJsonOptions);
                    return new PublicMenuCacheEntry(etag, jsonPayload);
                },
                ct);

            if (context.Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) &&
                MatchesEtag(ifNoneMatch, entry.ETag))
            {
                context.Response.Headers.ETag = entry.ETag;
                context.Response.Headers.CacheControl = "public, max-age=60, s-maxage=3600";
                return Results.StatusCode(StatusCodes.Status304NotModified);
            }

            context.Response.Headers.ETag = entry.ETag;
            context.Response.Headers.CacheControl = "public, max-age=60, s-maxage=3600";

            return Results.Content(entry.PayloadJson, "application/json");
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

    private static bool MatchesEtag(string? ifNoneMatchHeader, string currentEtag)
    {
        if (string.IsNullOrWhiteSpace(ifNoneMatchHeader)) return false;
        if (ifNoneMatchHeader == "*") return true;
        if (string.Equals(ifNoneMatchHeader, currentEtag, StringComparison.Ordinal)) return true;

        var cleanCurrent = currentEtag.Trim('\"');
        var tokens = ifNoneMatchHeader.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            var cleanToken = token.StartsWith("W/", StringComparison.OrdinalIgnoreCase)
                ? token[2..].Trim('\"')
                : token.Trim('\"');

            if (string.Equals(cleanToken, cleanCurrent, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }
}
