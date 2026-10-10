namespace RestoCore.Application.Common.Interfaces.Repositories;

using RestoCore.Domain.Entities;

public interface IMenuLayoutRepository
{
    Task<Tenant?> GetTenantForLayoutAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<MenuPublishJob?> GetPublishJobByIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    void AddPublishJob(MenuPublishJob job);
    Task<List<MenuItem>> GetPublishedMenuItemsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Category>> GetPublishedCategoriesAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
