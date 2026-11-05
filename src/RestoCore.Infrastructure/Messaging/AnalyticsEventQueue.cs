namespace RestoCore.Infrastructure.Messaging;

using System.Threading.Channels;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Features.Analytics.DTOs;

public class AnalyticsEventQueue : IAnalyticsEventQueue
{
    private readonly Channel<AnalyticsEventMessage> _channel;

    public AnalyticsEventQueue()
    {
        var options = new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<AnalyticsEventMessage>(options);
    }

    public ValueTask EnqueueAsync(AnalyticsEventMessage message, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public IAsyncEnumerable<AnalyticsEventMessage> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
