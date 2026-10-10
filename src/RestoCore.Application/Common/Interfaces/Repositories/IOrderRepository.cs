namespace RestoCore.Application.Common.Interfaces.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestoCore.Domain.Entities;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    Task<List<Order>> GetActiveOrdersByTenantAsync(Guid tenantId, CancellationToken ct = default);
    void AddOrder(Order order);
    void UpdateOrderStatus(Order order, string status);
}
