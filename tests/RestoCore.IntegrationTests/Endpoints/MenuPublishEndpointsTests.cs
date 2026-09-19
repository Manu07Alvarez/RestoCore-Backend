namespace RestoCore.IntegrationTests.Endpoints;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Features.MenuLayout.DTOs;
using RestoCore.Domain.Entities;
using RestoCore.Domain.ValueObjects;
using RestoCore.Infrastructure.Persistence;
using RestoCore.IntegrationTests.Fixtures;
using Xunit;

public class MenuPublishEndpointsTests : IClassFixture<ContainerizedStackFixture>
{
    private readonly HttpClient _client;
    private readonly ContainerizedStackFixture _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MenuPublishEndpointsTests(ContainerizedStackFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostPublish_WithAuthorizedAdmin_Returns202AcceptedAndPersistsJob()
    {
        var slug = $"pub-{Guid.NewGuid():N}"[..15];
        Guid tenantId;
        Guid dishId = Guid.NewGuid();

        // 1. Seed tenant with menu and layout
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenant = new Tenant
            {
                Name = "Publish Grill",
                Slug = slug,
                Status = "Active",
                LayoutConfig = new LayoutConfig
                {
                    CanvasEnabled = true,
                    BackgroundColor = "#000000",
                    Elements = new List<CanvasElement>
                    {
                        new()
                        {
                            DishId = dishId,
                            X = 100.0,
                            Y = 150.0,
                            ZIndex = 1,
                            Width = 200.0,
                            Height = 120.0
                        }
                    }
                }
            };
            db.Tenants.Add(tenant);

            var cat = new Category { TenantId = tenant.Id, Name = "Carnes", DisplayOrder = 1, IsActive = true };
            db.Categories.Add(cat);

            var item = new MenuItem
            {
                Id = dishId,
                TenantId = tenant.Id,
                CategoryId = cat.Id,
                Name = "Ojo de Bife",
                BasePrice = 18000m,
                IsAvailable = true,
                DisplayOrder = 1
            };
            db.MenuItems.Add(item);

            await db.SaveChangesAsync();
            tenantId = tenant.Id;
        }

        // 2. Invoke POST /api/v1/admin/menu/publish
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/menu/publish")
        {
            Content = JsonContent.Create(new MenuPublishRequest { ForceRecompile = true })
        };
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        request.Headers.Add("X-User-Role", "owner");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<MenuPublishJobResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.JobId.Should().NotBeEmpty();
        result.TenantSlug.Should().Be(slug);
        result.Status.Should().Be("completed");
        result.VersionHash.Should().MatchRegex(@"^v\d{14}-[a-f0-9]{8}$");
        result.CdnPurgeRequested.Should().BeTrue();

        // 3. Verify tenant CurrentVersionHash in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedTenant = await db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId);
            savedTenant.Should().NotBeNull();
            savedTenant!.CurrentVersionHash.Should().Be(result.VersionHash);

            var savedJob = await db.MenuPublishJobs.IgnoreQueryFilters().FirstOrDefaultAsync(j => j.Id == result.JobId);
            savedJob.Should().NotBeNull();
            savedJob!.TenantId.Should().Be(tenantId);
            savedJob.Status.Should().Be("completed");
            savedJob.VersionHash.Should().Be(result.VersionHash);
        }
    }

    [Fact]
    public async Task PostPublish_WithUnauthorizedRole_Returns403Forbidden()
    {
        var tenantId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/menu/publish")
        {
            Content = JsonContent.Create(new MenuPublishRequest())
        };
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        request.Headers.Add("X-User-Role", "waiter");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
