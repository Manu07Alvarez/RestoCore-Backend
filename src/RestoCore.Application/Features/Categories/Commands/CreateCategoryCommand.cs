namespace RestoCore.Application.Features.Categories.Commands;

using System.Text.Json.Serialization;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public record CreateCategoryCommand(
    string Name,
    string? Description,
    [property: JsonPropertyName("sort_order")] int DisplayOrder = 0,
    [property: JsonPropertyName("is_visible")] bool IsActive = true
) : IRequest<CategoryResponse>;

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    [property: JsonPropertyName("sort_order")] int DisplayOrder,
    [property: JsonPropertyName("is_visible")] bool IsActive
);

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICdnPurgeService? _cdnPurgeService;

    public CreateCategoryCommandHandler(
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

    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var category = new Category
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        _catalogRepository.AddCategory(category);
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
