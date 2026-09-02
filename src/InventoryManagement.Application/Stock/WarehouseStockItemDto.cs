namespace InventoryManagement.Application.Stock;

public sealed record WarehouseStockItemDto(string ProductCode, string ProductName, int Quantity, DateTime UpdatedAt);
