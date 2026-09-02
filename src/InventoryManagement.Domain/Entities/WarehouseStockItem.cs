namespace InventoryManagement.Domain.Entities;

public sealed record WarehouseStockItem(string ProductCode, string ProductName, int Quantity, DateTime UpdatedAt);
