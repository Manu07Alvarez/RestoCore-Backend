namespace RestoCore.Infrastructure.WebSockets;

using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;

public class KitchenWebSocketManager : IKitchenNotifier
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<WebSocket>> _socketsByTenant = new();
    private readonly ILogger<KitchenWebSocketManager> _logger;

    public KitchenWebSocketManager(ILogger<KitchenWebSocketManager> logger)
    {
        _logger = logger;
    }

    public void RegisterSocket(string tenantSlug, WebSocket socket)
    {
        var sockets = _socketsByTenant.GetOrAdd(tenantSlug, _ => new ConcurrentBag<WebSocket>());
        sockets.Add(socket);
        _logger.LogInformation("Kitchen WebSocket registered for tenant {TenantSlug}. Total connections: {Count}",
            tenantSlug, sockets.Count);
    }

    public async Task BroadcastOrderCreatedAsync(string tenantSlug, OrderCreatedNotification notification, CancellationToken ct = default)
    {
        if (!_socketsByTenant.TryGetValue(tenantSlug, out var sockets))
        {
            return;
        }

        var payload = new
        {
            @event = "ORDER_CREATED",
            data = notification
        };

        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        var buffer = new ArraySegment<byte>(bytes);

        var deadSockets = new List<WebSocket>();

        foreach (var socket in sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send WebSocket message to client in tenant {TenantSlug}", tenantSlug);
                    deadSockets.Add(socket);
                }
            }
            else
            {
                deadSockets.Add(socket);
            }
        }
    }
}
