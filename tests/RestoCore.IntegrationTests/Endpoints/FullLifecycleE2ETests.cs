namespace RestoCore.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Features.PublicMenu.DTOs;
using RestoCore.Domain.Entities;
using RestoCore.Infrastructure.Persistence;
using RestoCore.IntegrationTests.Fixtures;
using Xunit;

public class FullLifecycleE2ETests : IClassFixture<ContainerizedStackFixture>
{
    private readonly HttpClient _client;
    private readonly ContainerizedStackFixture _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public FullLifecycleE2ETests(ContainerizedStackFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteLifecycle_TenantToPublicMenuAndQr_SucceedsEndToEnd()
    {
        var slug = $"bodegon-{Guid.NewGuid():N}".Substring(0, 15);
        Guid tenantId;

        // 1. Provision Tenant directly into DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenant = new Tenant
            {
                Name = "El Bodegon Test",
                Slug = slug,
                Status = "Active"
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
            tenantId = tenant.Id;
        }

        // 2. Add Category and MenuItem in DB (set TenantContext in scope)
        Guid categoryId;
        Guid itemId;
        string tableToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(tenantId, slug);

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new Category
            {
                TenantId = tenantId,
                Name = "Especialidades",
                DisplayOrder = 1,
                IsActive = true
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            categoryId = category.Id;

            var item = new MenuItem
            {
                TenantId = tenantId,
                CategoryId = categoryId,
                Name = "Asado de Tira",
                Description = "Corte clasico argentino a la lena",
                BasePrice = 16500.00m,
                IsAvailable = true,
                DisplayOrder = 1,
                Allergens = Array.Empty<string>(),
                DietaryLabels = new[] { "sin-tacc" }
            };
            db.MenuItems.Add(item);

            tableToken = $"tok-{Guid.NewGuid():N}".Substring(0, 20);
            var table = new Table
            {
                TenantId = tenantId,
                TableNumber = 12,
                Token = tableToken,
                IsActive = true
            };
            db.Tables.Add(table);

            await db.SaveChangesAsync();
            itemId = item.Id;
        }

        // 3. Query Public Digital Menu via HTTP (both default and with table_token)
        var menuResponse = await _client.GetAsync($"/api/v1/tenants/{slug}/menu");
        menuResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        menuResponse.Headers.ETag.Should().NotBeNull();
        menuResponse.Headers.CacheControl.Should().NotBeNull();

        var menuData = await menuResponse.Content.ReadFromJsonAsync<PublicMenuResponse>(JsonOptions);
        menuData.Should().NotBeNull();
        menuData!.TenantSlug.Should().Be(slug);
        menuData.Categories.Should().Contain(c => c.Name == "Especialidades");

        var specialtyCategory = menuData.Categories.First(c => c.Name == "Especialidades");
        specialtyCategory.Items.Should().Contain(i => i.Name == "Asado de Tira" && i.IsAvailable);

        var tableMenuResponse = await _client.GetAsync($"/api/v1/tenants/{slug}/menu?table_token={tableToken}");
        tableMenuResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tableMenuData = await tableMenuResponse.Content.ReadFromJsonAsync<PublicMenuResponse>(JsonOptions);
        tableMenuData.Should().NotBeNull();
        tableMenuData!.TableNumber.Should().Be(12);

        // 4. Test ETag 304 Not Modified
        var etag = menuResponse.Headers.ETag!.Tag;
        var conditionalRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tenants/{slug}/menu");
        conditionalRequest.Headers.TryAddWithoutValidation("If-None-Match", etag);

        var conditionalResponse = await _client.SendAsync(conditionalRequest);
        conditionalResponse.StatusCode.Should().Be(HttpStatusCode.NotModified);

        // 5. Kitchen Staff toggles availability to FALSE directly via DB using IgnoreQueryFilters
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbItem = await db.MenuItems.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == itemId);
            dbItem!.IsAvailable = false;
            await db.SaveChangesAsync();
        }

        // 6. Public menu query immediately reflects item as unavailable for both default and table_token queries
        var updatedMenuResponse = await _client.GetAsync($"/api/v1/tenants/{slug}/menu");
        var updatedMenuData = await updatedMenuResponse.Content.ReadFromJsonAsync<PublicMenuResponse>(JsonOptions);
        updatedMenuData.Should().NotBeNull();
        var updatedCategory = updatedMenuData!.Categories.First(c => c.Name == "Especialidades");
        updatedCategory.Items.First(i => i.Name == "Asado de Tira").IsAvailable.Should().BeFalse();

        var updatedTableMenuResponse = await _client.GetAsync($"/api/v1/tenants/{slug}/menu?table_token={tableToken}");
        var updatedTableMenuData = await updatedTableMenuResponse.Content.ReadFromJsonAsync<PublicMenuResponse>(JsonOptions);
        updatedTableMenuData.Should().NotBeNull();
        var updatedTableCategory = updatedTableMenuData!.Categories.First(c => c.Name == "Especialidades");
        updatedTableCategory.Items.First(i => i.Name == "Asado de Tira").IsAvailable.Should().BeFalse();
    }
}
