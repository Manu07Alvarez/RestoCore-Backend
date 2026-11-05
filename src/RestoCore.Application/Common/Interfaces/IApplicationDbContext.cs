namespace RestoCore.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using RestoCore.Domain.Entities;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Category> Categories { get; }
    DbSet<MenuItem> MenuItems { get; }
    DbSet<ModifierGroup> ModifierGroups { get; }
    DbSet<ModifierOption> ModifierOptions { get; }
    DbSet<Table> Tables { get; }
    DbSet<MenuPublishJob> MenuPublishJobs { get; }
    DbSet<User> Users { get; }
    DbSet<DailyMenuTrafficSummary> DailyMenuTrafficSummaries { get; }
    DbSet<DailyItemViewSummary> DailyItemViewSummaries { get; }
    DbSet<DailyCategoryInteractionSummary> DailyCategoryInteractionSummaries { get; }
    DbSet<HourlyMenuTrafficSummary> HourlyMenuTrafficSummaries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
