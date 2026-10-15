namespace RestoCore.UnitTests.Features.Categories;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Categories.Queries;
using RestoCore.Domain.Entities;
using Xunit;

public class CategoryQueryHandlerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public CategoryQueryHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOrderedCategoriesForTenant()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), TenantId = _tenantId, Name = "Entradas", Description = "Aperitivos", DisplayOrder = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), TenantId = _tenantId, Name = "Principales", Description = "Carnes", DisplayOrder = 2, IsActive = true }
        };

        _catalogRepoMock.Setup(r => r.GetCategoriesByTenantAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var handler = new GetCategoriesQueryHandler(_catalogRepoMock.Object, _tenantContextMock.Object);

        // Act
        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Entradas");
        result[0].DisplayOrder.Should().Be(1);
        result[1].Name.Should().Be("Principales");
        result[1].DisplayOrder.Should().Be(2);
    }

    [Fact]
    public async Task GetCategoryById_WhenExists_ShouldReturnCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            TenantId = _tenantId,
            Name = "Postres",
            Description = "Dulces",
            DisplayOrder = 3,
            IsActive = true
        };

        _catalogRepoMock.Setup(r => r.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new GetCategoryByIdQueryHandler(_catalogRepoMock.Object, _tenantContextMock.Object);

        // Act
        var result = await handler.Handle(new GetCategoryByIdQuery(categoryId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(categoryId);
        result.Name.Should().Be("Postres");
    }

    [Fact]
    public async Task GetCategoryById_WhenNotFoundOrOtherTenant_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _catalogRepoMock.Setup(r => r.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var handler = new GetCategoryByIdQueryHandler(_catalogRepoMock.Object, _tenantContextMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetCategoryByIdQuery(categoryId), CancellationToken.None));
    }
}
