namespace RestoCore.UnitTests.Features.MenuItems;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.MenuItems.Commands;
using RestoCore.Domain.Entities;
using Xunit;

public class RestoreMenuItemCommandHandlerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public RestoreMenuItemCommandHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WhenItemDeletedWithin7Days_ShouldRestoreAndReturnMenuItemResponse()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MenuItem
        {
            Id = itemId,
            TenantId = _tenantId,
            Name = "Archived Dish",
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-3)
        };

        _catalogRepoMock.Setup(r => r.GetMenuItemByIdWithDeletedAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var handler = new RestoreMenuItemCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);

        // Act
        var result = await handler.Handle(new RestoreMenuItemCommand(itemId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(itemId);
        item.IsDeleted.Should().BeFalse();
        item.DeletedAt.Should().BeNull();

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenItemDeletedMoreThan7DaysAgo_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MenuItem
        {
            Id = itemId,
            TenantId = _tenantId,
            Name = "Purged Dish",
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-8)
        };

        _catalogRepoMock.Setup(r => r.GetMenuItemByIdWithDeletedAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var handler = new RestoreMenuItemCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new RestoreMenuItemCommand(itemId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenItemIsNotDeleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MenuItem
        {
            Id = itemId,
            TenantId = _tenantId,
            Name = "Active Dish",
            IsDeleted = false
        };

        _catalogRepoMock.Setup(r => r.GetMenuItemByIdWithDeletedAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var handler = new RestoreMenuItemCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new RestoreMenuItemCommand(itemId), CancellationToken.None));
    }
}
