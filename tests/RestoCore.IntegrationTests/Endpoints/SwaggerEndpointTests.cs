namespace RestoCore.IntegrationTests.Endpoints;

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class SwaggerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SwaggerEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSwaggerJson_InDevelopment_ReturnsOkAndOpenApiDocument()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        root.TryGetProperty("openapi", out var openapiProp).Should().BeTrue();
        root.GetProperty("info").GetProperty("title").GetString().Should().Be("RestoCore Backend API");
        root.TryGetProperty("paths", out var pathsProp).Should().BeTrue();
    }

    [Fact]
    public async Task GetSwaggerUi_InDevelopment_ReturnsHtmlPage()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/index.html");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSwaggerJson_InProduction_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
