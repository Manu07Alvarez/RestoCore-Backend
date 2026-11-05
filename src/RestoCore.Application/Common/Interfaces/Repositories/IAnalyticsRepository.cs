namespace RestoCore.Application.Common.Interfaces.Repositories;

using RestoCore.Application.Features.Analytics.DTOs;
using RestoCore.Domain.Entities;

public interface IAnalyticsRepository
{
    Task RecordEventAsync(AnalyticsEventMessage message, CancellationToken cancellationToken = default);
    Task<List<DailyMenuTrafficSummary>> GetDailyTrafficAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<int> GetTotalQrVisitsAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<List<DailyItemViewSummary>> GetItemViewsAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<List<DailyCategoryInteractionSummary>> GetCategoryInteractionsAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<List<HourlyMenuTrafficSummary>> GetHourlyTrafficAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<List<MenuItem>> GetAllTenantItemsWithDeletedAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
