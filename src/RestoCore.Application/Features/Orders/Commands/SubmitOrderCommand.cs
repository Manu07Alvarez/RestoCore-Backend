namespace RestoCore.Application.Features.Orders.Commands;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;

public record SubmitOrderItemDto(
    Guid MenuItemId,
    int Quantity,
    string? Instructions,
    List<Guid>? SelectedOptionIds
);

public record SubmitOrderCommand(
    string TenantSlug,
    string TableToken,
    string? Notes,
    List<SubmitOrderItemDto> Items
) : IRequest<SubmitOrderResponse>;

public record SubmitOrderResponse(
    Guid OrderId,
    string Status,
    string TenantSlug,
    int TableNumber,
    DateTimeOffset QueuedAt
);

public class SubmitOrderCommandHandler : IRequestHandler<SubmitOrderCommand, SubmitOrderResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IOrderQueueService _orderQueueService;

    public SubmitOrderCommandHandler(
        ICatalogRepository catalogRepository,
        ITenantRepository tenantRepository,
        IOrderQueueService orderQueueService)
    {
        _catalogRepository = catalogRepository;
        _tenantRepository = tenantRepository;
        _orderQueueService = orderQueueService;
    }

    public async Task<SubmitOrderResponse> Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetTenantBySlugAsync(request.TenantSlug, cancellationToken);
        if (tenant == null || tenant.Status != "Active")
        {
            throw new KeyNotFoundException($"Restaurante '{request.TenantSlug}' no existe o está inactivo.");
        }

        var table = await _catalogRepository.GetActiveTableByTokenAsync(tenant.Id, request.TableToken, cancellationToken);
        if (table == null)
        {
            throw new KeyNotFoundException($"Mesa con token '{request.TableToken}' no es válida.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException("El pedido debe contener al menos un elemento.");
        }

        var orderId = Guid.NewGuid();
        var ingestionPayload = new OrderIngestionPayload(
            OrderId: orderId,
            TenantId: tenant.Id,
            TableId: table.Id,
            TableNumber: table.TableNumber,
            Notes: request.Notes,
            Items: request.Items.Select(i => new OrderItemInput(
                MenuItemId: i.MenuItemId,
                Quantity: i.Quantity > 0 ? i.Quantity : 1,
                Instructions: i.Instructions,
                SelectedOptionIds: i.SelectedOptionIds ?? new List<Guid>()
            )).ToList()
        );

        await _orderQueueService.EnqueueOrderAsync(tenant.Slug, ingestionPayload, cancellationToken);

        return new SubmitOrderResponse(
            OrderId: orderId,
            Status: "queued",
            TenantSlug: tenant.Slug,
            TableNumber: table.TableNumber,
            QueuedAt: DateTimeOffset.UtcNow
        );
    }
}
