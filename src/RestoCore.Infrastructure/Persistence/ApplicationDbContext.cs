namespace RestoCore.Infrastructure.Persistence;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Domain.Common;
using RestoCore.Domain.Entities;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly IDistributedCache? _cache;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext,
        IDistributedCache? cache = null)
        : base(options)
    {
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<ModifierGroup> ModifierGroups => Set<ModifierGroup>();
    public DbSet<ModifierOption> ModifierOptions => Set<ModifierOption>();
    public DbSet<Table> Tables => Set<Table>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global Multi-Tenant Query Filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScopedEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ITenantScopedEntity.TenantId));
                var tenantIdValue = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
                
                // e.TenantId == CurrentTenantId
                var comparison = Expression.Equal(property, tenantIdValue);
                var lambda = Expression.Lambda(comparison, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public Guid CurrentTenantId => _tenantContext.TenantId ?? Guid.Empty;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ITenantScopedEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty && _tenantContext.TenantId.HasValue)
            {
                entry.Entity.TenantId = _tenantContext.TenantId.Value;
            }
        }

        List<Guid>? modifiedTenantIds = null;
        if (_cache != null)
        {
            modifiedTenantIds = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Select(e => e.Entity switch
                {
                    ITenantScopedEntity tse => tse.TenantId,
                    Tenant t => t.Id,
                    _ => Guid.Empty
                })
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_cache != null && modifiedTenantIds != null && modifiedTenantIds.Count > 0)
        {
            var slugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(_tenantContext.TenantSlug))
            {
                slugs.Add(_tenantContext.TenantSlug);
            }

            var dbSlugs = await Tenants.IgnoreQueryFilters()
                .Where(t => modifiedTenantIds.Contains(t.Id))
                .Select(t => t.Slug)
                .ToListAsync(cancellationToken);

            foreach (var s in dbSlugs)
            {
                slugs.Add(s);
            }

            foreach (var slug in slugs)
            {
                try
                {
                    await _cache.RemoveAsync($"menu_payload:{slug}:default", cancellationToken);
                    await _cache.RemoveAsync($"menu_etag:{slug}:default", cancellationToken);
                }
                catch
                {
                    // Non-blocking cache eviction failure
                }
            }
        }

        return result;
    }
}
