namespace RestoCore.Application.Features.MenuItems.Commands;

using RestoCore.Application.Common.Mediator;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Domain.Entities;

public record CreateMenuItemCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal BasePrice,
    string? ImageUrl,
    int DisplayOrder,
    string[] Allergens,
    string[] DietaryLabels
) : IRequest<MenuItemResponse>;

public record MenuItemResponse(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal BasePrice,
    bool IsAvailable,
    string? ImageUrl,
    int DisplayOrder,
    string[] Allergens,
    string[] DietaryLabels
);

public class CreateMenuItemCommandHandler : IRequestHandler<CreateMenuItemCommand, MenuItemResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateMenuItemCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<MenuItemResponse> Handle(CreateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.TenantId == tenantId, cancellationToken);

        if (!categoryExists)
        {
            throw new KeyNotFoundException($"Category {request.CategoryId} was not found for the current tenant.");
        }

        var item = new MenuItem
        {
            TenantId = tenantId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            BasePrice = request.BasePrice,
            IsAvailable = true,
            ImageUrl = request.ImageUrl?.Trim(),
            DisplayOrder = request.DisplayOrder,
            Allergens = request.Allergens ?? Array.Empty<string>(),
            DietaryLabels = request.DietaryLabels ?? Array.Empty<string>()
        };

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

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
