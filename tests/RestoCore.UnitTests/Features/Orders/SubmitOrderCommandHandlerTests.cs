namespace RestoCore.UnitTests.Features.Orders;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Orders.Commands;
using RestoCore.Domain.Entities;
using Xunit;

public class SubmitOrderCommandHandlerTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock;
    private readonly Mock<ITenantRepository> _tenantRepoMock;
    private readonly Mock<IOrderQueueService> _queueServiceMock;
    private readonly SubmitOrderCommandHandler _handler;

    public SubmitOrderCommandHandlerTests()
    {
        _catalogRepoMock = new Mock<ICatalogRepository>();
        _tenantRepoMock = new Mock<ITenantRepository>();
        _queueServiceMock = new Mock<IOrderQueueService>();

        _handler = new SubmitOrderCommandHandler(
            _catalogRepoMock.Object,
            _tenantRepoMock.Object,
            _queueServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOrder_EnqueuesAndReturns202Response()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var tenantSlug = "bistro-test";
        var tableToken = "tab-token-123";

        var tenant = new Tenant { Id = tenantId, Slug = tenantSlug, Status = "Active" };
        var table = new Table { Id = tableId, TenantId = tenantId, TableNumber = 5, Token = tableToken };

        _tenantRepoMock
            .Setup(r => r.GetTenantBySlugAsync(tenantSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _catalogRepoMock
            .Setup(r => r.GetActiveTableByTokenAsync(tenantId, tableToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(table);

        _queueServiceMock
            .Setup(q => q.EnqueueOrderAsync(tenantSlug, It.IsAny<OrderIngestionPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("1728590000-0");

        var command = new SubmitOrderCommand(
            TenantSlug: tenantSlug,
            TableToken: tableToken,
            Notes: "Sin sal",
            Items: new List<SubmitOrderItemDto>
            {
                new(Guid.NewGuid(), 2, "Término medio", null)
            }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("queued");
        result.TenantSlug.Should().Be(tenantSlug);
        result.TableNumber.Should().Be(5);
        result.OrderId.Should().NotBeEmpty();

        _queueServiceMock.Verify(q => q.EnqueueOrderAsync(
            tenantSlug,
            It.Is<OrderIngestionPayload>(p => p.TableNumber == 5 && p.OrderId == result.OrderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveTenant_ThrowsKeyNotFoundException()
    {
        // Arrange
        _tenantRepoMock
            .Setup(r => r.GetTenantBySlugAsync("inactive-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var command = new SubmitOrderCommand("inactive-slug", "token", null, new List<SubmitOrderItemDto>());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
