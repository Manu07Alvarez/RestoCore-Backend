namespace RestoCore.Application.Features.Analytics.Commands;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Analytics.DTOs;

public record CollectAnalyticsEventCommand(
    string EventType,
    string TenantSlug,
    Guid? ItemId = null,
    Guid? CategoryId = null,
    string? DeviceType = null
) : IRequest<bool>;

public class CollectAnalyticsEventCommandHandler : IRequestHandler<CollectAnalyticsEventCommand, bool>
{
    private static readonly HashSet<string> ValidEventTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "menu_view",
        "item_view",
        "category_click"
    };

    private static readonly HashSet<string> ValidDeviceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "mobile",
        "desktop",
        "tablet"
    };

    private readonly ITenantRepository _tenantRepository;
    private readonly IAnalyticsEventQueue _eventQueue;

    public CollectAnalyticsEventCommandHandler(
        ITenantRepository tenantRepository,
        IAnalyticsEventQueue eventQueue)
    {
        _tenantRepository = tenantRepository;
        _eventQueue = eventQueue;
    }

    public async Task<bool> Handle(CollectAnalyticsEventCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TenantSlug))
        {
            throw new ArgumentException("tenant_slug is required.");
        }

        if (string.IsNullOrWhiteSpace(request.EventType) || !ValidEventTypes.Contains(request.EventType.Trim()))
        {
            throw new ArgumentException("event_type must be one of: menu_view, item_view, category_click.");
        }

        var normalizedEventType = request.EventType.Trim().ToLowerInvariant();

        if (normalizedEventType == "item_view" && (!request.ItemId.HasValue || request.ItemId.Value == Guid.Empty))
        {
            throw new ArgumentException("item_id is required when event_type is item_view.");
        }

        if (normalizedEventType == "category_click" && (!request.CategoryId.HasValue || request.CategoryId.Value == Guid.Empty))
        {
            throw new ArgumentException("category_id is required when event_type is category_click.");
        }

        var normalizedDeviceType = "mobile";
        if (!string.IsNullOrWhiteSpace(request.DeviceType))
        {
            var trimmedDevice = request.DeviceType.Trim().ToLowerInvariant();
            if (!ValidDeviceTypes.Contains(trimmedDevice))
            {
                throw new ArgumentException("device_type must be one of: mobile, desktop, tablet.");
            }
            normalizedDeviceType = trimmedDevice;
        }

        var tenant = await _tenantRepository.GetTenantBySlugAsync(request.TenantSlug.Trim(), cancellationToken);
        if (tenant == null || !string.Equals(tenant.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new KeyNotFoundException($"Active tenant '{request.TenantSlug}' was not found.");
        }

        var message = new AnalyticsEventMessage(
            tenant.Id,
            normalizedEventType,
            request.ItemId,
            request.CategoryId,
            normalizedDeviceType,
            DateTimeOffset.UtcNow
        );

        await _eventQueue.EnqueueAsync(message, cancellationToken);
        return true;
    }
}
