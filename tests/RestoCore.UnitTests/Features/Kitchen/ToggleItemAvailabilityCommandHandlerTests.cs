namespace RestoCore.UnitTests.Features.Kitchen;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Kitchen.Commands;
using RestoCore.Domain.Entities;
using Xunit;

public class ToggleItemAvailabilityCommandHandlerTests
{
    private readonly Mock<IKitchenRepository> _kitchenRepositoryMock;
    private readonly ToggleItemAvailabilityCommandHandler _handler;

    public ToggleItemAvailabilityCommandHandlerTests()
    {
        _kitchenRepositoryMock = new Mock<IKitchenRepository>();
        _handler = new ToggleItemAvailabilityCommandHandler(_kitchenRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingItem_TogglesAvailabilitySuccessfully()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MenuItem { Id = itemId, Name = "Burger", IsAvailable = true };

        _kitchenRepositoryMock
            .Setup(r => r.GetItemForAvailabilityToggleAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _kitchenRepositoryMock
            .Setup(r => r.UpdateItemAvailabilityWithRetryAsync(itemId, false, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new ToggleItemAvailabilityCommand(itemId, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ItemId.Should().Be(itemId);
        result.IsAvailable.Should().BeFalse();

        _kitchenRepositoryMock.Verify(
            r => r.UpdateItemAvailabilityWithRetryAsync(itemId, false, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentItem_ThrowsKeyNotFoundException()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _kitchenRepositoryMock
            .Setup(r => r.GetItemForAvailabilityToggleAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuItem?)null);

        var command = new ToggleItemAvailabilityCommand(itemId, true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_RetryFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MenuItem { Id = itemId, Name = "Pizza", IsAvailable = true };

        _kitchenRepositoryMock
            .Setup(r => r.GetItemForAvailabilityToggleAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _kitchenRepositoryMock
            .Setup(r => r.UpdateItemAvailabilityWithRetryAsync(itemId, false, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new ToggleItemAvailabilityCommand(itemId, false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*concurrency contention*");
    }
}
