using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Interfaces;

public interface IStockRepository
{
    Task<Stock?> GetStockAsync(string warehouseCode, string productCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Stock>> GetStockByWarehouseCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Stock>> GetStockByProductCodeAsync(string productCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseStockItem>> GetWarehouseStockDetailsAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductStockLocation>> GetProductStockDetailsAsync(string productCode, CancellationToken cancellationToken = default);
    Task UpsertStockAsync(string warehouseCode, string productCode, int quantity, CancellationToken cancellationToken = default);
}
