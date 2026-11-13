namespace RestoCore.Infrastructure.Messaging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;
using StackExchange.Redis;

public class RedisOrderQueueService : IOrderQueueService
{
    public const string ActiveTenantStreamsKey = "restocore:active_tenant_streams";

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisOrderQueueService> _logger;

    public RedisOrderQueueService(
        IConnectionMultiplexer redis,
        ILogger<RedisOrderQueueService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<string> EnqueueOrderAsync(string tenantSlug, OrderIngestionPayload payload, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var normalizedSlug = tenantSlug.Trim();
        var streamKey = $"orders:{normalizedSlug}";
        var jsonPayload = JsonSerializer.Serialize(payload);

        // ADR-0018 (OPT-05a): Register active tenant stream in Redis Set so worker does not poll PostgreSQL
        await db.SetAddAsync(ActiveTenantStreamsKey, normalizedSlug);

        var entryId = await db.StreamAddAsync(streamKey, new NameValueEntry[]
        {
            new("order_id", payload.OrderId.ToString()),
            new("payload", jsonPayload)
        });

        _logger.LogInformation("Enqueued order {OrderId} to stream {StreamKey} with ID {EntryId}",
            payload.OrderId, streamKey, entryId.ToString());

        return entryId.ToString();
    }

    public async Task<IReadOnlyCollection<string>> GetActiveTenantStreamsAsync(CancellationToken ct = default)
    {
        try
        {
            if (!_redis.IsConnected)
            {
                return Array.Empty<string>();
            }

            var db = _redis.GetDatabase();
            var members = await db.SetMembersAsync(ActiveTenantStreamsKey);
            if (members.Length == 0)
            {
                return Array.Empty<string>();
            }

            return members
                .Where(m => !m.IsNullOrEmpty)
                .Select(m => m.ToString())
                .ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read active tenant streams from Redis Set {Key}", ActiveTenantStreamsKey);
            return Array.Empty<string>();
        }
    }

    public async Task<OrderStreamMessage?> DequeueNextOrderAsync(string tenantSlug, string consumerGroup, string consumerName, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var streamKey = $"orders:{tenantSlug}";

        try
        {
            if (!await db.KeyExistsAsync(streamKey))
            {
                return null;
            }

            try
            {
                await db.StreamCreateConsumerGroupAsync(streamKey, consumerGroup, "0-0", createStream: true);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
            {
                // Group already exists
            }

            var entries = await db.StreamReadGroupAsync(streamKey, consumerGroup, consumerName, ">", count: 1);
            if (entries.Length == 0)
            {
                return null;
            }

            var entry = entries[0];
            var payloadJson = entry["payload"].ToString();
            if (string.IsNullOrEmpty(payloadJson))
            {
                return null;
            }

            var payload = JsonSerializer.Deserialize<OrderIngestionPayload>(payloadJson);
            if (payload == null)
            {
                return null;
            }

            return new OrderStreamMessage(entry.Id.ToString(), payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from stream {StreamKey}", streamKey);
            return null;
        }
    }

    public async Task AcknowledgeOrderAsync(string tenantSlug, string consumerGroup, string messageId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var streamKey = $"orders:{tenantSlug}";
        await db.StreamAcknowledgeAsync(streamKey, consumerGroup, messageId);
    }
}
