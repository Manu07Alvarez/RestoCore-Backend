namespace RestoCore.Infrastructure.MultiTenancy;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware>? logger = null)
    {
        _next = next;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantResolutionMiddleware>.Instance;
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
            var id = resolvedTenantId ?? Guid.Empty;
            var slug = resolvedSlug ?? string.Empty;
            tenantContext.SetTenant(id, slug);
            context.Items["TenantId"] = id;
            context.Items["TenantSlug"] = slug;

            Activity.Current?.SetTag("tenant.id", id.ToString());
            if (!string.IsNullOrEmpty(slug))
            {
                Activity.Current?.SetTag("tenant.slug", slug);
            }

            // ADR-0017 (OPT-07): Structured logger scope correlated with OpenTelemetry trace/span
            var scopeState = new Dictionary<string, object>
            {
                ["tenant_id"] = id,
                ["tenant_slug"] = slug
            };

            using (_logger.BeginScope(scopeState))
            {
                await _next(context);
            }
            return;
        }

        await _next(context);
    }
}
