namespace RestoCore.Application.Common.Caching;

using System;
using System.Threading;
using System.Threading.Tasks;

public sealed record PublicMenuCacheEntry(string ETag, string PayloadJson);

public interface IPublicMenuCacheService
{
    Task<PublicMenuCacheEntry> GetOrCreateMenuAsync(
        string tenantSlug,
        string? tableToken,
        Func<CancellationToken, Task<PublicMenuCacheEntry>> factory,
        CancellationToken cancellationToken = default);

    Task InvalidateTenantMenuAsync(
        string tenantSlug,
        CancellationToken cancellationToken = default);
}
