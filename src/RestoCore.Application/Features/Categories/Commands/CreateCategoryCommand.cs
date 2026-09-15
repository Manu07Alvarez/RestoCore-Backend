namespace RestoCore.Application.Features.Categories.Commands;

using RestoCore.Application.Common.Mediator;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;
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
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateCategoryCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
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

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.IsActive
        );
    }
}
