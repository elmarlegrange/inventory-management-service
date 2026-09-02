namespace InventoryManagement.Application.Warehouses;

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseDto>> GetAllWarehousesAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDto> GetWarehouseByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default);
}
