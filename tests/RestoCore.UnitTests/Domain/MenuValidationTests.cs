namespace RestoCore.UnitTests.Domain;

using FluentAssertions;
using RestoCore.Application.Features.Categories.Commands;
using RestoCore.Application.Features.Categories.Validators;
using RestoCore.Application.Features.MenuItems.Commands;
using RestoCore.Application.Features.MenuItems.Validators;
using Xunit;

public class MenuValidationTests
{
    private readonly CreateCategoryValidator _categoryValidator = new();
    private readonly CreateMenuItemValidator _menuItemValidator = new();

    [Fact]
    public void CreateCategory_WithValidData_ShouldPassValidation()
    {
        var command = new CreateCategoryCommand(
            Name: "Pastas Caseras",
            Description: "Pastas hechas a mano diariamente",
            DisplayOrder: 1
        );

        var result = _categoryValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCategory_WithEmptyName_ShouldFailValidation(string invalidName)
    {
        var command = new CreateCategoryCommand(
            Name: invalidName,
            Description: "Valida",
            DisplayOrder: 1
        );

        var result = _categoryValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void CreateCategory_WithNameExceeding80Characters_ShouldFailValidation()
    {
        var command = new CreateCategoryCommand(
            Name: new string('A', 81),
            Description: null,
            DisplayOrder: 0
        );

        var result = _categoryValidator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateMenuItem_WithValidData_ShouldPassValidation()
    {
        var command = new CreateMenuItemCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Ravioles de Espinaca",
            Description: "Con salsa bolognesa tradicional",
            BasePrice: 12500.50m,
            ImageUrl: "https://cdn.restocore.app/items/ravioles.webp",
            DisplayOrder: 1,
            Allergens: new[] { "gluten", "lactosa" },
            DietaryLabels: new[] { "vegetariano" }
        );

        var result = _menuItemValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateMenuItem_WithNegativePrice_ShouldFailValidation()
    {
        var command = new CreateMenuItemCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Ensalada Mixta",
            Description: null,
            BasePrice: -100.00m,
            ImageUrl: null,
            DisplayOrder: 0,
            Allergens: Array.Empty<string>(),
            DietaryLabels: Array.Empty<string>()
        );

        var result = _menuItemValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMenuItemCommand.BasePrice));
    }

    [Fact]
    public void CreateMenuItem_WithEmptyName_ShouldFailValidation()
    {
        var command = new CreateMenuItemCommand(
            CategoryId: Guid.NewGuid(),
            Name: "",
            Description: null,
            BasePrice: 5000m,
            ImageUrl: null,
            DisplayOrder: 0,
            Allergens: Array.Empty<string>(),
            DietaryLabels: Array.Empty<string>()
        );

        var result = _menuItemValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMenuItemCommand.Name));
    }

    [Fact]
    public void CreateMenuItem_WithEmptyCategoryId_ShouldFailValidation()
    {
        var command = new CreateMenuItemCommand(
            CategoryId: Guid.Empty,
            Name: "Bife de Chorizo",
            Description: null,
            BasePrice: 18000m,
            ImageUrl: null,
            DisplayOrder: 0,
            Allergens: Array.Empty<string>(),
            DietaryLabels: Array.Empty<string>()
        );

        var result = _menuItemValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMenuItemCommand.CategoryId));
    }
}
