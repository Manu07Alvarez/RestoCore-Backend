namespace RestoCore.UnitTests.Features.MenuItems;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.MenuItems.Commands;
using RestoCore.Domain.Entities;
using Xunit;

public class PatchMenuItemCommandHandlerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public PatchMenuItemCommandHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WhenPartialAttributesProvided_ShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingItem = new MenuItem
        {
            Id = itemId,
            TenantId = _tenantId,
            CategoryId = categoryId,
            Name = "Original Burger",
            Description = "Original Desc",
            BasePrice = 1000m,
            IsAvailable = true,
            DietaryLabels = new[] { "celiac_safe" }
        };

        _catalogRepoMock.Setup(r => r.GetMenuItemByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var handler = new PatchMenuItemCommandHandler(
            _catalogRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);

        var command = new PatchMenuItemCommand(
            itemId,
            null,
            null,
            null,
            1250m,
            false,
            null,
            null,
            null,
            null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Original Burger"); // Unchanged
        result.Description.Should().Be("Original Desc"); // Unchanged
        result.BasePrice.Should().Be(1250m); // Updated
        result.IsAvailable.Should().BeFalse(); // Updated
        result.DietaryLabels.Should().Contain("celiac_safe"); // Unchanged

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
