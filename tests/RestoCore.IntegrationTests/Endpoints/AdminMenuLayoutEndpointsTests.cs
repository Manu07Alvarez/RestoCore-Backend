namespace RestoCore.IntegrationTests.Endpoints;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Features.MenuLayout.DTOs;
using RestoCore.Domain.Entities;
using RestoCore.Infrastructure.Persistence;
using RestoCore.IntegrationTests.Fixtures;
using Xunit;

public class AdminMenuLayoutEndpointsTests : IClassFixture<ContainerizedStackFixture>
{
    private readonly HttpClient _client;
    private readonly ContainerizedStackFixture _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AdminMenuLayoutEndpointsTests(ContainerizedStackFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PutLayout_WithValidCoordinates_PersistsAndReturns200()
    {
        var slug = $"layout-{Guid.NewGuid():N}"[..15];
        Guid tenantId;
        Guid dishId = Guid.NewGuid();

        // 1. Seed tenant and dish
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenant = new Tenant
            {
                Name = "Canvas Bistro",
                Slug = slug,
                Status = "Active"
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
            tenantId = tenant.Id;

            var cat = new Category { TenantId = tenantId, Name = "Entradas", DisplayOrder = 1, IsActive = true };
            db.Categories.Add(cat);
            var item = new MenuItem
            {
                Id = dishId,
                TenantId = tenantId,
                CategoryId = cat.Id,
                Name = "Empanada Criolla",
                BasePrice = 1200m,
                IsAvailable = true,
                DisplayOrder = 1
            };
            db.MenuItems.Add(item);
            await db.SaveChangesAsync();
        }

        // 2. Prepare PUT /api/v1/admin/menu/layout
        var layoutPayload = new LayoutConfigDto
        {
            CanvasEnabled = true,
            BackgroundColor = "#1A1A1A",
            BackgroundUrl = "http://localhost:8333/restocore-images/bg.webp",
            Elements = new List<CanvasElementDto>
            {
                new()
                {
                    DishId = dishId,
                    X = 150.0,
                    Y = 300.0,
                    ZIndex = 2,
                    Width = 280.0,
                    Height = 180.0
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/admin/menu/layout")
        {
            Content = JsonContent.Create(layoutPayload)
        };
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        request.Headers.Add("X-User-Role", "owner");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LayoutConfigDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.CanvasEnabled.Should().BeTrue();
        result.Elements.Should().HaveCount(1);
        result.Elements[0].DishId.Should().Be(dishId);

        // Verify database persistence
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedTenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            savedTenant.Should().NotBeNull();
            savedTenant!.LayoutConfig.Should().NotBeNull();
            savedTenant.LayoutConfig!.CanvasEnabled.Should().BeTrue();
            savedTenant.LayoutConfig.Elements.Should().HaveCount(1);
            savedTenant.LayoutConfig.Elements[0].X.Should().Be(150.0);
        }
    }

    [Fact]
    public async Task PutLayout_WithInvalidCoordinates_Returns400BadRequest()
    {
        var tenantId = Guid.NewGuid();
        var layoutPayload = new LayoutConfigDto
        {
            CanvasEnabled = true,
            Elements = new List<CanvasElementDto>
            {
                new()
                {
                    DishId = Guid.NewGuid(),
                    X = -50.0, // Invalid negative coordinate
                    Y = 100.0,
                    ZIndex = 0,
                    Width = 100,
                    Height = 100
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/admin/menu/layout")
        {
            Content = JsonContent.Create(layoutPayload)
        };
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        request.Headers.Add("X-User-Role", "owner");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutLayout_WithUnauthorizedRole_Returns403Forbidden()
    {
        var tenantId = Guid.NewGuid();
        var layoutPayload = new LayoutConfigDto { CanvasEnabled = true };

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/admin/menu/layout")
        {
            Content = JsonContent.Create(layoutPayload)
        };
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        request.Headers.Add("X-User-Role", "waiter"); // Waiter cannot update layout

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}