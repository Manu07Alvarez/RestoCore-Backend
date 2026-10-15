namespace RestoCore.Application.Features.Categories.Queries;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Categories.Commands;

public record GetCategoriesQuery() : IRequest<List<CategoryResponse>>;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryResponse>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryResponse>>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly ITenantContext _tenantContext;

    public GetCategoriesQueryHandler(
        ICatalogRepository catalogRepository,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _tenantContext = tenantContext;
    }

    public async Task<List<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var categories = await _catalogRepository.GetCategoriesByTenantAsync(tenantId, cancellationToken);

        return categories.Select(c => new CategoryResponse(
            c.Id,
            c.Name,
            c.Description,
            c.DisplayOrder,
            c.IsActive
        )).ToList();
    }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly ITenantContext _tenantContext;

    public GetCategoryByIdQueryHandler(
        ICatalogRepository catalogRepository,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _tenantContext = tenantContext;
    }

    public async Task<CategoryResponse> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var category = await _catalogRepository.GetCategoryByIdAsync(request.Id, cancellationToken);
        if (category == null || category.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Category {request.Id} was not found for the current tenant.");
        }

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.IsActive
        );
    }
}
