namespace RestoCore.Application.Common.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface IKitchenNotifier
{
    Task BroadcastOrderCreatedAsync(string tenantSlug, OrderCreatedNotification notification, CancellationToken ct = default);
}

public record OrderCreatedNotification(
    Guid OrderId,
    string TenantSlug,
    int TableNumber,
    string Status,
    decimal TotalAmount,
    string? Notes,
    DateTimeOffset CreatedAt
);
