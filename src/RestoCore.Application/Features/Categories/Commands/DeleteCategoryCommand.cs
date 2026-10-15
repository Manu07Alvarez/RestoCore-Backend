namespace RestoCore.Application.Features.Categories.Commands;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;

public record DeleteCategoryCommand(Guid Id) : IRequest<bool>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICdnPurgeService? _cdnPurgeService;

    public DeleteCategoryCommandHandler(
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

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var category = await _catalogRepository.GetCategoryByIdAsync(request.Id, cancellationToken);
        if (category == null || category.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Category {request.Id} was not found for the current tenant.");
        }

        var hasItems = await _catalogRepository.HasMenuItemsForCategoryAsync(request.Id, cancellationToken);
        if (hasItems)
        {
            throw new InvalidOperationException(
                "No es posible eliminar la categoría porque contiene platos gastronómicos asociados (activos o archivados). Reasigne o elimine los platos antes de proceder.");
        }

        _catalogRepository.RemoveCategory(category);
        await _unitOfWork.CommitAsync(cancellationToken);

        if (_cdnPurgeService != null && !string.IsNullOrWhiteSpace(_tenantContext.TenantSlug))
        {
            await _cdnPurgeService.PurgeTenantCacheAsync(_tenantContext.TenantSlug, cancellationToken);
        }

        return true;
    }
}
