namespace RestoCore.UnitTests.Domain;

using FluentAssertions;
using RestoCore.Domain.Entities;
using Xunit;

public class ItemAvailabilityStateTests
{
    [Fact]
    public void NewMenuItem_DefaultsToAvailable()
    {
        var item = new MenuItem
        {
            TenantId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Milanesa Napolitana",
            BasePrice = 9500m
        };

        item.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void MenuItem_WhenPausedByKitchenStaff_BecomesUnavailable()
    {
        var item = new MenuItem
        {
            TenantId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Milanesa Napolitana",
            BasePrice = 9500m,
            IsAvailable = true
        };

        // Kitchen staff toggles off
        item.IsAvailable = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        item.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void MenuItem_WhenRestocked_BecomesAvailableAgain()
    {
        var item = new MenuItem
        {
            TenantId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Milanesa Napolitana",
            BasePrice = 9500m,
            IsAvailable = false
        };

        // Kitchen staff toggles back on
        item.IsAvailable = true;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        item.IsAvailable.Should().BeTrue();
    }
}
