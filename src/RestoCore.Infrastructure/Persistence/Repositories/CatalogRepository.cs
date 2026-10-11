namespace RestoCore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public class CatalogRepository : ICatalogRepository
{
    private readonly ApplicationDbContext _context;

    public CatalogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> CategoryExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public void AddCategory(Category category)
    {
        _context.Categories.Add(category);
    }

    public void RemoveCategory(Category category)
    {
        _context.Categories.Remove(category);
    }

    public async Task<MenuItem?> GetMenuItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<MenuItem?> GetMenuItemByIdWithDeletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == _context.CurrentTenantId, cancellationToken);
    }

    public async Task<List<MenuItem>> GetArchivedMenuItemsOlderThanAsync(DateTimeOffset threshold, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .IgnoreQueryFilters()
            .Where(m => m.IsDeleted && m.DeletedAt != null && m.DeletedAt <= threshold)
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItem?> GetMenuItemWithModifiersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Include(m => m.ModifierGroups)
            .ThenInclude(g => g.Options)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public void AddMenuItem(MenuItem menuItem)
    {
        _context.MenuItems.Add(menuItem);
    }

    public void RemoveMenuItem(MenuItem menuItem)
    {
        _context.MenuItems.Remove(menuItem);
    }

    public async Task<ModifierGroup?> GetModifierGroupByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ModifierGroups
            .Include(g => g.Options)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public void AddModifierGroup(ModifierGroup modifierGroup)
    {
        _context.ModifierGroups.Add(modifierGroup);
    }

    public void RemoveModifierGroup(ModifierGroup modifierGroup)
    {
        _context.ModifierGroups.Remove(modifierGroup);
    }

    public async Task<Tenant?> GetTenantWithCatalogAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(t => t.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Items.OrderBy(m => m.DisplayOrder))
                    .ThenInclude(m => m.ModifierGroups)
                        .ThenInclude(mg => mg.Options)
            .FirstOrDefaultAsync(t => t.Slug == tenantSlug && t.Status == "Active", cancellationToken);
    }

    public async Task<Table?> GetActiveTableByTokenAsync(Guid tenantId, string tableToken, CancellationToken cancellationToken = default)
    {
        return await _context.Tables
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Token == tableToken && t.IsActive, cancellationToken);
    }
}
