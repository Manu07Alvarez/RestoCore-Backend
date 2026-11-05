namespace RestoCore.Application.Features.Analytics.DTOs;

using System.Text.Json.Serialization;

public record AnalyticsEventRequest(
    [property: JsonPropertyName("event_type")] string EventType,
    [property: JsonPropertyName("tenant_slug")] string TenantSlug,
    [property: JsonPropertyName("item_id")] Guid? ItemId = null,
    [property: JsonPropertyName("category_id")] Guid? CategoryId = null,
    [property: JsonPropertyName("device_type")] string? DeviceType = null
);

public record AnalyticsEventMessage(
    Guid TenantId,
    string EventType,
    Guid? ItemId,
    Guid? CategoryId,
    string DeviceType,
    DateTimeOffset OccurredAtUtc
);

public record AnalyticsDashboardResponse(
    [property: JsonPropertyName("tenant_id")] Guid TenantId,
    [property: JsonPropertyName("period_from")] string PeriodFrom,
    [property: JsonPropertyName("period_to")] string PeriodTo,
    [property: JsonPropertyName("priority_high_metrics")] PriorityHighMetricsDto PriorityHighMetrics,
    [property: JsonPropertyName("priority_medium_metrics")] PriorityMediumMetricsDto? PriorityMediumMetrics
);

public record PriorityHighMetricsDto(
    [property: JsonPropertyName("total_qr_visits")] int TotalQrVisits,
    [property: JsonPropertyName("monthly_qr_visits")] int MonthlyQrVisits,
    [property: JsonPropertyName("top_viewed_items")] List<TopViewedItemDto> TopViewedItems,
    [property: JsonPropertyName("top_categories")] List<TopCategoryDto> TopCategories,
    [property: JsonPropertyName("menu_engineering_matrix")] MenuEngineeringMatrixDto MenuEngineeringMatrix
);

public record TopViewedItemDto(
    [property: JsonPropertyName("item_id")] Guid ItemId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("views_count")] int ViewsCount
);

public record TopCategoryDto(
    [property: JsonPropertyName("category_id")] Guid CategoryId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("interactions_count")] int InteractionsCount
);

public record MenuEngineeringMatrixDto(
    [property: JsonPropertyName("stars")] List<Guid> Stars,
    [property: JsonPropertyName("plowhorses")] List<Guid> Plowhorses,
    [property: JsonPropertyName("puzzles")] List<Guid> Puzzles,
    [property: JsonPropertyName("dogs")] List<Guid> Dogs
);

public record PriorityMediumMetricsDto(
    [property: JsonPropertyName("peak_hours")] List<PeakHourDto> PeakHours,
    [property: JsonPropertyName("device_breakdown")] DeviceBreakdownDto DeviceBreakdown
);

public record PeakHourDto(
    [property: JsonPropertyName("hour")] int Hour,
    [property: JsonPropertyName("visits_count")] int VisitsCount
);

public record DeviceBreakdownDto(
    [property: JsonPropertyName("mobile_percentage")] decimal MobilePercentage,
    [property: JsonPropertyName("desktop_percentage")] decimal DesktopPercentage
);
