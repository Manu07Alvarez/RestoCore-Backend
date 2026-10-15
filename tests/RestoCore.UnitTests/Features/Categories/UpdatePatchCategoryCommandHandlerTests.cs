namespace RestoCore.UnitTests.Features.Categories;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Categories.Commands;
using RestoCore.Domain.Entities;
using Xunit;

public class UpdatePatchCategoryCommandHandlerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Mock<ICdnPurgeService> _cdnPurgeMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public UpdatePatchCategoryCommandHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
        _tenantContextMock.Setup(t => t.TenantSlug).Returns("don-pepe");
    }

    [Fact]
    public async Task UpdateCategory_WhenExists_ShouldUpdateAllFieldsAndInvalidateCache()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            TenantId = _tenantId,
            Name = "Old Name",
            Description = "Old Desc",
            DisplayOrder = 1,
            IsActive = true
        };

        _catalogRepoMock.Setup(r => r.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new UpdateCategoryCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _cdnPurgeMock.Object);

        var command = new UpdateCategoryCommand(categoryId, "New Name", "New Desc", 5, false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New Desc");
        result.DisplayOrder.Should().Be(5);
        result.IsActive.Should().BeFalse();

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cdnPurgeMock.Verify(c => c.PurgeTenantCacheAsync("don-pepe", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PatchCategory_WhenPartialFieldsProvided_ShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            TenantId = _tenantId,
            Name = "Keep Name",
            Description = "Keep Desc",
            DisplayOrder = 1,
            IsActive = true
        };

        _catalogRepoMock.Setup(r => r.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new PatchCategoryCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _cdnPurgeMock.Object);

        var command = new PatchCategoryCommand(categoryId, null, null, 10, false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Keep Name");
        result.Description.Should().Be("Keep Desc");
        result.DisplayOrder.Should().Be(10);
        result.IsActive.Should().BeFalse();

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cdnPurgeMock.Verify(c => c.PurgeTenantCacheAsync("don-pepe", It.IsAny<CancellationToken>()), Times.Once);
    }
}
