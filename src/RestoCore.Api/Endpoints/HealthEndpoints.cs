namespace RestoCore.Api.Endpoints;

using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;
using StackExchange.Redis;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/healthz", () => Results.Ok(new
        {
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow
        })).AllowAnonymous().WithTags("Health");

        app.MapGet("/ready", async (
            IApplicationDbContext context,
            IStorageService storageService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory) =>
        {
            var checks = new Dictionary<string, string>();
            var isAllHealthy = true;

            // 1. PostgreSQL check
            try
            {
                var canConnectDb = await context.Tenants.AnyAsync();
                checks["postgresql"] = "Connected";
            }
            catch (Exception ex)
            {
                isAllHealthy = false;
                checks["postgresql"] = $"Error: {ex.Message}";
            }

            // 2. Redis check
            try
            {
                var redisConn = configuration["Redis:Configuration"] ?? "localhost:6379";
                var multiplexer = await ConnectionMultiplexer.ConnectAsync(redisConn);
                var ping = await multiplexer.GetDatabase().PingAsync();
                checks["redis"] = $"Connected ({ping.TotalMilliseconds:F1}ms)";
            }
            catch (Exception ex)
            {
                isAllHealthy = false;
                checks["redis"] = $"Error: {ex.Message}";
            }

            // 3. SeaweedFS check
            try
            {
                var bucket = configuration["Storage:BucketName"] ?? "restocore-images";
                var bucketExists = await storageService.BucketExistsAsync(bucket);
                checks["seaweedfs"] = bucketExists ? "Connected (Bucket exists)" : "Connected (Bucket pending)";
            }
            catch (Exception ex)
            {
                isAllHealthy = false;
                checks["seaweedfs"] = $"Error: {ex.Message}";
            }

            // 4. OPA check
            try
            {
                var opaUrl = configuration["Opa:BaseUrl"] ?? "http://localhost:8181";
                var client = httpClientFactory.CreateClient();
                var resp = await client.GetAsync($"{opaUrl}/v1/data");
                checks["opa"] = resp.IsSuccessStatusCode ? "Connected" : $"Status: {resp.StatusCode}";
                if (!resp.IsSuccessStatusCode) isAllHealthy = false;
            }
            catch (Exception ex)
            {
                isAllHealthy = false;
                checks["opa"] = $"Error: {ex.Message}";
            }

            var resultPayload = new
            {
                status = isAllHealthy ? "Ready" : "Degraded",
                dependencies = checks,
                timestamp = DateTimeOffset.UtcNow
            };

            return isAllHealthy
                ? Results.Ok(resultPayload)
                : Results.Json(resultPayload, statusCode: StatusCodes.Status503ServiceUnavailable);
        }).AllowAnonymous().WithTags("Health");

        return app;
    }
}
