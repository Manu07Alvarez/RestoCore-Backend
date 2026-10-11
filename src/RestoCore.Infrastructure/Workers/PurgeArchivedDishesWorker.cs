namespace RestoCore.Infrastructure.Workers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;

public class PurgeArchivedDishesWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PurgeArchivedDishesWorker> _logger;
    private readonly TimeSpan _checkInterval;

    public PurgeArchivedDishesWorker(
        IServiceProvider serviceProvider,
        ILogger<PurgeArchivedDishesWorker> logger,
        TimeSpan? checkInterval = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _checkInterval = checkInterval ?? TimeSpan.FromHours(24);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PurgeArchivedDishesWorker started with interval {Interval}.", _checkInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PurgeExpiredDishesAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error occurred during scheduled purge of archived dishes.");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public async Task<int> PurgeExpiredDishesAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var catalogRepository = scope.ServiceProvider.GetRequiredService<ICatalogRepository>();
        var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var threshold = DateTimeOffset.UtcNow.AddDays(-7);
        var expiredDishes = await catalogRepository.GetArchivedMenuItemsOlderThanAsync(threshold, cancellationToken);

        if (expiredDishes.Count == 0)
        {
            return 0;
        }

        _logger.LogInformation("Found {Count} archived dishes eligible for permanent purge.", expiredDishes.Count);

        foreach (var dish in expiredDishes)
        {
            if (!string.IsNullOrWhiteSpace(dish.ImageUrl))
            {
                try
                {
                    await storageService.DeleteObjectAsync(dish.ImageUrl, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete storage image {ImageUrl} for dish {DishId}.", dish.ImageUrl, dish.Id);
                }
            }

            catalogRepository.RemoveMenuItem(dish);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        _logger.LogInformation("Successfully purged {Count} archived dishes.", expiredDishes.Count);
        return expiredDishes.Count;
    }
}
