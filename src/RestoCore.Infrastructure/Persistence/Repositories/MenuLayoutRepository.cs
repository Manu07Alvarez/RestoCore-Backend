namespace RestoCore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public class MenuLayoutRepository : IMenuLayoutRepository
{
    private readonly ApplicationDbContext _context;

    public MenuLayoutRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetTenantForLayoutAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
    }

    public async Task<MenuPublishJob?> GetPublishJobByIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuPublishJobs
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
    }

    public void AddPublishJob(MenuPublishJob job)
    {
        _context.MenuPublishJobs.Add(job);
    }

    public async Task<List<MenuItem>> GetPublishedMenuItemsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetPublishedCategoriesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}
