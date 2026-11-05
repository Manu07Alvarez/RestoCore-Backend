namespace RestoCore.SecurityTests.MultiTenancy;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestoCore.Domain.Entities;
using RestoCore.Infrastructure.MultiTenancy;
using RestoCore.Infrastructure.Persistence;
using Xunit;

public class AnalyticsTenantIsolationTests
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
    public async Task AnalyticsRollUpTables_InRealPostgres_EnforceStrictTenantIsolation()
    {
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await using (var adminCtx = CreateContext())
        {
            const string ensureSql = """
                ALTER TABLE tenants ADD COLUMN IF NOT EXISTS "PlanTier" character varying(30) NOT NULL DEFAULT 'Standard';
                ALTER TABLE menu_items ADD COLUMN IF NOT EXISTS "EstimatedCost" numeric(10,2);
                CREATE TABLE IF NOT EXISTS daily_menu_traffic_summaries (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "TenantId" uuid NOT NULL REFERENCES tenants ("Id") ON DELETE CASCADE,
                    "DateUtc" date NOT NULL,
                    "QrVisitsCount" integer NOT NULL DEFAULT 0,
                    "MobileCount" integer NOT NULL DEFAULT 0,
                    "DesktopCount" integer NOT NULL DEFAULT 0,
                    "TabletCount" integer NOT NULL DEFAULT 0,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL
                );
                """;
            await adminCtx.Database.ExecuteSqlRawAsync(ensureSql);

            adminCtx.Tenants.AddRange(
                new Tenant { Id = tenantAId, Name = "Analytics Tenant A", Slug = "analytics-a-" + Guid.NewGuid().ToString("N")[..8] },
                new Tenant { Id = tenantBId, Name = "Analytics Tenant B", Slug = "analytics-b-" + Guid.NewGuid().ToString("N")[..8] });

            adminCtx.DailyMenuTrafficSummaries.AddRange(
                new DailyMenuTrafficSummary { Id = Guid.NewGuid(), TenantId = tenantAId, DateUtc = today, QrVisitsCount = 500, MobileCount = 450, DesktopCount = 50 },
                new DailyMenuTrafficSummary { Id = Guid.NewGuid(), TenantId = tenantBId, DateUtc = today, QrVisitsCount = 900, MobileCount = 800, DesktopCount = 100 });

            await adminCtx.SaveChangesAsync();
        }

        await using (var ctxA = CreateContext(tenantAId))
        {
            var summariesForA = await ctxA.DailyMenuTrafficSummaries.ToListAsync();
            summariesForA.Should().ContainSingle();
            summariesForA[0].TenantId.Should().Be(tenantAId);
            summariesForA[0].QrVisitsCount.Should().Be(500);
        }

        await using (var ctxB = CreateContext(tenantBId))
        {
            var summariesForB = await ctxB.DailyMenuTrafficSummaries.ToListAsync();
            summariesForB.Should().ContainSingle();
            summariesForB[0].TenantId.Should().Be(tenantBId);
            summariesForB[0].QrVisitsCount.Should().Be(900);
        }
    }
}
