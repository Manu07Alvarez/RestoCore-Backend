namespace RestoCore.UnitTests.Features.PublicMenu;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.PublicMenu.Queries;
using RestoCore.Domain.Entities;
using RestoCore.Domain.ValueObjects;
using Xunit;

public class PublicMenuSoftDeleteFilterTests
{
    [Fact]
    public async Task GetPublicMenuQueryHandler_ShouldExcludeSoftDeletedAndUnavailableMenuItems_FromCategoriesAndLayoutElements()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var activeItemId = Guid.NewGuid();
        var deletedItemId = Guid.NewGuid();
        var unavailableItemId = Guid.NewGuid();

        var activeItem = new MenuItem
        {
            Id = activeItemId,
            TenantId = tenantId,
            CategoryId = categoryId,
            Name = "Active Pizza",
            BasePrice = 1500m,
            IsAvailable = true,
            IsDeleted = false,
            DisplayOrder = 1
        };

        var deletedItem = new MenuItem
        {
            Id = deletedItemId,
            TenantId = tenantId,
            CategoryId = categoryId,
            Name = "Deleted Pizza",
            BasePrice = 1500m,
            IsAvailable = true,
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow,
            DisplayOrder = 2
        };

        var unavailableItem = new MenuItem
        {
            Id = unavailableItemId,
            TenantId = tenantId,
            CategoryId = categoryId,
            Name = "Unavailable Pizza",
            BasePrice = 1500m,
            IsAvailable = false,
            IsDeleted = false,
            DisplayOrder = 3
        };

        var category = new Category
        {
            Id = categoryId,
            TenantId = tenantId,
            Name = "Pizzas",
            DisplayOrder = 1,
            IsActive = true,
            Items = new List<MenuItem> { activeItem, deletedItem, unavailableItem }
        };

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Pizzeria Test",
            Slug = "pizzeria-test",
            Status = "Active",
            Categories = new List<Category> { category },
            LayoutConfig = new LayoutConfig
            {
                CanvasEnabled = true,
                Elements = new List<CanvasElement>
                {
                    new() { DishId = activeItemId, X = 10, Y = 10, Width = 100, Height = 100 },
                    new() { DishId = deletedItemId, X = 20, Y = 20, Width = 100, Height = 100 },
                    new() { DishId = unavailableItemId, X = 30, Y = 30, Width = 100, Height = 100 }
                }
            }
        };

        var catalogRepoMock = new Mock<ICatalogRepository>();
        catalogRepoMock
            .Setup(r => r.GetTenantWithCatalogAsync("pizzeria-test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var handler = new GetPublicMenuQueryHandler(catalogRepoMock.Object);
        var (response, etag) = await handler.Handle(new GetPublicMenuQuery("pizzeria-test"), CancellationToken.None);

        response.Should().NotBeNull();
        etag.Should().NotBeNullOrWhiteSpace();
        response.Categories.Should().ContainSingle();
        response.Categories[0].Items.Should().HaveCount(2);
        response.Categories[0].Items.Should().Contain(i => i.Id == activeItemId && i.Name == "Active Pizza" && i.IsAvailable);
        response.Categories[0].Items.Should().Contain(i => i.Id == unavailableItemId && i.Name == "Unavailable Pizza" && !i.IsAvailable);
        response.Categories[0].Items.Should().NotContain(i => i.Id == deletedItemId);
        response.LayoutConfig.Should().NotBeNull();
        response.LayoutConfig!.Elements.Should().NotContain(e => e.DishId == deletedItemId);
    }
}
