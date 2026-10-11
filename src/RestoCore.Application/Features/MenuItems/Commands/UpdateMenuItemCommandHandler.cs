namespace RestoCore.Application.Features.MenuItems.Commands;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;

public class UpdateMenuItemCommandHandler : IRequestHandler<UpdateMenuItemCommand, MenuItemResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateMenuItemCommandHandler(
        ICatalogRepository catalogRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<MenuItemResponse> Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var item = await _catalogRepository.GetMenuItemByIdAsync(request.Id, cancellationToken);
        if (item == null || item.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Menu item {request.Id} was not found.");
        }

        var category = await _catalogRepository.GetCategoryByIdAsync(request.CategoryId, cancellationToken);
        if (category == null || category.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Category {request.CategoryId} was not found for the current tenant.");
        }

        item.CategoryId = request.CategoryId;
        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.BasePrice = request.BasePrice;
        item.IsAvailable = request.IsAvailable;
        item.ImageUrl = request.ImageUrl?.Trim();
        item.DisplayOrder = request.DisplayOrder;
        item.Allergens = request.Allergens ?? Array.Empty<string>();
        item.DietaryLabels = request.DietaryLabels ?? Array.Empty<string>();

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
