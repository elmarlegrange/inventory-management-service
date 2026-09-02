using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Interfaces;

public interface IStockRepository
{
    Task<Stock?> GetStockAsync(string warehouseCode, string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Stock>> GetStockByWarehouseAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Stock>> GetStockBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task UpsertStockAsync(string warehouseCode, string sku, int quantity, CancellationToken cancellationToken = default);
}
