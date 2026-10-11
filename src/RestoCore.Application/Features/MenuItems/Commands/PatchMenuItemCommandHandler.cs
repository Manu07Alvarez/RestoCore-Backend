namespace RestoCore.Application.Features.MenuItems.Commands;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;

public class PatchMenuItemCommandHandler : IRequestHandler<PatchMenuItemCommand, MenuItemResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public PatchMenuItemCommandHandler(
        ICatalogRepository catalogRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<MenuItemResponse> Handle(PatchMenuItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var item = await _catalogRepository.GetMenuItemByIdAsync(request.Id, cancellationToken);
        if (item == null || item.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Menu item {request.Id} was not found.");
        }

        if (request.CategoryId.HasValue)
        {
            var category = await _catalogRepository.GetCategoryByIdAsync(request.CategoryId.Value, cancellationToken);
            if (category == null || category.TenantId != tenantId)
            {
                throw new KeyNotFoundException($"Category {request.CategoryId.Value} was not found for the current tenant.");
            }
            item.CategoryId = request.CategoryId.Value;
        }

        if (request.Name != null) item.Name = request.Name.Trim();
        if (request.Description != null) item.Description = request.Description.Trim();
        if (request.BasePrice.HasValue) item.BasePrice = request.BasePrice.Value;
        if (request.IsAvailable.HasValue) item.IsAvailable = request.IsAvailable.Value;
        if (request.ImageUrl != null) item.ImageUrl = request.ImageUrl.Trim();
        if (request.DisplayOrder.HasValue) item.DisplayOrder = request.DisplayOrder.Value;
        if (request.Allergens != null) item.Allergens = request.Allergens;
        if (request.DietaryLabels != null) item.DietaryLabels = request.DietaryLabels;

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
