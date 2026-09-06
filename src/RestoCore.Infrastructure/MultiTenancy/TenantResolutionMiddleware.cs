namespace RestoCore.Infrastructure.MultiTenancy;

using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Common.Interfaces;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // 1. Path routing strategy: /api/v1/tenants/{slug}/... or /r/{slug}
        string? resolvedSlug = null;
        var segments = path.Trim('/').Split('/');
        
        if (segments.Length >= 4 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) 
                                 && segments[1].Equals("v1", StringComparison.OrdinalIgnoreCase) 
                                 && segments[2].Equals("tenants", StringComparison.OrdinalIgnoreCase))
        {
            resolvedSlug = segments[3];
        }
        else if (segments.Length >= 2 && segments[0].Equals("r", StringComparison.OrdinalIgnoreCase))
        {
            resolvedSlug = segments[1];
        }

        // 2. Subdomain strategy: {slug}.domain.com
        if (string.IsNullOrEmpty(resolvedSlug))
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            if (parts.Length >= 3 && !parts[0].Equals("api", StringComparison.OrdinalIgnoreCase) 
                                 && !parts[0].Equals("www", StringComparison.OrdinalIgnoreCase))
            {
                resolvedSlug = parts[0];
            }
        }

        // 3. JWT Claims strategy
        Guid? resolvedTenantId = null;
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(tenantClaim, out var claimId))
            {
                resolvedTenantId = claimId;
            }
        }

        // 4. Header strategy (X-Tenant-ID)
        if (!resolvedTenantId.HasValue && context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerVal))
        {
            if (Guid.TryParse(headerVal.FirstOrDefault(), out var parsedId))
            {
                resolvedTenantId = parsedId;
            }
        }

        if (resolvedTenantId.HasValue || !string.IsNullOrEmpty(resolvedSlug))
        {
            // Establish tenant context (if id is unknown from slug alone, deterministic lookup or synthetic resolution)
            var id = resolvedTenantId ?? Guid.Empty;
            var slug = resolvedSlug ?? string.Empty;
            tenantContext.SetTenant(id, slug);

            Activity.Current?.SetTag("tenant.id", id.ToString());
            if (!string.IsNullOrEmpty(slug))
            {
                Activity.Current?.SetTag("tenant.slug", slug);
            }
        }

        await _next(context);
    }
}
