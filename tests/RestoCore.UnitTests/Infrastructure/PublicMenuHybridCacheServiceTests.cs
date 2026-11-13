namespace RestoCore.UnitTests.Infrastructure;

using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestoCore.Application.Common.Caching;
using RestoCore.Infrastructure.Caching;
using StackExchange.Redis;
using Xunit;

public class PublicMenuHybridCacheServiceTests
{
    [Fact]
    public async Task GetOrCreateMenuAsync_ShouldServeFromL1OnSecondCallWithoutHittingRedis()
    {
        // Arrange
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var distributedCacheMock = new Mock<IDistributedCache>();
        var service = new PublicMenuHybridCacheService(
            memoryCache,
            distributedCacheMock.Object,
            redis: null,
            NullLogger<PublicMenuHybridCacheService>.Instance);

        int factoryInvocations = 0;

        // Act
        var first = await service.GetOrCreateMenuAsync(
            "don-pepe",
            "mesa-1",
            _ =>
            {
                factoryInvocations++;
                return Task.FromResult(new PublicMenuCacheEntry("\"etag-1\"", "{\"tenantName\":\"Don Pepe\"}"));
            });

        var second = await service.GetOrCreateMenuAsync(
            "don-pepe",
            "mesa-1",
            _ =>
            {
                factoryInvocations++;
                return Task.FromResult(new PublicMenuCacheEntry("\"etag-2\"", "{\"tenantName\":\"Other\"}"));
            });

        // Assert
        factoryInvocations.Should().Be(1);
        first.ETag.Should().Be("\"etag-1\"");
        second.ETag.Should().Be("\"etag-1\"");
        distributedCacheMock.Verify(
            d => d.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrCreateMenuAsync_ShouldPopulateL1FromL2WhenL1Misses()
    {
        // Arrange
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var distributedCacheMock = new Mock<IDistributedCache>();
        var l2Entry = new PublicMenuCacheEntry("\"etag-l2\"", "{\"tenantName\":\"FromRedis\"}");
        var serialized = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(l2Entry));

        distributedCacheMock
            .Setup(d => d.GetAsync(It.Is<string>(k => k.Contains("don-pepe") && k.Contains("vip")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serialized);

        var service = new PublicMenuHybridCacheService(
            memoryCache,
            distributedCacheMock.Object,
            redis: null,
            NullLogger<PublicMenuHybridCacheService>.Instance);

        bool factoryCalled = false;

        // Act
        var entry = await service.GetOrCreateMenuAsync(
            "don-pepe",
            "vip",
            _ =>
            {
                factoryCalled = true;
                return Task.FromResult(new PublicMenuCacheEntry("\"etag-db\"", "{}"));
            });

        // Assert
        factoryCalled.Should().BeFalse();
        entry.ETag.Should().Be("\"etag-l2\"");
        entry.PayloadJson.Should().Contain("FromRedis");
    }

    [Fact]
    public async Task InvalidateTenantMenuAsync_ShouldEvictL1AndPublishRedisInvalidation()
    {
        // Arrange
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var distributedCacheMock = new Mock<IDistributedCache>();
        var redisMock = new Mock<IConnectionMultiplexer>();
        var subscriberMock = new Mock<ISubscriber>();
        redisMock.Setup(r => r.IsConnected).Returns(true);
        redisMock.Setup(r => r.GetSubscriber(It.IsAny<object>())).Returns(subscriberMock.Object);

        var service = new PublicMenuHybridCacheService(
            memoryCache,
            distributedCacheMock.Object,
            redisMock.Object,
            NullLogger<PublicMenuHybridCacheService>.Instance);

        int factoryInvocations = 0;

        await service.GetOrCreateMenuAsync(
            "don-pepe",
            "default",
            _ =>
            {
                factoryInvocations++;
                return Task.FromResult(new PublicMenuCacheEntry($"\"etag-{factoryInvocations}\"", "{}"));
            });

        // Act
        await service.InvalidateTenantMenuAsync("don-pepe");

        var refreshed = await service.GetOrCreateMenuAsync(
            "don-pepe",
            "default",
            _ =>
            {
                factoryInvocations++;
                return Task.FromResult(new PublicMenuCacheEntry($"\"etag-{factoryInvocations}\"", "{}"));
            });

        // Assert
        factoryInvocations.Should().Be(2);
        refreshed.ETag.Should().Be("\"etag-2\"");
        subscriberMock.Verify(
            s => s.PublishAsync(
                It.Is<RedisChannel>(c => c.ToString().Contains("restocore:hybridcache:invalidate")),
                It.Is<RedisValue>(v => v.ToString() == "don-pepe"),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }
}
