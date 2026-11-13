namespace RestoCore.UnitTests.Infrastructure;

using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using RestoCore.Domain.Entities;
using Xunit;

public class PersistenceOptimizationTests
{
    [Fact]
    public void OrderItemSnapshot_And_OrderModifierSnapshot_ShouldSerializeWithCamelCaseKeys_ForPostgresJsonbContainment()
    {
        var menuItemId = Guid.NewGuid();
        var optionId = Guid.NewGuid();

        var snapshot = new List<OrderItemSnapshot>
        {
            new()
            {
                MenuItemId = menuItemId,
                Name = "Milanesa Napolitana",
                Quantity = 2,
                UnitPrice = 8500m,
                Instructions = "Sin sal",
                Modifiers = new List<OrderModifierSnapshot>
                {
                    new()
                    {
                        OptionId = optionId,
                        Name = "Papas Fritas",
                        PriceDelta = 1200m
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(snapshot);

        json.Should().Contain($"\"menuItemId\":\"{menuItemId}\"");
        json.Should().Contain("\"name\":\"Milanesa Napolitana\"");
        json.Should().Contain("\"quantity\":2");
        json.Should().Contain("\"unitPrice\":8500");
        json.Should().Contain("\"modifiers\":");
        json.Should().Contain($"\"optionId\":\"{optionId}\"");

        // Verify round-trip deserialization matches the Containment needle structure [{"menuItemId": "..."}]
        var deserialized = JsonSerializer.Deserialize<List<OrderItemSnapshot>>(json);
        deserialized.Should().NotBeNull();
        deserialized.Should().ContainSingle(x => x.MenuItemId == menuItemId && x.Quantity == 2);
        deserialized![0].Modifiers.Should().ContainSingle(m => m.OptionId == optionId);
    }
}
