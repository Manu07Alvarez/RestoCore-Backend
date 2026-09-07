namespace RestoCore.SecurityTests.MultiTenancy;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Domain.Entities;
using RestoCore.Infrastructure.MultiTenancy;
using RestoCore.Infrastructure.Persistence;
using Xunit;

public class CrossTenantDatabaseIsolationTests
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=restocore_dev;Username=postgres;Password=postgres_dev_password";

    private ApplicationDbContext CreateContext(Guid? tenantId = null)
    {
        var tenantContext = new TenantContext();
        if (tenantId.HasValue)
        {
            tenantContext.SetTenant(tenantId.Value, "tenant-" + tenantId.Value.ToString("N"));
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new ApplicationDbContext(options, tenantContext);
    }

    [Fact]
    public async Task GlobalQueryFilters_InRealPostgres_StrictlyPartitionsDataAcrossTenants()
    {
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        // 1. Seed two distinct tenants and categories directly
        await using (var adminCtx = CreateContext())
        {
            var tenantA = new Tenant { Id = tenantAId, Name = "Trattoria A", Slug = "slug-a-" + Guid.NewGuid().ToString("N")[..8] };
            var tenantB = new Tenant { Id = tenantBId, Name = "Bistro B", Slug = "slug-b-" + Guid.NewGuid().ToString("N")[..8] };
            adminCtx.Tenants.AddRange(tenantA, tenantB);

            var catA = new Category { Id = Guid.NewGuid(), TenantId = tenantAId, Name = "Pastas de A", DisplayOrder = 1, IsActive = true };
            var catB = new Category { Id = Guid.NewGuid(), TenantId = tenantBId, Name = "Carnes de B", DisplayOrder = 1, IsActive = true };
            adminCtx.Categories.AddRange(catA, catB);

            await adminCtx.SaveChangesAsync();
        }

        // 2. Query as Tenant A -> MUST ONLY see Tenant A categories
        await using (var ctxA = CreateContext(tenantAId))
        {
            var categoriesForA = await ctxA.Categories.ToListAsync();

            categoriesForA.Should().NotBeEmpty();
            categoriesForA.Should().OnlyContain(c => c.TenantId == tenantAId);
            categoriesForA.Should().NotContain(c => c.TenantId == tenantBId);
            categoriesForA.Should().Contain(c => c.Name == "Pastas de A");
        }

        // 3. Query as Tenant B -> MUST ONLY see Tenant B categories
        await using (var ctxB = CreateContext(tenantBId))
        {
            var categoriesForB = await ctxB.Categories.ToListAsync();

            categoriesForB.Should().NotBeEmpty();
            categoriesForB.Should().OnlyContain(c => c.TenantId == tenantBId);
            categoriesForB.Should().NotContain(c => c.TenantId == tenantAId);
            categoriesForB.Should().Contain(c => c.Name == "Carnes de B");
        }
    }
}
