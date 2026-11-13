namespace RestoCore.UnitTests.Infrastructure;

using System.Net;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Infrastructure.Authorization;
using RestoCore.Infrastructure.MultiTenancy;
using RestoCore.Infrastructure.Storage;
using Xunit;

public class GovernanceAndSecurityOptimizationTests
{
    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            if (request.Content != null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":true}", Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class ScopeCapturingLogger<T> : ILogger<T>
    {
        public List<object> Scopes { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            Scopes.Add(state);
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    [Theory]
    [InlineData(false, "http://")]
    [InlineData(true, "https://")]
    public async Task SeaweedStorageService_ShouldRespectUseSslAndGeneratePresignedUrlSynchronously(bool useSsl, string expectedScheme)
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:ServiceUrl"] = useSsl ? "https://s3.restocore.com" : "http://localhost:8333",
                ["Storage:PublicBaseUrl"] = useSsl ? "https://cdn.restocore.com" : "http://localhost:8333",
                ["Storage:BucketName"] = "restocore-images",
                ["Storage:AccessKey"] = "test-access",
                ["Storage:SecretKey"] = "test-secret",
                ["Storage:UseSsl"] = useSsl.ToString()
            })
            .Build();

        var service = new SeaweedStorageService(config);

        // Act
        var result = await service.GeneratePresignedUploadUrlAsync(
            Guid.NewGuid(),
            "plato.webp",
            "image/webp",
            "dishes");

        // Assert
        result.UploadUrl.Should().StartWith(expectedScheme);
        result.PublicUrl.Should().StartWith(expectedScheme);
    }

    [Fact]
    public async Task OpaClient_ShouldSerializePermissionsAndCacheDecisionFor30Seconds()
    {
        // Arrange
        var handler = new CapturingHttpMessageHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8181")
        };

        var config = new ConfigurationBuilder().Build();
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var opaClient = new OpaClient(httpClient, config, NullLogger<OpaClient>.Instance, memoryCache);

        var input = new OpaInput
        {
            Method = "PUT",
            Path = new[] { "api", "v1", "admin", "branding" },
            Action = "manage_branding",
            User = new OpaUser
            {
                Id = "user-123",
                TenantId = "tenant-abc",
                Roles = new[] { "owner" },
                Permissions = new[] { "manage_branding", "manage_menu" }
            },
            Resource = new OpaResource
            {
                Type = "tenant",
                TenantId = "tenant-abc"
            }
        };

        // Act
        var first = await opaClient.EvaluatePolicyAsync(input);
        var second = await opaClient.EvaluatePolicyAsync(input);

        // Assert
        first.Should().BeTrue();
        second.Should().BeTrue();
        handler.CallCount.Should().Be(1);
        handler.LastRequestBody.Should().NotBeNull();
        handler.LastRequestBody.Should().Contain("\"permissions\":[\"manage_branding\",\"manage_menu\"]");
    }

    [Fact]
    public async Task OpaAuthorizationHandler_ShouldPopulateRoleAndClaimPermissionsInOpaUser()
    {
        // Arrange
        OpaInput? capturedInput = null;
        var opaClientMock = new Mock<IOpaClient>();
        opaClientMock
            .Setup(c => c.EvaluatePolicyAsync(It.IsAny<OpaInput>(), It.IsAny<CancellationToken>()))
            .Callback<OpaInput, CancellationToken>((inp, _) => capturedInput = inp)
            .ReturnsAsync(true);

        var tenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "PUT";
        httpContext.Request.Path = "/api/v1/admin/branding";

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var tenantContextMock = new Mock<ITenantContext>();
        tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var handler = new OpaAuthorizationHandler(opaClientMock.Object, accessorMock.Object, tenantContextMock.Object);
        var requirement = new OpaRequirement("manage_branding");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Role, "owner"),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("permission", "custom_permission")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var authContext = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeTrue();
        capturedInput.Should().NotBeNull();
        capturedInput!.User.Permissions.Should().Contain("manage_branding");
        capturedInput.User.Permissions.Should().Contain("manage_menu");
        capturedInput.User.Permissions.Should().Contain("custom_permission");
    }

    [Fact]
    public async Task TenantResolutionMiddleware_ShouldOpenStructuredLoggerScopeWithTenantIdAndSlug()
    {
        // Arrange
        var logger = new ScopeCapturingLogger<TenantResolutionMiddleware>();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask, logger);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/tenants/bistro-gourmet/menu";
        var tenantContext = new TenantContext();

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        tenantContext.TenantSlug.Should().Be("bistro-gourmet");
        logger.Scopes.Should().ContainSingle();
        var scopeDict = logger.Scopes[0].Should().BeAssignableTo<IReadOnlyDictionary<string, object>>().Subject;
        scopeDict["tenant_slug"].Should().Be("bistro-gourmet");
    }
}
