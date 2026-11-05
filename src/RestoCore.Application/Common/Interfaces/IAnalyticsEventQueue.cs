namespace RestoCore.Application.Common.Interfaces;

using RestoCore.Application.Features.Analytics.DTOs;

public interface IAnalyticsEventQueue
{
    ValueTask EnqueueAsync(AnalyticsEventMessage message, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AnalyticsEventMessage> ReadAllAsync(CancellationToken cancellationToken = default);
}
