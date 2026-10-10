namespace RestoCore.Application.Common.Interfaces.Repositories;

using RestoCore.Domain.Entities;

public interface ICatalogRepository
{
    Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CategoryExistsAsync(string name, CancellationToken cancellationToken = default);
    void AddCategory(Category category);
    void RemoveCategory(Category category);

    Task<MenuItem?> GetMenuItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MenuItem?> GetMenuItemWithModifiersAsync(Guid id, CancellationToken cancellationToken = default);
    void AddMenuItem(MenuItem menuItem);
    void RemoveMenuItem(MenuItem menuItem);

    Task<ModifierGroup?> GetModifierGroupByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void AddModifierGroup(ModifierGroup modifierGroup);
    void RemoveModifierGroup(ModifierGroup modifierGroup);

    Task<Tenant?> GetTenantWithCatalogAsync(string tenantSlug, CancellationToken cancellationToken = default);
    Task<Table?> GetActiveTableByTokenAsync(Guid tenantId, string tableToken, CancellationToken cancellationToken = default);
}
