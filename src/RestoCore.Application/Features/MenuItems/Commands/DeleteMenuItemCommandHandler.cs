namespace RestoCore.Application.Features.MenuItems.Commands;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.MenuItems.DTOs;

public class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand, MenuItemArchiveResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public const string FriendlyMessage = "El plato ha sido retirado de la carta. Los clientes ya no lo verán en el menú digital ni podrán pedirlo, pero todas tus ventas anteriores y reportes seguirán intactos y seguros. Dispones de 7 días para restaurarlo antes de su eliminación definitiva.";
    public const string ActiveOrdersErrorMessage = "No es posible retirar este plato porque se encuentra actualmente en preparación en cocina o pedidos abiertos de salón. Espera a que los pedidos finalicen antes de archivarlo.";

    public DeleteMenuItemCommandHandler(
        ICatalogRepository catalogRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<MenuItemArchiveResponse> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var item = await _catalogRepository.GetMenuItemByIdAsync(request.Id, cancellationToken);
        if (item == null || item.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Menu item {request.Id} was not found.");
        }

        var hasActiveOrders = await _orderRepository.HasActiveOrdersForMenuItemAsync(tenantId, request.Id, cancellationToken);
        if (hasActiveOrders)
        {
            throw new InvalidOperationException(ActiveOrdersErrorMessage);
        }

        var now = DateTimeOffset.UtcNow;
        item.IsDeleted = true;
        item.DeletedAt = now;

        await _unitOfWork.CommitAsync(cancellationToken);

        return new MenuItemArchiveResponse(
            ItemId: item.Id,
            Status: "archived",
            Message: FriendlyMessage,
            HasOpenOrders: false,
            ArchivedAt: now,
            RetentionDays: 7,
            PurgeScheduledAt: now.AddDays(7)
        );
    }
}
