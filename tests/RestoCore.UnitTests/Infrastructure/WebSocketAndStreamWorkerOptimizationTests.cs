namespace RestoCore.UnitTests.Infrastructure;

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;
using RestoCore.Infrastructure.BackgroundServices;
using RestoCore.Infrastructure.WebSockets;
using Xunit;

public class WebSocketAndStreamWorkerOptimizationTests
{
    [Fact]
    public async Task KitchenWebSocketManager_Broadcast_ShouldRemoveAndDisposeClosedAndFaultedSockets()
    {
        var manager = new KitchenWebSocketManager(NullLogger<KitchenWebSocketManager>.Instance);
        var tenantSlug = "pizzeria-test";

        var openSocketMock = new Mock<WebSocket>();
        openSocketMock.SetupGet(s => s.State).Returns(WebSocketState.Open);
        openSocketMock
            .Setup(s => s.SendAsync(It.IsAny<ArraySegment<byte>>(), WebSocketMessageType.Text, true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var closedSocketMock = new Mock<WebSocket>();
        closedSocketMock.SetupGet(s => s.State).Returns(WebSocketState.Closed);

        var faultedSocketMock = new Mock<WebSocket>();
        faultedSocketMock.SetupGet(s => s.State).Returns(WebSocketState.Open);
        faultedSocketMock
            .Setup(s => s.SendAsync(It.IsAny<ArraySegment<byte>>(), WebSocketMessageType.Text, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WebSocketException("Simulated socket disconnect"));

        var openId = manager.RegisterSocket(tenantSlug, openSocketMock.Object);
        var closedId = manager.RegisterSocket(tenantSlug, closedSocketMock.Object);
        var faultedId = manager.RegisterSocket(tenantSlug, faultedSocketMock.Object);

        manager.GetActiveConnectionCount(tenantSlug).Should().Be(3);

        await manager.BroadcastOrderCreatedAsync(
            tenantSlug,
            new OrderCreatedNotification(
                Guid.NewGuid(),
                tenantSlug,
                12,
                "received",
                4500m,
                null,
                DateTimeOffset.UtcNow));

        manager.GetActiveConnectionCount(tenantSlug).Should().Be(1);
        closedSocketMock.Verify(s => s.Dispose(), Times.Once);
        faultedSocketMock.Verify(s => s.Dispose(), Times.Once);

        manager.RemoveAndDisposeSocket(tenantSlug, openId);
        manager.GetActiveConnectionCount(tenantSlug).Should().Be(0);
    }

    [Fact]
    public async Task OrderProcessorWorker_ProcessOrderMessageAsync_ShouldBatchLoadMenuItemsInSingleQuery()
    {
        var tenantId = Guid.NewGuid();
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var item1 = new MenuItem
        {
            Id = item1Id,
            TenantId = tenantId,
            Name = "Empanada Carne",
            BasePrice = 1500m,
            IsAvailable = true,
            IsDeleted = false
        };

        var item2 = new MenuItem
        {
            Id = item2Id,
            TenantId = tenantId,
            Name = "Empanada Jamon y Queso",
            BasePrice = 1500m,
            IsAvailable = true,
            IsDeleted = false
        };

        var catalogRepoMock = new Mock<ICatalogRepository>();
        catalogRepoMock
            .Setup(r => r.GetMenuItemsWithModifiersByIdsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItem> { item1, item2 });

        var orderRepoMock = new Mock<IOrderRepository>();
        orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var notifierMock = new Mock<IKitchenNotifier>();
        var queueServiceMock = new Mock<IOrderQueueService>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var worker = new OrderProcessorWorker(serviceProviderMock.Object, NullLogger<OrderProcessorWorker>.Instance);
        var streamMessage = new OrderStreamMessage(
            "1700000000-0",
            new OrderIngestionPayload(
                orderId,
                tenantId,
                Guid.NewGuid(),
                12,
                "Mesa 12",
                new List<OrderItemInput>
                {
                    new(item1Id, 2, null, new List<Guid>()),
                    new(item2Id, 3, null, new List<Guid>())
                }));

        await worker.ProcessOrderMessageAsync(
            streamMessage,
            "pizzeria-test",
            catalogRepoMock.Object,
            orderRepoMock.Object,
            unitOfWorkMock.Object,
            notifierMock.Object,
            queueServiceMock.Object,
            CancellationToken.None);

        // Verify batch method was called exactly once and N+1 individual method was NEVER called
        catalogRepoMock.Verify(
            r => r.GetMenuItemsWithModifiersByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        catalogRepoMock.Verify(
            r => r.GetMenuItemWithModifiersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        orderRepoMock.Verify(r => r.AddOrder(It.Is<Order>(o => o.Id == orderId && o.TotalAmount == 7500m)), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        queueServiceMock.Verify(q => q.AcknowledgeOrderAsync("pizzeria-test", "kitchen-workers", "1700000000-0", It.IsAny<CancellationToken>()), Times.Once);
    }
}
