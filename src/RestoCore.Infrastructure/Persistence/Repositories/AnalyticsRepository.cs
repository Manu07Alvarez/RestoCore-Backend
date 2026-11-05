namespace RestoCore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Analytics.DTOs;
using RestoCore.Domain.Entities;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public AnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RecordEventAsync(AnalyticsEventMessage message, CancellationToken cancellationToken = default)
    {
        var dateUtc = DateOnly.FromDateTime(message.OccurredAtUtc.UtcDateTime);
        var hourOfDay = message.OccurredAtUtc.UtcDateTime.Hour;
        var deviceType = (message.DeviceType ?? "mobile").ToLowerInvariant();

        if (message.EventType == "menu_view")
        {
            var dailyTraffic = await _context.DailyMenuTrafficSummaries
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TenantId == message.TenantId && x.DateUtc == dateUtc, cancellationToken);

            if (dailyTraffic == null)
            {
                dailyTraffic = new DailyMenuTrafficSummary
                {
                    TenantId = message.TenantId,
                    DateUtc = dateUtc,
                    QrVisitsCount = 1,
                    MobileCount = deviceType == "mobile" ? 1 : 0,
                    DesktopCount = deviceType == "desktop" ? 1 : 0,
                    TabletCount = deviceType == "tablet" ? 1 : 0
                };
                _context.DailyMenuTrafficSummaries.Add(dailyTraffic);
            }
            else
            {
                dailyTraffic.QrVisitsCount += 1;
                if (deviceType == "mobile") dailyTraffic.MobileCount += 1;
                else if (deviceType == "desktop") dailyTraffic.DesktopCount += 1;
                else if (deviceType == "tablet") dailyTraffic.TabletCount += 1;
            }

            var hourlyTraffic = await _context.HourlyMenuTrafficSummaries
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.TenantId == message.TenantId && x.DateUtc == dateUtc && x.HourOfDay == hourOfDay,
                    cancellationToken);

            if (hourlyTraffic == null)
            {
                hourlyTraffic = new HourlyMenuTrafficSummary
                {
                    TenantId = message.TenantId,
                    DateUtc = dateUtc,
                    HourOfDay = hourOfDay,
                    VisitsCount = 1
                };
                _context.HourlyMenuTrafficSummaries.Add(hourlyTraffic);
            }
            else
            {
                hourlyTraffic.VisitsCount += 1;
            }
        }
        else if (message.EventType == "item_view" && message.ItemId.HasValue)
        {
            var itemExists = await _context.MenuItems
                .IgnoreQueryFilters()
                .AnyAsync(m => m.Id == message.ItemId.Value && m.TenantId == message.TenantId, cancellationToken);

            if (itemExists)
            {
                var itemView = await _context.DailyItemViewSummaries
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.TenantId == message.TenantId && x.ItemId == message.ItemId.Value && x.DateUtc == dateUtc,
                        cancellationToken);

                if (itemView == null)
                {
                    itemView = new DailyItemViewSummary
                    {
                        TenantId = message.TenantId,
                        ItemId = message.ItemId.Value,
                        DateUtc = dateUtc,
                        ViewsCount = 1
                    };
                    _context.DailyItemViewSummaries.Add(itemView);
                }
                else
                {
                    itemView.ViewsCount += 1;
                }
            }
        }
        else if (message.EventType == "category_click" && message.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == message.CategoryId.Value && c.TenantId == message.TenantId, cancellationToken);

            if (categoryExists)
            {
                var categoryClick = await _context.DailyCategoryInteractionSummaries
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.TenantId == message.TenantId && x.CategoryId == message.CategoryId.Value && x.DateUtc == dateUtc,
                        cancellationToken);

                if (categoryClick == null)
                {
                    categoryClick = new DailyCategoryInteractionSummary
                    {
                        TenantId = message.TenantId,
                        CategoryId = message.CategoryId.Value,
                        DateUtc = dateUtc,
                        InteractionsCount = 1
                    };
                    _context.DailyCategoryInteractionSummaries.Add(categoryClick);
                }
                else
                {
                    categoryClick.InteractionsCount += 1;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<DailyMenuTrafficSummary>> GetDailyTrafficAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.DailyMenuTrafficSummaries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DateUtc >= fromDate && x.DateUtc <= toDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalQrVisitsAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.DailyMenuTrafficSummaries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DateUtc >= fromDate && x.DateUtc <= toDate)
            .SumAsync(x => x.QrVisitsCount, cancellationToken);
    }

    public async Task<List<DailyItemViewSummary>> GetItemViewsAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.DailyItemViewSummaries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DateUtc >= fromDate && x.DateUtc <= toDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DailyCategoryInteractionSummary>> GetCategoryInteractionsAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.DailyCategoryInteractionSummaries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DateUtc >= fromDate && x.DateUtc <= toDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<HourlyMenuTrafficSummary>> GetHourlyTrafficAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.HourlyMenuTrafficSummaries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DateUtc >= fromDate && x.DateUtc <= toDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MenuItem>> GetAllTenantItemsWithDeletedAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}
