namespace RestoCore.UnitTests.Features.MenuLayout;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using RestoCore.Application.Features.MenuLayout.DTOs;
using RestoCore.Infrastructure.Services.Compilation;
using Xunit;

public class MenuCompilationServiceTests
{
    private readonly MenuCompilationService _service;

    public MenuCompilationServiceTests()
    {
        // MenuCompilationService accepts IApplicationDbContext which can be null for pure ComputeVersionHash tests
        _service = new MenuCompilationService(null!);
    }

    [Fact]
    public void ComputeVersionHash_Should_Follow_Required_Format()
    {
        // Arrange
        var dishId = Guid.NewGuid();
        var timestamp = new DateTime(2026, 9, 18, 14, 30, 0, DateTimeKind.Utc);
        var layout = new LayoutConfigDto
        {
            CanvasEnabled = true,
            Elements = new List<CanvasElementDto>
            {
                new() { DishId = dishId, X = 100, Y = 200, ZIndex = 1, Width = 150, Height = 100 }
            }
        };

        // Act
        var hash = _service.ComputeVersionHash(layout, new[] { dishId }, timestamp);

        // Assert
        hash.Should().MatchRegex(@"^v20260918143000-[a-f0-9]{8}$");
    }

    [Fact]
    public void ComputeVersionHash_Should_Be_Deterministic_For_Identical_Inputs()
    {
        // Arrange
        var dishId = Guid.NewGuid();
        var timestamp = new DateTime(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc);

        var layout1 = new LayoutConfigDto
        {
            CanvasEnabled = true,
            BackgroundColor = "#FFFFFF",
            Elements = new List<CanvasElementDto>
            {
                new() { DishId = dishId, X = 50, Y = 75, ZIndex = 1, Width = 200, Height = 120 }
            }
        };

        var layout2 = new LayoutConfigDto
        {
            CanvasEnabled = true,
            BackgroundColor = "#FFFFFF",
            Elements = new List<CanvasElementDto>
            {
                new() { DishId = dishId, X = 50, Y = 75, ZIndex = 1, Width = 200, Height = 120 }
            }
        };

        // Act
        var hash1 = _service.ComputeVersionHash(layout1, new[] { dishId }, timestamp);
        var hash2 = _service.ComputeVersionHash(layout2, new[] { dishId }, timestamp);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ComputeVersionHash_Should_Change_When_Coordinates_Differ()
    {
        // Arrange
        var dishId = Guid.NewGuid();
        var timestamp = new DateTime(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc);

        var layout1 = new LayoutConfigDto
        {
            CanvasEnabled = true,
            Elements = new List<CanvasElementDto>
            {
                new() { DishId = dishId, X = 50, Y = 75, ZIndex = 1, Width = 200, Height = 120 }
            }
        };

        var layout2 = new LayoutConfigDto
        {
            CanvasEnabled = true,
            Elements = new List<CanvasElementDto>
            {
                new() { DishId = dishId, X = 150, Y = 75, ZIndex = 1, Width = 200, Height = 120 }
            }
        };

        // Act
        var hash1 = _service.ComputeVersionHash(layout1, new[] { dishId }, timestamp);
        var hash2 = _service.ComputeVersionHash(layout2, new[] { dishId }, timestamp);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void ComputeVersionHash_Should_Change_When_Catalog_Dishes_Differ()
    {
        // Arrange
        var dishId1 = Guid.NewGuid();
        var dishId2 = Guid.NewGuid();
        var timestamp = new DateTime(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc);

        var layout = new LayoutConfigDto
        {
            CanvasEnabled = false
        };

        // Act
        var hash1 = _service.ComputeVersionHash(layout, new[] { dishId1 }, timestamp);
        var hash2 = _service.ComputeVersionHash(layout, new[] { dishId1, dishId2 }, timestamp);

        // Assert
        hash1.Should().NotBe(hash2);
    }
}
