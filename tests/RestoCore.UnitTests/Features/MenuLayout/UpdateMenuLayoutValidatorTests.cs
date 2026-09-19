namespace RestoCore.UnitTests.Features.MenuLayout;

using System;
using System.Collections.Generic;
using FluentAssertions;
using RestoCore.Application.Features.MenuLayout.Commands;
using RestoCore.Application.Features.MenuLayout.DTOs;
using RestoCore.Application.Features.MenuLayout.Validators;
using Xunit;

public class UpdateMenuLayoutValidatorTests
{
    private readonly UpdateMenuLayoutValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Layout_Is_Valid()
    {
        var command = new UpdateMenuLayoutCommand(new LayoutConfigDto
        {
            CanvasEnabled = true,
            BackgroundColor = "#1A1A1A",
            BackgroundUrl = "https://storage.restocore.app/bg.webp",
            Elements = new List<CanvasElementDto>
            {
                new()
                {
                    DishId = Guid.NewGuid(),
                    X = 100.5,
                    Y = 200.0,
                    ZIndex = 1,
                    Width = 300,
                    Height = 200
                }
            }
        });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_When_Element_Has_Negative_Coordinates()
    {
        var command = new UpdateMenuLayoutCommand(new LayoutConfigDto
        {
            CanvasEnabled = true,
            Elements = new List<CanvasElementDto>
            {
                new()
                {
                    DishId = Guid.NewGuid(),
                    X = -10.0,
                    Y = 100.0,
                    ZIndex = 0,
                    Width = 100,
                    Height = 100
                }
            }
        });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("X"));
    }

    [Fact]
    public void Should_Fail_When_Element_Has_Zero_Or_Negative_Dimensions()
    {
        var command = new UpdateMenuLayoutCommand(new LayoutConfigDto
        {
            CanvasEnabled = true,
            Elements = new List<CanvasElementDto>
            {
                new()
                {
                    DishId = Guid.NewGuid(),
                    X = 0,
                    Y = 0,
                    ZIndex = 0,
                    Width = 0,
                    Height = -50
                }
            }
        });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Width"));
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Height"));
    }

    [Fact]
    public void Should_Fail_When_DishId_Is_Empty()
    {
        var command = new UpdateMenuLayoutCommand(new LayoutConfigDto
        {
            CanvasEnabled = true,
            Elements = new List<CanvasElementDto>
            {
                new()
                {
                    DishId = Guid.Empty,
                    X = 10,
                    Y = 10,
                    ZIndex = 1,
                    Width = 100,
                    Height = 100
                }
            }
        });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("DishId"));
    }
}