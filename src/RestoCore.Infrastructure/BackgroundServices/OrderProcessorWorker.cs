namespace RestoCore.Infrastructure.BackgroundServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public class OrderProcessorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderProcessorWorker> _logger;
    private const string ConsumerGroup = "kitchen-workers";
    private const string ConsumerName = "worker-1";

    public OrderProcessorWorker(
        IServiceProvider serviceProvider,
        ILogger<OrderProcessorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderProcessorWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var queueService = scope.ServiceProvider.GetRequiredService<IOrderQueueService>();
                var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
                var catalogRepo = scope.ServiceProvider.GetRequiredService<ICatalogRepository>();
                var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var notifier = scope.ServiceProvider.GetRequiredService<IKitchenNotifier>();

                // Poll tenants or default streams
                var db = scope.ServiceProvider.GetRequiredService<RestoCore.Infrastructure.Persistence.ApplicationDbContext>();
                var activeTenants = db.Tenants.Select(t => t.Slug).ToList();

                bool hadWork = false;

                foreach (var slug in activeTenants)
                {
                    var msg = await queueService.DequeueNextOrderAsync(slug, ConsumerGroup, ConsumerName, stoppingToken);
                    if (msg != null)
                    {
                        hadWork = true;
                        await ProcessOrderMessageAsync(msg, slug, catalogRepo, orderRepo, unitOfWork, notifier, queueService, stoppingToken);
                    }
                }

                if (!hadWork)
                {
                    await Task.Delay(500, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in OrderProcessorWorker loop");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("OrderProcessorWorker stopped.");
    }

    private async Task ProcessOrderMessageAsync(
        OrderStreamMessage msg,
        string tenantSlug,
        ICatalogRepository catalogRepo,
        IOrderRepository orderRepo,
        IUnitOfWork unitOfWork,
        IKitchenNotifier notifier,
        IOrderQueueService queueService,
        CancellationToken ct)
    {
        var payload = msg.Payload;
        _logger.LogInformation("Processing sequential order {OrderId} for table {TableNumber} in tenant {TenantSlug}",
            payload.OrderId, payload.TableNumber, tenantSlug);

        // Check idempotency: order already persisted?
        var existingOrder = await orderRepo.GetByIdAsync(payload.OrderId, ct);
        if (existingOrder != null)
        {
            await queueService.AcknowledgeOrderAsync(tenantSlug, ConsumerGroup, msg.MessageId, ct);
            return;
        }

        decimal totalAmount = 0;
        var snapshots = new List<OrderItemSnapshot>();

        foreach (var itemInput in payload.Items)
        {
            var menuItem = await catalogRepo.GetMenuItemWithModifiersAsync(itemInput.MenuItemId, ct);
            if (menuItem == null) continue;

            decimal unitPrice = menuItem.BasePrice;
            var modifierSnapshots = new List<OrderModifierSnapshot>();

            foreach (var optId in itemInput.SelectedOptionIds)
            {
                var opt = menuItem.ModifierGroups
                    .SelectMany(g => g.Options)
                    .FirstOrDefault(o => o.Id == optId);

                if (opt != null)
                {
                    unitPrice += opt.PriceDelta;
                    modifierSnapshots.Add(new OrderModifierSnapshot
                    {
                        OptionId = opt.Id,
                        Name = opt.Name,
                        PriceDelta = opt.PriceDelta
                    });
                }
            }

            totalAmount += unitPrice * itemInput.Quantity;

            snapshots.Add(new OrderItemSnapshot
            {
                MenuItemId = menuItem.Id,
                Name = menuItem.Name,
                Quantity = itemInput.Quantity,
                UnitPrice = unitPrice,
                Instructions = itemInput.Instructions,
                Modifiers = modifierSnapshots
            });
        }

        var order = new Order
        {
            Id = payload.OrderId,
            TenantId = payload.TenantId,
            TableId = payload.TableId,
            TableNumber = payload.TableNumber,
            Status = "received",
            TotalAmount = totalAmount,
            CustomerNotes = payload.Notes,
            ItemsSnapshot = snapshots,
            CreatedAt = DateTimeOffset.UtcNow,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        orderRepo.AddOrder(order);
        await unitOfWork.CommitAsync(ct);

        // Acknowledge Redis stream message
        await queueService.AcknowledgeOrderAsync(tenantSlug, ConsumerGroup, msg.MessageId, ct);

        // Broadcast real-time event to kitchen screens (<100ms)
        await notifier.BroadcastOrderCreatedAsync(tenantSlug, new OrderCreatedNotification(
            OrderId: order.Id,
            TenantSlug: tenantSlug,
            TableNumber: order.TableNumber,
            Status: order.Status,
            TotalAmount: order.TotalAmount,
            Notes: order.CustomerNotes,
            CreatedAt: order.CreatedAt
        ), ct);
    }
}
