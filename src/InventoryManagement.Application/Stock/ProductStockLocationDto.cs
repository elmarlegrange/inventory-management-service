namespace InventoryManagement.Application.Stock;

public sealed record ProductStockLocationDto(string WarehouseCode, string WarehouseName, int Quantity, DateTime UpdatedAt);
