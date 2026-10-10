namespace RestoCore.Application.Common.Interfaces.Repositories;

using RestoCore.Domain.Entities;

public interface ITenantRepository
{
    Task<Tenant?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> CustomDomainExistsAsync(string customDomain, CancellationToken cancellationToken = default);
    void AddTenant(Tenant tenant);

    Task<Table?> GetTableWithTenantByIdAsync(Guid tableId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<Table?> GetTableByNumberAsync(Guid tenantId, int tableNumber, CancellationToken cancellationToken = default);
    Task<Table?> GetTableByTokenAsync(string token, CancellationToken cancellationToken = default);
    void AddTable(Table table);
}
