namespace RestoCore.Infrastructure.Caching;

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Caching;
using StackExchange.Redis;

public sealed class PublicMenuHybridCacheService : IPublicMenuCacheService
{
    public const string InvalidationChannelName = "restocore:hybridcache:invalidate";
    private static readonly TimeSpan L1Expiration = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan L2Expiration = TimeSpan.FromHours(2);

    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<PublicMenuHybridCacheService> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _tenantKeys = new(StringComparer.OrdinalIgnoreCase);

    public PublicMenuHybridCacheService(
        IMemoryCache memoryCache,
        IDistributedCache? distributedCache = null,
        IConnectionMultiplexer? redis = null,
        ILogger<PublicMenuHybridCacheService>? logger = null)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _redis = redis;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PublicMenuHybridCacheService>.Instance;

        SubscribeToInvalidationChannel();
    }

    public async Task<PublicMenuCacheEntry> GetOrCreateMenuAsync(
        string tenantSlug,
        string? tableToken,
        Func<CancellationToken, Task<PublicMenuCacheEntry>> factory,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = tenantSlug.Trim().ToLowerInvariant();
        var normalizedToken = string.IsNullOrWhiteSpace(tableToken) ? "default" : tableToken.Trim();
        var cacheKey = BuildCacheKey(normalizedSlug, normalizedToken);

        // 1. Fast L1 In-Memory lookup (< 0.5 ms)
        if (_memoryCache.TryGetValue(cacheKey, out PublicMenuCacheEntry? cachedEntry) && cachedEntry != null)
        {
            return cachedEntry;
        }

        var keyLock = _keyLocks.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check L1 after acquiring stampede lock
            if (_memoryCache.TryGetValue(cacheKey, out cachedEntry) && cachedEntry != null)
            {
                return cachedEntry;
            }

            // 2. L2 Distributed Redis lookup (single atomic key)
            if (_distributedCache != null)
            {
                try
                {
                    var bytes = await _distributedCache.GetAsync(cacheKey, cancellationToken);
                    if (bytes is { Length: > 0 })
                    {
                        var l2Entry = JsonSerializer.Deserialize<PublicMenuCacheEntry>(bytes);
                        if (l2Entry != null)
                        {
                            StoreInL1(normalizedSlug, cacheKey, l2Entry);
                            return l2Entry;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "L2 Redis read failed for key {CacheKey}; falling back to source factory.", cacheKey);
                }
            }

            // 3. Cache miss: execute factory once and populate L1 + L2
            var createdEntry = await factory(cancellationToken);
            StoreInL1(normalizedSlug, cacheKey, createdEntry);

            if (_distributedCache != null)
            {
                try
                {
                    var serialized = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(createdEntry));
                    await _distributedCache.SetAsync(
                        cacheKey,
                        serialized,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = L2Expiration
                        },
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "L2 Redis write failed for key {CacheKey}.", cacheKey);
                }
            }

            return createdEntry;
        }
        finally
        {
            keyLock.Release();
        }
    }

    public async Task InvalidateTenantMenuAsync(
        string tenantSlug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantSlug))
        {
            return;
        }

        var normalizedSlug = tenantSlug.Trim().ToLowerInvariant();
        var keysToRemove = EvictLocalTenantKeys(normalizedSlug);

        if (_distributedCache != null)
        {
            try
            {
                var defaultKey = BuildCacheKey(normalizedSlug, "default");
                await _distributedCache.RemoveAsync(defaultKey, cancellationToken);

                foreach (var key in keysToRemove)
                {
                    if (!string.Equals(key, defaultKey, StringComparison.OrdinalIgnoreCase))
                    {
                        await _distributedCache.RemoveAsync(key, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove L2 cache keys for tenant {TenantSlug}.", normalizedSlug);
            }
        }

        if (_redis is { IsConnected: true })
        {
            try
            {
                var subscriber = _redis.GetSubscriber();
                await subscriber.PublishAsync(
                    RedisChannel.Literal(InvalidationChannelName),
                    normalizedSlug);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish hybrid cache invalidation over Redis Pub/Sub for tenant {TenantSlug}.", normalizedSlug);
            }
        }
    }

    private void StoreInL1(string normalizedSlug, string cacheKey, PublicMenuCacheEntry entry)
    {
        _memoryCache.Set(cacheKey, entry, L1Expiration);
        var set = _tenantKeys.GetOrAdd(normalizedSlug, static _ => new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase));
        set.TryAdd(cacheKey, 0);
    }

    private string[] EvictLocalTenantKeys(string normalizedSlug)
    {
        var defaultKey = BuildCacheKey(normalizedSlug, "default");
        _memoryCache.Remove(defaultKey);

        if (_tenantKeys.TryRemove(normalizedSlug, out var keys))
        {
            var snapshot = keys.Keys.ToArray();
            foreach (var key in snapshot)
            {
                _memoryCache.Remove(key);
            }
            return snapshot;
        }

        return new[] { defaultKey };
    }

    private void SubscribeToInvalidationChannel()
    {
        if (_redis is not { IsConnected: true })
        {
            return;
        }

        try
        {
            var subscriber = _redis.GetSubscriber();
            subscriber.Subscribe(
                RedisChannel.Literal(InvalidationChannelName),
                (_, value) =>
                {
                    if (!value.IsNullOrEmpty)
                    {
                        EvictLocalTenantKeys(value.ToString().Trim().ToLowerInvariant());
                    }
                });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to subscribe to Redis Pub/Sub invalidation channel {Channel}.", InvalidationChannelName);
        }
    }

    public static string BuildCacheKey(string tenantSlug, string tableToken)
        => $"menu:public:{tenantSlug.Trim().ToLowerInvariant()}:{tableToken}";
}
