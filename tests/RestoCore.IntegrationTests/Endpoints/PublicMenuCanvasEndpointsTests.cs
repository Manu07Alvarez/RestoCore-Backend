namespace RestoCore.IntegrationTests.Endpoints;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Features.PublicMenu.DTOs;
using RestoCore.Domain.Entities;
using RestoCore.Domain.ValueObjects;
using RestoCore.Infrastructure.Persistence;
using RestoCore.IntegrationTests.Fixtures;
using Xunit;

public class PublicMenuCanvasEndpointsTests : IClassFixture<ContainerizedStackFixture>
{
    private readonly HttpClient _client;
    private readonly ContainerizedStackFixture _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PublicMenuCanvasEndpointsTests(ContainerizedStackFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPublicMenu_WithCanvasLayout_DeliversPrunedLayoutAndSupportsEtag304()
    {
        var slug = $"canvas-{Guid.NewGuid():N}"[..15];
        Guid tenantId;
        Guid activeDishId = Guid.NewGuid();
        Guid orphanDishId = Guid.NewGuid();

        // 1. Seed tenant with catalog and canvas layout
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenant = new Tenant
            {
                Name = "Canvas Trattoria",
                Slug = slug,
                Status = "Active",
                LayoutConfig = new LayoutConfig
                {
                    CanvasEnabled = true,
                    BackgroundColor = "#0F172A",
                    BackgroundUrl = "https://cdn.restocore.com/canvas-bg.webp",
                    Elements = new List<CanvasElement>
                    {
                        new()
                        {
                            DishId = activeDishId,
                            X = 120.0,
                            Y = 240.0,
                            ZIndex = 1,
                            Width = 300.0,
                            Height = 200.0
                        },
                        new()
                        {
                            DishId = orphanDishId, // Orphan: Not in catalog
                            X = 450.0,
                            Y = 240.0,
                            ZIndex = 1,
                            Width = 300.0,
                            Height = 200.0
                        }
                    }
                }
            };
            db.Tenants.Add(tenant);

            var category = new Category
            {
                TenantId = tenant.Id,
                Name = "Pastas Caseras",
                DisplayOrder = 1,
                IsActive = true
            };
            db.Categories.Add(category);

            var item = new MenuItem
            {
                Id = activeDishId,
                TenantId = tenant.Id,
                CategoryId = category.Id,
                Name = "Ravioles de Ricota",
                BasePrice = 9500.00m,
                IsAvailable = true,
                DisplayOrder = 1
            };
            db.MenuItems.Add(item);

            await db.SaveChangesAsync();
            tenantId = tenant.Id;
        }

        // 2. Fetch public menu
        var response = await _client.GetAsync($"/api/v1/tenants/{slug}/menu");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.ETag.Should().NotBeNull();
        response.Headers.CacheControl.Should().NotBeNull();

        var menu = await response.Content.ReadFromJsonAsync<PublicMenuResponse>(JsonOptions);
        menu.Should().NotBeNull();
        menu!.LayoutConfig.Should().NotBeNull();
        menu.LayoutConfig!.CanvasEnabled.Should().BeTrue();
        menu.LayoutConfig.BackgroundColor.Should().Be("#0F172A");
        menu.LayoutConfig.BackgroundUrl.Should().Be("https://cdn.restocore.com/canvas-bg.webp");

        // Assert orphan pruning: only active dish is in elements
        menu.LayoutConfig.Elements.Should().HaveCount(1);
        menu.LayoutConfig.Elements[0].DishId.Should().Be(activeDishId);
        menu.LayoutConfig.Elements[0].X.Should().Be(120.0);
        menu.LayoutConfig.Elements[0].Y.Should().Be(240.0);

        // 3. Conditional GET with matching If-None-Match yields 304 Not Modified
        var etag = response.Headers.ETag!.Tag;
        var conditionalRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tenants/{slug}/menu");
        conditionalRequest.Headers.TryAddWithoutValidation("If-None-Match", etag);

        var conditionalResponse = await _client.SendAsync(conditionalRequest);
        conditionalResponse.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }
}
