namespace RestoCore.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
    [JsonPropertyName("menuItemId")]
    public Guid MenuItemId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("modifiers")]
    public List<OrderModifierSnapshot> Modifiers { get; set; } = new();

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }
}

public class OrderModifierSnapshot
{
    [JsonPropertyName("optionId")]
    public Guid OptionId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("priceDelta")]
    public decimal PriceDelta { get; set; }
}
