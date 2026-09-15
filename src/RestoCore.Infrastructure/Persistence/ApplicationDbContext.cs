namespace RestoCore.Infrastructure.Persistence;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Domain.Common;
using RestoCore.Domain.Entities;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<ApplicationDbContext>? _logger;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext,
        IDistributedCache? cache = null,
        ILogger<ApplicationDbContext>? logger = null)
        : base(options)
    {
        _tenantContext = tenantContext;
        _cache = cache;
        _logger = logger;
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
        HashSet<string>? slugsToInvalidate = null;

        if (_cache != null)
        {
            slugsToInvalidate = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(_tenantContext.TenantSlug))
            {
                slugsToInvalidate.Add(_tenantContext.TenantSlug);
            }

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                {
                    if (entry.Entity is Tenant tenant && !string.IsNullOrEmpty(tenant.Slug))
                    {
                        slugsToInvalidate.Add(tenant.Slug);
                    }
                    else if (entry.Entity is ITenantScopedEntity tse && tse.TenantId != Guid.Empty)
                    {
                        modifiedTenantIds ??= new List<Guid>();
                        modifiedTenantIds.Add(tse.TenantId);
                    }
                }
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_cache != null && slugsToInvalidate != null)
        {
            // Post-commit cache invalidation is strictly best-effort:
            // 1. Do not use the caller's cancellation token so cancellations after commit don't abort cleanup.
            // 2. Any failure must not cause SaveChangesAsync to fail since data is already persisted.
            try
            {
                if (modifiedTenantIds != null && modifiedTenantIds.Count > 0)
                {
                    var distinctTenantIds = modifiedTenantIds.Distinct().ToList();
                    var dbSlugs = await Tenants.IgnoreQueryFilters()
                        .Where(t => distinctTenantIds.Contains(t.Id))
                        .Select(t => t.Slug)
                        .ToListAsync(CancellationToken.None);

                    foreach (var s in dbSlugs)
                    {
                        slugsToInvalidate.Add(s);
                    }
                }

                foreach (var slug in slugsToInvalidate)
                {
                    // Bump tenant-wide menu version: invalidates all table tokens and default menu caches
                    var newVersion = Guid.NewGuid().ToString("N");
                    await _cache.SetStringAsync(
                        $"menu_version:{slug}",
                        newVersion,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
                        },
                        CancellationToken.None);

                    // Also proactively clean legacy default keys
                    await _cache.RemoveAsync($"menu_payload:{slug}:default", CancellationToken.None);
                    await _cache.RemoveAsync($"menu_etag:{slug}:default", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Best-effort post-commit menu cache invalidation failed after successful database commit.");
            }
        }

        return result;
    }
}
