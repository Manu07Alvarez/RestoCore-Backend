namespace RestoCore.Application.Features.Kitchen.Commands;

using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;

public record ToggleItemAvailabilityCommand(
    Guid ItemId,
    bool IsAvailable
) : IRequest<ItemAvailabilityResponse>;

public record ItemAvailabilityResponse(
    Guid ItemId,
    bool IsAvailable,
    DateTimeOffset UpdatedAt
);

public class ToggleItemAvailabilityCommandHandler : IRequestHandler<ToggleItemAvailabilityCommand, ItemAvailabilityResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ToggleItemAvailabilityCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ItemAvailabilityResponse> Handle(ToggleItemAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var item = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == request.ItemId && m.TenantId == tenantId, cancellationToken);

        if (item == null)
        {
            throw new KeyNotFoundException($"Menu item {request.ItemId} was not found for the current tenant.");
        }

        item.IsAvailable = request.IsAvailable;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ItemAvailabilityResponse(item.Id, item.IsAvailable, item.UpdatedAt);
    }
}
