namespace RestoCore.Infrastructure.Cdn;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Infrastructure.Telemetry;

public class LocalDevelopmentCdnPurgeService : ICdnPurgeService
{
    private readonly ILogger<LocalDevelopmentCdnPurgeService> _logger;

    public LocalDevelopmentCdnPurgeService(ILogger<LocalDevelopmentCdnPurgeService> logger)
    {
        _logger = logger;
    }

    public Task<bool> PurgeTenantCacheAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        using var activity = OpenTelemetryExtensions.ActivitySource.StartActivity("CdnPurge.Tenant");
        activity?.SetTag("tenant.slug", tenantSlug);

        try
        {
            _logger.LogInformation("CDN Cache Purge requested for tenant: {TenantSlug}", tenantSlug);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error.code", "CDN_PURGE_FAILURE");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "CDN purge failed for tenant {TenantSlug} with code CDN_PURGE_FAILURE", tenantSlug);
            throw;
        }
    }

    public Task<bool> PurgeTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        using var activity = OpenTelemetryExtensions.ActivitySource.StartActivity("CdnPurge.Tags");
        activity?.SetTag("cdn.tags", string.Join(",", tags));

        try
        {
            _logger.LogInformation("CDN Cache Purge requested for tags: {Tags}", string.Join(", ", tags));
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error.code", "CDN_PURGE_FAILURE");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "CDN purge failed for tags with code CDN_PURGE_FAILURE");
            throw;
        }
    }
}