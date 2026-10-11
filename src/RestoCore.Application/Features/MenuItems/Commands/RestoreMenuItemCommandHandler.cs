namespace RestoCore.Application.Features.MenuItems.Commands;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;

public class RestoreMenuItemCommandHandler : IRequestHandler<RestoreMenuItemCommand, MenuItemResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public RestoreMenuItemCommandHandler(
        ICatalogRepository catalogRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<MenuItemResponse> Handle(RestoreMenuItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var item = await _catalogRepository.GetMenuItemByIdWithDeletedAsync(request.Id, cancellationToken);
        if (item == null || item.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Menu item {request.Id} was not found.");
        }

        if (!item.IsDeleted)
        {
            throw new InvalidOperationException($"Menu item {request.Id} is not deleted.");
        }

        // Verify 7-day retention grace period
        var retentionLimit = DateTimeOffset.UtcNow.AddDays(-7);
        if (item.DeletedAt.HasValue && item.DeletedAt.Value < retentionLimit)
        {
            throw new KeyNotFoundException($"The 7-day retention window for menu item {request.Id} has expired.");
        }

        item.IsDeleted = false;
        item.DeletedAt = null;

        await _unitOfWork.CommitAsync(cancellationToken);

        return new MenuItemResponse(
            item.Id,
            item.CategoryId,
            item.Name,
            item.Description,
            item.BasePrice,
            item.IsAvailable,
            item.ImageUrl,
            item.DisplayOrder,
            item.Allergens,
            item.DietaryLabels
        );
    }
}
