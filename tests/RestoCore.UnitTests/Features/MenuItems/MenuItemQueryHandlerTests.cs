namespace RestoCore.UnitTests.Features.MenuItems;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.MenuItems.Queries;
using RestoCore.Domain.Entities;

public class MenuItemQueryHandlerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepositoryMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public MenuItemQueryHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task GetMenuItems_ShouldReturnTenantMenuItems()
    {
        var categoryId = Guid.NewGuid();
        var items = new List<MenuItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantId,
                CategoryId = categoryId,
                Name = "Bife de Chorizo",
                Description = "Corte clásico",
                BasePrice = 18500m,
                IsAvailable = true,
                DisplayOrder = 1,
                Allergens = Array.Empty<string>(),
                DietaryLabels = new[] { "Sin TACC" }
            }
        };

        _catalogRepositoryMock
            .Setup(r => r.GetMenuItemsByTenantAsync(_tenantId, categoryId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var handler = new GetMenuItemsQueryHandler(_catalogRepositoryMock.Object, _tenantContextMock.Object);
        var result = await handler.Handle(new GetMenuItemsQuery(categoryId, false), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Bife de Chorizo");
        result[0].BasePrice.Should().Be(18500m);
    }

    [Fact]
    public async Task GetMenuItemById_WhenFoundForTenant_ShouldReturnMenuItem()
    {
        var itemId = Guid.NewGuid();
        var item = new MenuItem
        {
            Id = itemId,
            TenantId = _tenantId,
            CategoryId = Guid.NewGuid(),
            Name = "Volcán de Chocolate",
            BasePrice = 8900m,
            IsAvailable = true
        };

        _catalogRepositoryMock
            .Setup(r => r.GetMenuItemByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var handler = new GetMenuItemByIdQueryHandler(_catalogRepositoryMock.Object, _tenantContextMock.Object);
        var result = await handler.Handle(new GetMenuItemByIdQuery(itemId), CancellationToken.None);

        result.Id.Should().Be(itemId);
        result.Name.Should().Be("Volcán de Chocolate");
    }

    [Fact]
    public async Task GetMenuItemById_WhenBelongsToDifferentTenant_ShouldThrowKeyNotFoundException()
    {
        var itemId = Guid.NewGuid();
        var item = new MenuItem
        {
            Id = itemId,
            TenantId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Other Tenant Dish"
        };

        _catalogRepositoryMock
            .Setup(r => r.GetMenuItemByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var handler = new GetMenuItemByIdQueryHandler(_catalogRepositoryMock.Object, _tenantContextMock.Object);
        var act = async () => await handler.Handle(new GetMenuItemByIdQuery(itemId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
