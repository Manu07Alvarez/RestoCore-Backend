namespace RestoCore.UnitTests.Features.MenuItems;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.MenuItems.Commands;
using RestoCore.Domain.Entities;
using Xunit;

public class DeleteMenuItemCommandHandlerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock = new();
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public DeleteMenuItemCommandHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WhenDishHasActiveOrders_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MenuItem { Id = itemId, TenantId = _tenantId, Name = "Burger" };

        _catalogRepoMock.Setup(r => r.GetMenuItemByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _orderRepoMock.Setup(r => r.HasActiveOrdersForMenuItemAsync(_tenantId, itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteMenuItemCommandHandler(
            _catalogRepoMock.Object,
            _orderRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteMenuItemCommand(itemId), CancellationToken.None));

        ex.Message.Should().Contain("cocina");
    }

    [Fact]
    public async Task Handle_WhenNoActiveOrders_ShouldSoftDeleteAndReturnFriendlyMessage()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MenuItem { Id = itemId, TenantId = _tenantId, Name = "Burger", IsDeleted = false };

        _catalogRepoMock.Setup(r => r.GetMenuItemByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _orderRepoMock.Setup(r => r.HasActiveOrdersForMenuItemAsync(_tenantId, itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new DeleteMenuItemCommandHandler(
            _catalogRepoMock.Object,
            _orderRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);

        // Act
        var result = await handler.Handle(new DeleteMenuItemCommand(itemId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("archived");
        result.RetentionDays.Should().Be(7);
        result.Message.Should().Contain("retirado de la carta");

        item.IsDeleted.Should().BeTrue();
        item.DeletedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
