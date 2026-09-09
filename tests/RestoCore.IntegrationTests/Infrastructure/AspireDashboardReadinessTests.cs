namespace RestoCore.IntegrationTests.Infrastructure;

using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using Xunit;

public class AspireDashboardReadinessTests
{
    [Fact]
    public async Task AspireDashboard_WebUiPort18888_IsReachable()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync("http://localhost:18888");

        // Assert - Aspire Dashboard redirects to /structuredlogs or responds 200/302
        ((int)response.StatusCode).Should().BeInRange(200, 399);
    }

    [Fact]
    public async Task AspireDashboard_OtlpGrpcPort4317_AcceptsTcpConnections()
    {
        // Arrange
        using var tcpClient = new TcpClient();

        // Act
        var connectTask = tcpClient.ConnectAsync("localhost", 4317);
        var timeoutTask = Task.Delay( TimeSpan.FromSeconds(5) );

        var completedTask = await Task.WhenAny(connectTask, timeoutTask);

        // Assert
        completedTask.Should().Be(connectTask, "TCP port 4317 (OTLP gRPC) should accept connections within 5 seconds");
        tcpClient.Connected.Should().BeTrue();
    }
}
