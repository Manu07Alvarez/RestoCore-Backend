namespace RestoCore.UnitTests.MultiTenancy;

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using RestoCore.Infrastructure.MultiTenancy;
using Xunit;

public class TenantResolutionTests
{
    [Fact]
    public async Task Should_Resolve_Tenant_From_Path_Slug()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/tenants/bistro-paris/menu";
        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        tenantContext.TenantSlug.Should().Be("bistro-paris");
    }

    [Fact]
    public async Task Should_Resolve_Tenant_From_Short_Qr_Route()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/r/la-trattoria";
        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        tenantContext.TenantSlug.Should().Be("la-trattoria");
    }

    [Fact]
    public async Task Should_Resolve_Tenant_From_Subdomain()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("pizzeria-napoli.restocore.app");
        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        tenantContext.TenantSlug.Should().Be("pizzeria-napoli");
    }

    [Fact]
    public async Task Should_Resolve_Tenant_From_Jwt_Claims()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim("tenant_id", expectedId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);

        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        tenantContext.TenantId.Should().Be(expectedId);
    }
}
