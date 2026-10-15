namespace RestoCore.UnitTests.Features.Categories;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Categories.Commands;
using RestoCore.Domain.Entities;
using Xunit;

public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Mock<ICdnPurgeService> _cdnPurgeMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public DeleteCategoryCommandHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
        _tenantContextMock.Setup(t => t.TenantSlug).Returns("don-pepe");
    }

    [Fact]
    public async Task Handle_WhenCategoryHasAssociatedDishes_ShouldThrowInvalidOperationExceptionForConflict()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, TenantId = _tenantId, Name = "Carnes" };

        _catalogRepoMock.Setup(r => r.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _catalogRepoMock.Setup(r => r.HasMenuItemsForCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteCategoryCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _cdnPurgeMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteCategoryCommand(categoryId), CancellationToken.None));

        ex.Message.Should().Contain("platos");
        _catalogRepoMock.Verify(r => r.RemoveCategory(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCategoryIsEmpty_ShouldRemoveCategoryAndInvalidateCache()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, TenantId = _tenantId, Name = "Vacía" };

        _catalogRepoMock.Setup(r => r.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _catalogRepoMock.Setup(r => r.HasMenuItemsForCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new DeleteCategoryCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _cdnPurgeMock.Object);

        // Act
        await handler.Handle(new DeleteCategoryCommand(categoryId), CancellationToken.None);

        // Assert
        _catalogRepoMock.Verify(r => r.RemoveCategory(category), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cdnPurgeMock.Verify(c => c.PurgeTenantCacheAsync("don-pepe", It.IsAny<CancellationToken>()), Times.Once);
    }
}
