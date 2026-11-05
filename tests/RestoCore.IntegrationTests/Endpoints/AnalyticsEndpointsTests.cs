namespace RestoCore.IntegrationTests.Endpoints;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Features.Analytics.DTOs;
using RestoCore.Domain.Entities;
using RestoCore.Infrastructure.Persistence;
using RestoCore.IntegrationTests.Fixtures;
using Xunit;

public class AnalyticsEndpointsTests : IClassFixture<ContainerizedStackFixture>
{
    private readonly HttpClient _client;
    private readonly ContainerizedStackFixture _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AnalyticsEndpointsTests(ContainerizedStackFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task EnsureAnalyticsTablesAsync(ApplicationDbContext db)
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
            CREATE TABLE IF NOT EXISTS daily_item_view_summaries (
                "Id" uuid NOT NULL PRIMARY KEY,
                "TenantId" uuid NOT NULL REFERENCES tenants ("Id") ON DELETE CASCADE,
                "ItemId" uuid NOT NULL REFERENCES menu_items ("Id") ON DELETE CASCADE,
                "DateUtc" date NOT NULL,
                "ViewsCount" integer NOT NULL DEFAULT 0,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL
            );
            CREATE TABLE IF NOT EXISTS daily_category_interaction_summaries (
                "Id" uuid NOT NULL PRIMARY KEY,
                "TenantId" uuid NOT NULL REFERENCES tenants ("Id") ON DELETE CASCADE,
                "CategoryId" uuid NOT NULL REFERENCES categories ("Id") ON DELETE CASCADE,
                "DateUtc" date NOT NULL,
                "InteractionsCount" integer NOT NULL DEFAULT 0,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL
            );
            CREATE TABLE IF NOT EXISTS hourly_menu_traffic_summaries (
                "Id" uuid NOT NULL PRIMARY KEY,
                "TenantId" uuid NOT NULL REFERENCES tenants ("Id") ON DELETE CASCADE,
                "DateUtc" date NOT NULL,
                "HourOfDay" integer NOT NULL,
                "VisitsCount" integer NOT NULL DEFAULT 0,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL
            );
            """;
        await db.Database.ExecuteSqlRawAsync(ensureSql);
    }

    [Fact]
    public async Task CollectAnalyticsEvent_WithValidPayload_Returns202AcceptedAndAggregatesInBackground()
    {
        var slug = $"analytics-{Guid.NewGuid():N}"[..16];
        Guid tenantId;
        Guid itemId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await EnsureAnalyticsTablesAsync(db);

            var tenant = new Tenant
            {
                Name = "Analytics Test Bistro",
                Slug = slug,
                Status = "Active",
                PlanTier = "Pro"
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
            tenantId = tenant.Id;

            var cat = new Category { TenantId = tenantId, Name = "Carnes", DisplayOrder = 1, IsActive = true };
            db.Categories.Add(cat);

            var item = new MenuItem
            {
                Id = itemId,
                TenantId = tenantId,
                CategoryId = cat.Id,
                Name = "Bife de Chorizo",
                BasePrice = 20000m,
                EstimatedCost = 6000m,
                IsAvailable = true
            };
            db.MenuItems.Add(item);
            await db.SaveChangesAsync();
        }

        // Warm up route & connection
        await _client.PostAsJsonAsync("/api/v1/analytics/collect", new AnalyticsEventRequest("menu_view", slug, null, null, "mobile"));

        // Measure ingestion latency (< 50ms target)
        var sw = Stopwatch.StartNew();
        var collectResponse = await _client.PostAsJsonAsync("/api/v1/analytics/collect", new AnalyticsEventRequest("item_view", slug, itemId, null, "mobile"));
        sw.Stop();

        collectResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        sw.ElapsedMilliseconds.Should().BeLessThan(150);

        // Wait briefly for background worker aggregation and query dashboard
        await Task.Delay(300);

        var dashboardRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/analytics/dashboard");
        dashboardRequest.Headers.Add("X-Tenant-Id", tenantId.ToString());
        dashboardRequest.Headers.Add("X-User-Role", "owner");

        var dashboardResponse = await _client.SendAsync(dashboardRequest);
        dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<AnalyticsDashboardResponse>(JsonOptions);
        dashboard.Should().NotBeNull();
        dashboard!.TenantId.Should().Be(tenantId);
        dashboard.PriorityHighMetrics.TotalQrVisits.Should().BeGreaterThanOrEqualTo(1);
        dashboard.PriorityHighMetrics.TopViewedItems.Should().Contain(x => x.ItemId == itemId && x.ViewsCount >= 1);
        dashboard.PriorityHighMetrics.MenuEngineeringMatrix.Stars.Should().Contain(itemId);
        dashboard.PriorityMediumMetrics.Should().NotBeNull();
    }

    [Fact]
    public async Task CollectAnalyticsEvent_WithInvalidEventType_Returns400BadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/analytics/collect",
            new AnalyticsEventRequest("invalid_event", "some-slug"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAnalyticsDashboard_WithWaiterRole_Returns403Forbidden()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/analytics/dashboard");
        request.Headers.Add("X-Tenant-Id", Guid.NewGuid().ToString());
        request.Headers.Add("X-User-Role", "waiter");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
