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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
