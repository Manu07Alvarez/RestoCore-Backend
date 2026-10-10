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
