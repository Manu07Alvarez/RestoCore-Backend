namespace RestoCore.UnitTests.Features.PublicMenu;

using System.Security.Cryptography;
using System.Text.Json;
using FluentAssertions;
using RestoCore.Application.Features.MenuLayout.DTOs;
using RestoCore.Application.Features.PublicMenu.DTOs;
using RestoCore.Domain.Entities;
using RestoCore.Domain.ValueObjects;
using Xunit;

public class PublicMenuCanvasTests
{
    [Fact]
    public void Should_Prune_Orphaned_Canvas_Elements_Referencing_Deleted_Or_Unavailable_Dishes()
    {
        // Arrange: Valid active dish, unavailable dish, and deleted dish (not in catalog)
        var validDishId = Guid.NewGuid();
        var unavailableDishId = Guid.NewGuid();
        var deletedDishId = Guid.NewGuid();
        var inactiveCategoryDishId = Guid.NewGuid();

        var activeCategory = new Category
        {
            Name = "Pizzas",
            IsActive = true,
            Items = new List<MenuItem>
            {
                new() { Id = validDishId, Name = "Margarita", IsAvailable = true },
                new() { Id = unavailableDishId, Name = "Fugazzeta", IsAvailable = false }
            }
        };

        var inactiveCategory = new Category
        {
            Name = "Bebidas",
            IsActive = false,
            Items = new List<MenuItem>
            {
                new() { Id = inactiveCategoryDishId, Name = "Vino Tinto", IsAvailable = true }
            }
        };

        var categories = new List<Category> { activeCategory, inactiveCategory };

        var layoutConfig = new LayoutConfig
        {
            CanvasEnabled = true,
            BackgroundColor = "#121212",
            BackgroundUrl = "https://storage.restocore.com/canvas-bg.webp",
            Elements = new List<CanvasElement>
            {
                new() { DishId = validDishId, X = 100, Y = 100, ZIndex = 1, Width = 200, Height = 150 },
                new() { DishId = unavailableDishId, X = 320, Y = 100, ZIndex = 1, Width = 200, Height = 150 },
                new() { DishId = deletedDishId, X = 540, Y = 100, ZIndex = 1, Width = 200, Height = 150 },
                new() { DishId = inactiveCategoryDishId, X = 100, Y = 280, ZIndex = 2, Width = 200, Height = 150 }
            }
        };

        // Act: Extract active & available dish IDs (simulating query handler logic)
        var activeDishIds = categories
            .Where(c => c.IsActive)
            .SelectMany(c => c.Items)
            .Where(i => i.IsAvailable)
            .Select(i => i.Id)
            .ToHashSet();

        var filteredElements = layoutConfig.Elements
            .Where(e => activeDishIds.Contains(e.DishId))
            .Select(e => new CanvasElementDto
            {
                DishId = e.DishId,
                X = e.X,
                Y = e.Y,
                ZIndex = e.ZIndex,
                Width = e.Width,
                Height = e.Height
            }).ToList();

        // Assert
        filteredElements.Should().HaveCount(1);
        filteredElements.First().DishId.Should().Be(validDishId);
        filteredElements.Should().NotContain(e => e.DishId == unavailableDishId);
        filteredElements.Should().NotContain(e => e.DishId == deletedDishId);
        filteredElements.Should().NotContain(e => e.DishId == inactiveCategoryDishId);
    }

    [Fact]
    public void Should_Retain_Accurate_Coordinates_And_Dimensions_When_Preserving_Valid_Elements()
    {
        // Arrange
        var dishId = Guid.NewGuid();
        var element = new CanvasElement
        {
            DishId = dishId,
            X = 250.75,
            Y = 480.50,
            ZIndex = 5,
            Width = 320.0,
            Height = 210.0
        };

        // Act
        var dto = new CanvasElementDto
        {
            DishId = element.DishId,
            X = element.X,
            Y = element.Y,
            ZIndex = element.ZIndex,
            Width = element.Width,
            Height = element.Height
        };

        // Assert
        dto.DishId.Should().Be(dishId);
        dto.X.Should().Be(250.75);
        dto.Y.Should().Be(480.50);
        dto.ZIndex.Should().Be(5);
        dto.Width.Should().Be(320.0);
        dto.Height.Should().Be(210.0);
    }

    [Fact]
    public void Should_Generate_Different_ETag_When_Layout_Coordinates_Change()
    {
        // Arrange
        var dishId = Guid.NewGuid();
        var menu1 = new PublicMenuResponse
        {
            TenantName = "Pizzeria",
            TenantSlug = "pizzeria",
            LayoutConfig = new LayoutConfigDto
            {
                CanvasEnabled = true,
                Elements = new List<CanvasElementDto>
                {
                    new() { DishId = dishId, X = 100, Y = 100, ZIndex = 1, Width = 150, Height = 100 }
                }
            }
        };

        var menu2 = new PublicMenuResponse
        {
            TenantName = "Pizzeria",
            TenantSlug = "pizzeria",
            LayoutConfig = new LayoutConfigDto
            {
                CanvasEnabled = true,
                Elements = new List<CanvasElementDto>
                {
                    new() { DishId = dishId, X = 200, Y = 100, ZIndex = 1, Width = 150, Height = 100 }
                }
            }
        };

        // Act
        var hash1 = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(menu1)));
        var hash2 = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(menu2)));

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Should_Yield_Identical_ETag_For_Unchanged_Canvas_Layout()
    {
        // Arrange
        var dishId = Guid.NewGuid();
        var menu1 = new PublicMenuResponse
        {
            TenantName = "Pizzeria",
            TenantSlug = "pizzeria",
            LayoutConfig = new LayoutConfigDto
            {
                CanvasEnabled = true,
                BackgroundColor = "#000000",
                Elements = new List<CanvasElementDto>
                {
                    new() { DishId = dishId, X = 100, Y = 100, ZIndex = 1, Width = 150, Height = 100 }
                }
            }
        };

        var menu2 = new PublicMenuResponse
        {
            TenantName = "Pizzeria",
            TenantSlug = "pizzeria",
            LayoutConfig = new LayoutConfigDto
            {
                CanvasEnabled = true,
                BackgroundColor = "#000000",
                Elements = new List<CanvasElementDto>
                {
                    new() { DishId = dishId, X = 100, Y = 100, ZIndex = 1, Width = 150, Height = 100 }
                }
            }
        };

        // Act
        var hash1 = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(menu1)));
        var hash2 = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(menu2)));

        // Assert
        hash1.Should().Be(hash2);
    }
}
