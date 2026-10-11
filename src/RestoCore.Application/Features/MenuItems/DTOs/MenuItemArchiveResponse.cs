namespace RestoCore.Application.Features.MenuItems.DTOs;

public record MenuItemArchiveResponse(
    Guid ItemId,
    string Status,
    string Message,
    bool HasOpenOrders,
    DateTimeOffset ArchivedAt,
    int RetentionDays,
    DateTimeOffset PurgeScheduledAt
);
