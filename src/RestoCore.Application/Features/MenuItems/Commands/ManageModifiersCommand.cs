namespace RestoCore.Application.Features.MenuItems.Commands;

using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public record ModifierOptionInput(
    string Name,
    decimal PriceDelta,
    bool IsAvailable
);

public record AddModifierGroupCommand(
    Guid MenuItemId,
    string Name,
    int MinSelection,
    int MaxSelection,
    bool IsRequired,
    List<ModifierOptionInput> Options
) : IRequest<ModifierGroupResponse>;

public record ModifierGroupResponse(
    Guid Id,
    Guid MenuItemId,
    string Name,
    int MinSelection,
    int MaxSelection,
    bool IsRequired,
    List<ModifierOptionResponse> Options
);

public record ModifierOptionResponse(
    Guid Id,
    string Name,
    decimal PriceDelta,
    bool IsAvailable
);

public class AddModifierGroupCommandHandler : IRequestHandler<AddModifierGroupCommand, ModifierGroupResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AddModifierGroupCommandHandler(
        ICatalogRepository catalogRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _catalogRepository = catalogRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ModifierGroupResponse> Handle(AddModifierGroupCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var menuItem = await _catalogRepository.GetMenuItemByIdAsync(request.MenuItemId, cancellationToken);

        if (menuItem == null || menuItem.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Menu item {request.MenuItemId} was not found for the current tenant.");
        }

        var group = new ModifierGroup
        {
            TenantId = tenantId,
            MenuItemId = menuItem.Id,
            Name = request.Name.Trim(),
            MinSelection = request.MinSelection,
            MaxSelection = request.MaxSelection,
            IsRequired = request.IsRequired
        };

        if (request.Options != null)
        {
            foreach (var opt in request.Options)
            {
                group.Options.Add(new ModifierOption
                {
                    TenantId = tenantId,
                    Name = opt.Name.Trim(),
                    PriceDelta = opt.PriceDelta,
                    IsAvailable = opt.IsAvailable
                });
            }
        }

        _catalogRepository.AddModifierGroup(group);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new ModifierGroupResponse(
            group.Id,
            group.MenuItemId,
            group.Name,
            group.MinSelection,
            group.MaxSelection,
            group.IsRequired,
            group.Options.Select(o => new ModifierOptionResponse(o.Id, o.Name, o.PriceDelta, o.IsAvailable)).ToList()
        );
    }
}
