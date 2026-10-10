namespace RestoCore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
    }

    public async Task<Tenant?> GetTenantBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<bool> CustomDomainExistsAsync(string customDomain, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.CustomDomain == customDomain, cancellationToken);
    }

    public void AddTenant(Tenant tenant)
    {
        _context.Tenants.Add(tenant);
    }

    public async Task<Table?> GetTableWithTenantByIdAsync(Guid tableId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Tables
            .Include(t => t.Tenant)
            .FirstOrDefaultAsync(t => t.Id == tableId && t.TenantId == tenantId, cancellationToken);
    }

    public async Task<Table?> GetTableByNumberAsync(Guid tenantId, int tableNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Tables
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.TableNumber == tableNumber, cancellationToken);
    }

    public async Task<Table?> GetTableByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Tables
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public void AddTable(Table table)
    {
        _context.Tables.Add(table);
    }
}
