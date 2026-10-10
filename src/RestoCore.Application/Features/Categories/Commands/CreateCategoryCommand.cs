namespace RestoCore.Application.Features.Categories.Commands;

using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public record CreateCategoryCommand(
    string Name,
    string? Description,
    int DisplayOrder
) : IRequest<CategoryResponse>;

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive
);

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateCategoryCommandHandler(
        ICatalogRepository catalogRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
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
            IsActive = true
        };

        _catalogRepository.AddCategory(category);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.IsActive
        );
    }
}
