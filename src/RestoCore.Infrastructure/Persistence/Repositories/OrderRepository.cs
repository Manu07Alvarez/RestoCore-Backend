namespace RestoCore.Infrastructure.Persistence.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public async Task<List<Order>> GetActiveOrdersByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.TenantId == tenantId && o.Status != "delivered" && o.Status != "cancelled")
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public void AddOrder(Order order)
    {
        _context.Orders.Add(order);
    }

    public void UpdateOrderStatus(Order order, string status)
    {
        order.Status = status;
        if (status == "delivered" || status == "cancelled")
        {
            order.CompletedAt = DateTimeOffset.UtcNow;
        }
    }
}
