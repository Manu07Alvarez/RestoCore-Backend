namespace RestoCore.UnitTests.Features.MenuItems;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.MenuItems.Commands;
using RestoCore.Domain.Entities;
using Xunit;

public class UpdateMenuItemCommandHandlerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public UpdateMenuItemCommandHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WhenItemAndCategoryExist_ShouldUpdateAllFieldsAndCommit()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, TenantId = _tenantId, Name = "Burgers" };
        var existingItem = new MenuItem
        {
            Id = itemId,
            TenantId = _tenantId,
            CategoryId = categoryId,
            Name = "Old Burger",
            BasePrice = 1000m,
            IsAvailable = true
        };

        _catalogRepoMock.Setup(r => r.GetMenuItemByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);
        _catalogRepoMock.Setup(r => r.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new UpdateMenuItemCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);

        var command = new UpdateMenuItemCommand(
            itemId,
            categoryId,
            "Updated Burger",
            "Tasty description",
            1500m,
            true,
            "http://image.jpg",
            2,
            new[] { "gluten" },
            new[] { "contains_dairy" }
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Burger");
        result.BasePrice.Should().Be(1500m);
        result.DietaryLabels.Should().Contain("contains_dairy");

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingItem = new MenuItem
        {
            Id = itemId,
            TenantId = _tenantId,
            CategoryId = categoryId,
            Name = "Burger"
        };

        _catalogRepoMock.Setup(r => r.GetMenuItemByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);
        _catalogRepoMock.Setup(r => r.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var handler = new UpdateMenuItemCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);

        var command = new UpdateMenuItemCommand(
            itemId,
            categoryId,
            "Updated Burger",
            null,
            1500m,
            true,
            null,
            1,
            Array.Empty<string>(),
            Array.Empty<string>()
        );

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
