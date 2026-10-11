namespace RestoCore.UnitTests.Workers;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;
using RestoCore.Infrastructure.Workers;
using Xunit;

public class PurgeArchivedDishesWorkerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<PurgeArchivedDishesWorker>> _loggerMock = new();
    private readonly IServiceProvider _serviceProvider;

    public PurgeArchivedDishesWorkerTests()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => _catalogRepoMock.Object);
        services.AddScoped(_ => _storageServiceMock.Object);
        services.AddScoped(_ => _unitOfWorkMock.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task PurgeExpiredDishesAsync_WhenExpiredDishesExist_ShouldDeleteFromRepoAndStorageAndCommit()
    {
        // Arrange
        var dish1 = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Old Dish 1",
            ImageUrl = "http://seaweedfs:8888/dishes/old1.jpg",
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-8)
        };

        var dish2 = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Old Dish 2 Without Image",
            ImageUrl = null,
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };

        var expiredList = new List<MenuItem> { dish1, dish2 };

        _catalogRepoMock.Setup(r => r.GetArchivedMenuItemsOlderThanAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredList);

        var worker = new PurgeArchivedDishesWorker(_serviceProvider, _loggerMock.Object);

        // Act
        var purgedCount = await worker.PurgeExpiredDishesAsync(CancellationToken.None);

        // Assert
        purgedCount.Should().Be(2);

        _storageServiceMock.Verify(s => s.DeleteObjectAsync("http://seaweedfs:8888/dishes/old1.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _catalogRepoMock.Verify(r => r.RemoveMenuItem(dish1), Times.Once);
        _catalogRepoMock.Verify(r => r.RemoveMenuItem(dish2), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PurgeExpiredDishesAsync_WhenNoExpiredDishes_ShouldReturnZeroWithoutCommit()
    {
        // Arrange
        _catalogRepoMock.Setup(r => r.GetArchivedMenuItemsOlderThanAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItem>());

        var worker = new PurgeArchivedDishesWorker(_serviceProvider, _loggerMock.Object);

        // Act
        var purgedCount = await worker.PurgeExpiredDishesAsync(CancellationToken.None);

        // Assert
        purgedCount.Should().Be(0);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
