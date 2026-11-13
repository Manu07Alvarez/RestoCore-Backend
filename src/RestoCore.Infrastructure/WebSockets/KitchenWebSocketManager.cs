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
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, WebSocket>> _socketsByTenant = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<KitchenWebSocketManager> _logger;

    public KitchenWebSocketManager(ILogger<KitchenWebSocketManager> logger)
    {
        _logger = logger;
    }

    public Guid RegisterSocket(string tenantSlug, WebSocket socket)
    {
        var connectionId = Guid.NewGuid();
        var sockets = _socketsByTenant.GetOrAdd(tenantSlug, static _ => new ConcurrentDictionary<Guid, WebSocket>());
        sockets.TryAdd(connectionId, socket);

        _logger.LogInformation(
            "Kitchen WebSocket {ConnectionId} registered for tenant {TenantSlug}. Total connections: {Count}",
            connectionId,
            tenantSlug,
            sockets.Count);

        return connectionId;
    }

    public void RemoveAndDisposeSocket(string tenantSlug, Guid connectionId)
    {
        if (_socketsByTenant.TryGetValue(tenantSlug, out var sockets) &&
            sockets.TryRemove(connectionId, out var socket))
        {
            try
            {
                socket.Dispose();
            }
            catch
            {
                // Ignore transport disposal errors on already aborted sockets
            }
        }
    }

    public int GetActiveConnectionCount(string tenantSlug)
        => _socketsByTenant.TryGetValue(tenantSlug, out var sockets) ? sockets.Count : 0;

    public async Task BroadcastOrderCreatedAsync(string tenantSlug, OrderCreatedNotification notification, CancellationToken ct = default)
    {
        if (!_socketsByTenant.TryGetValue(tenantSlug, out var sockets) || sockets.IsEmpty)
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

        foreach (var (connectionId, socket) in sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send WebSocket message to connection {ConnectionId} in tenant {TenantSlug}", connectionId, tenantSlug);
                    RemoveAndDisposeSocket(tenantSlug, connectionId);
                }
            }
            else
            {
                RemoveAndDisposeSocket(tenantSlug, connectionId);
            }
        }
    }
}
