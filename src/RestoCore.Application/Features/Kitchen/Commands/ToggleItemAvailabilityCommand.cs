namespace RestoCore.Application.Features.Kitchen.Commands;

using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Common.Interfaces.Repositories;

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
    private readonly IKitchenRepository _kitchenRepository;

    public ToggleItemAvailabilityCommandHandler(IKitchenRepository kitchenRepository)
    {
        _kitchenRepository = kitchenRepository;
    }

    public async Task<ItemAvailabilityResponse> Handle(ToggleItemAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var item = await _kitchenRepository.GetItemForAvailabilityToggleAsync(request.ItemId, cancellationToken);
        if (item == null)
        {
            throw new KeyNotFoundException($"Menu item {request.ItemId} was not found for the current tenant.");
        }

        var success = await _kitchenRepository.UpdateItemAvailabilityWithRetryAsync(
            request.ItemId,
            request.IsAvailable,
            maxRetries: 3,
            cancellationToken);

        if (!success)
        {
            throw new InvalidOperationException($"Failed to update availability for item {request.ItemId} due to concurrency contention.");
        }

        return new ItemAvailabilityResponse(request.ItemId, request.IsAvailable, DateTimeOffset.UtcNow);
    }
}
