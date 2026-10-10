# Data Model & Interface Contracts: Asynchronous Order Processing & WebSockets

## 1. Domain Entities & Value Objects

### 1.1 `Order` Entity
Location: `src/RestoCore.Domain/Entities/Order.cs`

```csharp
namespace RestoCore.Domain.Entities;

using System;
using System.Collections.Generic;

public class Order
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TableId { get; set; }
    public int TableNumber { get; set; }
    public string Status { get; set; } = "queued"; // queued, received, preparing, ready, delivered, cancelled
    public decimal TotalAmount { get; set; }
    public string? CustomerNotes { get; set; }
    public List<OrderItemSnapshot> ItemsSnapshot { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public virtual Tenant? Tenant { get; set; }
    public virtual Table? Table { get; set; }
}

public class OrderItemSnapshot
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public List<OrderModifierSnapshot> Modifiers { get; set; } = new();
    public string? Instructions { get; set; }
}

public class OrderModifierSnapshot
{
    public Guid OptionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
}
```

---

## 2. Contracts & Services

### 2.1 `IOrderQueueService`
Location: `src/RestoCore.Application/Common/Interfaces/IOrderQueueService.cs`

```csharp
namespace RestoCore.Application.Common.Interfaces;

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
```

### 2.2 `IKitchenNotifier` (WebSocket Connection Registry)
Location: `src/RestoCore.Application/Common/Interfaces/IKitchenNotifier.cs`

```csharp
namespace RestoCore.Application.Common.Interfaces;

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
```

### 2.3 `IOrderRepository`
Location: `src/RestoCore.Application/Common/Interfaces/Repositories/IOrderRepository.cs`

```csharp
namespace RestoCore.Application.Common.Interfaces.Repositories;

using RestoCore.Domain.Entities;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    Task<List<Order>> GetActiveOrdersByTenantAsync(Guid tenantId, CancellationToken ct = default);
    void AddOrder(Order order);
    void UpdateOrderStatus(Order order, string status);
}
```
