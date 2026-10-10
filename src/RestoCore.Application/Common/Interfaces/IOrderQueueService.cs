namespace RestoCore.Application.Common.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IOrderQueueService
{
    Task<string> EnqueueOrderAsync(string tenantSlug, OrderIngestionPayload payload, CancellationToken ct = default);
    Task<OrderStreamMessage?> DequeueNextOrderAsync(string tenantSlug, string consumerGroup, string consumerName, CancellationToken ct = default);
    Task AcknowledgeOrderAsync(string tenantSlug, string consumerGroup, string messageId, CancellationToken ct = default);
}

public record OrderIngestionPayload(
    Guid OrderId,
    Guid TenantId,
    Guid TableId,
    int TableNumber,
    string? Notes,
    List<OrderItemInput> Items
);

public record OrderItemInput(
    Guid MenuItemId,
    int Quantity,
    string? Instructions,
    List<Guid> SelectedOptionIds
);

public record OrderStreamMessage(
    string MessageId,
    OrderIngestionPayload Payload
);
