namespace RestoCore.Application.Features.Categories.Commands;

using System.Text.Json.Serialization;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    [property: JsonPropertyName("sort_order")] int DisplayOrder,
    [property: JsonPropertyName("is_visible")] bool IsActive
) : IRequest<CategoryResponse>;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICdnPurgeService? _cdnPurgeService;

    public UpdateCategoryCommandHandler(
        ICatalogRepository catalogRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICdnPurgeService? cdnPurgeService = null)
    {
        _catalogRepository = catalogRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _cdnPurgeService = cdnPurgeService;
    }

    public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var category = await _catalogRepository.GetCategoryByIdAsync(request.Id, cancellationToken);
        if (category == null || category.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Category {request.Id} was not found for the current tenant.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(request.Name));
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;

        await _unitOfWork.CommitAsync(cancellationToken);

        if (_cdnPurgeService != null && !string.IsNullOrWhiteSpace(_tenantContext.TenantSlug))
        {
            await _cdnPurgeService.PurgeTenantCacheAsync(_tenantContext.TenantSlug, cancellationToken);
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
