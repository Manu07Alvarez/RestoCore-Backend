namespace RestoCore.Application.Features.Analytics.Queries;

using System.Globalization;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Analytics.DTOs;

public record GetAnalyticsDashboardQuery(
    string? From = null,
    string? To = null
) : IRequest<AnalyticsDashboardResponse>;

public class GetAnalyticsDashboardQueryHandler : IRequestHandler<GetAnalyticsDashboardQuery, AnalyticsDashboardResponse>
{
    private const decimal DefaultCostRatio = 0.30m;

    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ICatalogRepository _catalogRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantContext _tenantContext;

    public GetAnalyticsDashboardQueryHandler(
        IAnalyticsRepository analyticsRepository,
        ICatalogRepository catalogRepository,
        ITenantRepository tenantRepository,
        ITenantContext tenantContext)
    {
        _analyticsRepository = analyticsRepository;
        _catalogRepository = catalogRepository;
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
    }

    public async Task<AnalyticsDashboardResponse> Handle(GetAnalyticsDashboardQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodTo = ParseDateOrDefault(request.To, todayUtc, nameof(request.To));
        var periodFrom = ParseDateOrDefault(request.From, periodTo.AddDays(-30), nameof(request.From));

        if (periodFrom > periodTo)
        {
            throw new ArgumentException("Query parameter 'from' cannot be later than 'to'.");
        }

        var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {tenantId} was not found.");

        // 1. Daily QR visits in requested period & trailing 30 days
        var dailyTraffic = await _analyticsRepository.GetDailyTrafficAsync(tenantId, periodFrom, periodTo, cancellationToken);
        var totalQrVisits = dailyTraffic.Sum(x => x.QrVisitsCount);

        var trailing30Start = todayUtc.AddDays(-30);
        var monthlyQrVisits = await _analyticsRepository.GetTotalQrVisitsAsync(tenantId, trailing30Start, todayUtc, cancellationToken);

        // 2. Item views & Top Viewed Items (includes historical soft-deleted dishes)
        var itemViewSummaries = await _analyticsRepository.GetItemViewsAsync(tenantId, periodFrom, periodTo, cancellationToken);
        var itemViewsByItemId = itemViewSummaries
            .GroupBy(x => x.ItemId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.ViewsCount));

        var allTenantItems = await _analyticsRepository.GetAllTenantItemsWithDeletedAsync(tenantId, cancellationToken);
        var itemsById = allTenantItems.ToDictionary(m => m.Id);

        var topViewedItems = itemViewsByItemId
            .Where(kvp => kvp.Value > 0 && itemsById.ContainsKey(kvp.Key))
            .Select(kvp => new TopViewedItemDto(
                kvp.Key,
                itemsById[kvp.Key].Name,
                kvp.Value))
            .OrderByDescending(x => x.ViewsCount)
            .ThenBy(x => x.Name)
            .Take(10)
            .ToList();

        // 3. Category interactions & Top Categories
        var categorySummaries = await _analyticsRepository.GetCategoryInteractionsAsync(tenantId, periodFrom, periodTo, cancellationToken);
        var categoryClicksByCategoryId = categorySummaries
            .GroupBy(x => x.CategoryId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.InteractionsCount));

        var categories = await _catalogRepository.GetCategoriesByTenantAsync(tenantId, cancellationToken);
        var categoriesById = categories.ToDictionary(c => c.Id);

        var topCategories = categoryClicksByCategoryId
            .Where(kvp => kvp.Value > 0 && categoriesById.ContainsKey(kvp.Key))
            .Select(kvp => new TopCategoryDto(
                kvp.Key,
                categoriesById[kvp.Key].Name,
                kvp.Value))
            .OrderByDescending(x => x.InteractionsCount)
            .ThenBy(x => x.Name)
            .Take(10)
            .ToList();

        // 4. Menu Engineering Matrix (FR-007: only active, non-deleted dishes)
        var activeItems = allTenantItems.Where(m => !m.IsDeleted).ToList();
        var matrix = ComputeMenuEngineeringMatrix(activeItems, itemViewsByItemId);

        var highMetrics = new PriorityHighMetricsDto(
            totalQrVisits,
            monthlyQrVisits,
            topViewedItems,
            topCategories,
            matrix
        );

        // 5. Priority Medium Metrics (FR-008: Pro/Enterprise tier only; ADR-0012: no table turnover/dwell heuristics)
        PriorityMediumMetricsDto? mediumMetrics = null;
        if (string.Equals(tenant.PlanTier, "Pro", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tenant.PlanTier, "Enterprise", StringComparison.OrdinalIgnoreCase))
        {
            var hourlySummaries = await _analyticsRepository.GetHourlyTrafficAsync(tenantId, periodFrom, periodTo, cancellationToken);
            var visitsByHour = hourlySummaries
                .GroupBy(x => x.HourOfDay)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.VisitsCount));

            var peakHours = Enumerable.Range(0, 24)
                .Select(hour => new PeakHourDto(hour, visitsByHour.GetValueOrDefault(hour, 0)))
                .ToList();

            var mobileAndTablet = dailyTraffic.Sum(x => x.MobileCount + x.TabletCount);
            var desktop = dailyTraffic.Sum(x => x.DesktopCount);
            var totalDeviceEvents = mobileAndTablet + desktop;

            decimal mobilePct = 0m;
            decimal desktopPct = 0m;
            if (totalDeviceEvents > 0)
            {
                mobilePct = Math.Round((decimal)mobileAndTablet * 100m / totalDeviceEvents, 2);
                desktopPct = Math.Round(100m - mobilePct, 2);
            }

            mediumMetrics = new PriorityMediumMetricsDto(
                peakHours,
                new DeviceBreakdownDto(mobilePct, desktopPct)
            );
        }

        return new AnalyticsDashboardResponse(
            tenantId,
            periodFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            periodTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            highMetrics,
            mediumMetrics
        );
    }

    private static MenuEngineeringMatrixDto ComputeMenuEngineeringMatrix(
        List<Domain.Entities.MenuItem> activeItems,
        Dictionary<Guid, int> itemViewsByItemId)
    {
        var stars = new List<Guid>();
        var plowhorses = new List<Guid>();
        var puzzles = new List<Guid>();
        var dogs = new List<Guid>();

        if (activeItems.Count == 0)
        {
            return new MenuEngineeringMatrixDto(stars, plowhorses, puzzles, dogs);
        }

        var itemStats = activeItems.Select(item =>
        {
            var views = itemViewsByItemId.GetValueOrDefault(item.Id, 0);
            var effectiveCost = item.EstimatedCost ?? (item.BasePrice * DefaultCostRatio);
            var margin = item.BasePrice - effectiveCost;
            return (item.Id, Views: views, Margin: margin);
        }).ToList();

        var totalActiveViews = itemStats.Sum(x => x.Views);
        if (totalActiveViews == 0)
        {
            return new MenuEngineeringMatrixDto(stars, plowhorses, puzzles, dogs);
        }

        var averageViews = (decimal)totalActiveViews / itemStats.Count;
        var averageMargin = itemStats.Average(x => x.Margin);

        foreach (var stat in itemStats)
        {
            var highPopularity = stat.Views >= averageViews;
            var highMargin = stat.Margin >= averageMargin;

            if (highPopularity && highMargin)
            {
                stars.Add(stat.Id);
            }
            else if (highPopularity && !highMargin)
            {
                plowhorses.Add(stat.Id);
            }
            else if (!highPopularity && highMargin)
            {
                puzzles.Add(stat.Id);
            }
            else
            {
                dogs.Add(stat.Id);
            }
        }

        return new MenuEngineeringMatrixDto(stars, plowhorses, puzzles, dogs);
    }

    private static DateOnly ParseDateOrDefault(string? rawDate, DateOnly defaultValue, string paramName)
    {
        if (string.IsNullOrWhiteSpace(rawDate))
        {
            return defaultValue;
        }

        if (DateOnly.TryParseExact(rawDate.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Invalid ISO-8601 date format for '{paramName}'. Expected yyyy-MM-dd.");
    }
}
