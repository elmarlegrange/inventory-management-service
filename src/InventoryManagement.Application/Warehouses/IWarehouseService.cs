using InventoryManagement.Application.Stock;

namespace InventoryManagement.Application.Warehouses;

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseDto>> GetAllWarehousesAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDto> GetWarehouseByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseStockItemDto>> GetStockForWarehouseAsync(string code, CancellationToken cancellationToken = default);
    Task AddStockToWarehouseAsync(string code, AddStockItemRequest request, CancellationToken cancellationToken = default);
}
