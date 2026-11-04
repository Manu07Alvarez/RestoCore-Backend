namespace RestoCore.Application.Features.MenuItems.Queries;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.MenuItems.Commands;

public record GetMenuItemsQuery(Guid? CategoryId = null, bool IncludeDeleted = false) : IRequest<List<MenuItemResponse>>;

public record GetMenuItemByIdQuery(Guid Id) : IRequest<MenuItemResponse>;

public class GetMenuItemsQueryHandler : IRequestHandler<GetMenuItemsQuery, List<MenuItemResponse>>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly ITenantContext _tenantContext;

    public GetMenuItemsQueryHandler(
        ICatalogRepository catalogRepository,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _tenantContext = tenantContext;
    }

    public async Task<List<MenuItemResponse>> Handle(GetMenuItemsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var items = await _catalogRepository.GetMenuItemsByTenantAsync(
            tenantId,
            request.CategoryId,
            request.IncludeDeleted,
            cancellationToken);

        return items.Select(item => new MenuItemResponse(
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
        )).ToList();
    }
}

public class GetMenuItemByIdQueryHandler : IRequestHandler<GetMenuItemByIdQuery, MenuItemResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly ITenantContext _tenantContext;

    public GetMenuItemByIdQueryHandler(
        ICatalogRepository catalogRepository,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _tenantContext = tenantContext;
    }

    public async Task<MenuItemResponse> Handle(GetMenuItemByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var item = await _catalogRepository.GetMenuItemByIdAsync(request.Id, cancellationToken);
        if (item == null || item.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Menu item {request.Id} was not found for the current tenant.");
        }

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
