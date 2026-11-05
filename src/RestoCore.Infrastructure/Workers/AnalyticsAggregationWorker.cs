namespace RestoCore.Infrastructure.Workers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;

public class AnalyticsAggregationWorker : BackgroundService
{
    private readonly IAnalyticsEventQueue _eventQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AnalyticsAggregationWorker> _logger;

    public AnalyticsAggregationWorker(
        IAnalyticsEventQueue eventQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<AnalyticsAggregationWorker> logger)
    {
        _eventQueue = eventQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AnalyticsAggregationWorker started.");

        await foreach (var message in _eventQueue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();
                await repository.RecordEventAsync(message, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating analytics event {EventType} for TenantId {TenantId}", message.EventType, message.TenantId);
            }
        }
    }
}
