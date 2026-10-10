namespace RestoCore.Application.Common.Interfaces.Repositories;

using RestoCore.Domain.Entities;

public interface IKitchenRepository
{
    Task<MenuItem?> GetItemForAvailabilityToggleAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<bool> UpdateItemAvailabilityWithRetryAsync(
        Guid itemId, 
        bool isAvailable, 
        int maxRetries = 3, 
        CancellationToken cancellationToken = default);
}
