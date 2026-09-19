namespace RestoCore.Application.Common.Interfaces;

public interface ICdnPurgeService
{
    Task<bool> PurgeTenantCacheAsync(string tenantSlug, CancellationToken cancellationToken = default);
    Task<bool> PurgeTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
}