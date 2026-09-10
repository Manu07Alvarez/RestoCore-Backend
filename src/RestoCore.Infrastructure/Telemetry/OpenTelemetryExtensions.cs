namespace RestoCore.Infrastructure.Telemetry;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class OpenTelemetryExtensions
{
    public const string ServiceName = "RestoCore.Backend";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);

    public static IServiceCollection AddRestoCoreTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: ServiceName, serviceVersion: "1.0.0"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ServiceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddNpgsql();

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(ServiceName)
                    .AddAspNetCoreInstrumentation();

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            })
            .WithLogging(logging =>
            {
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    logging.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return services;
    }
}
