namespace RestoCore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public class KitchenRepository : IKitchenRepository
{
    private readonly ApplicationDbContext _context;

    public KitchenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MenuItem?> GetItemForAvailabilityToggleAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
    }

    public async Task<bool> UpdateItemAvailabilityWithRetryAsync(
        Guid itemId, 
        bool isAvailable, 
        int maxRetries = 3, 
        CancellationToken cancellationToken = default)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var item = await _context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);

            if (item == null)
            {
                return false;
            }

            item.IsAvailable = isAvailable;
            item.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
            {
                // Detach or reload state before retry to clear stale ChangeTracker entries
                _context.Entry(item).State = EntityState.Detached;

                // Exponential backoff with small random jitter
                var delayMs = (int)(Math.Pow(2, attempt) * 25) + Random.Shared.Next(10, 50);
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        return false;
    }
}
