using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(
        string productCode,
        string sourceWarehouseCode,
        string destinationWarehouseCode,
        int quantity,
        CancellationToken cancellationToken = default);
}
