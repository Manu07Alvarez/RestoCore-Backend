namespace RestoCore.IntegrationTests.Infrastructure;

using System.Net.Sockets;
using FluentAssertions;
using Npgsql;
using StackExchange.Redis;
using Xunit;

public class ContainerReadinessTests
{
    [Fact]
    public async Task Postgres_Port5432_IsAcceptingConnectionsAndQueries()
    {
        var connectionString = "Host=localhost;Port=5432;Database=restocore_dev;Username=postgres;Password=postgres_dev_password";
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT count(*) FROM tenants;", conn);
        var result = await cmd.ExecuteScalarAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Redis_Port6379_RespondsToPing()
    {
        var multiplexer = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var ping = await multiplexer.GetDatabase().PingAsync();

        ping.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task SeaweedFs_Port8333_S3GatewayIsListening()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("http://localhost:8333/");

        // S3 gateway responds with XML or status code
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Opa_Port8181_RespondsWithPolicyData()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("http://localhost:8181/v1/data");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
