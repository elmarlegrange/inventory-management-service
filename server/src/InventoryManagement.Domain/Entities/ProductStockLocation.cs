namespace InventoryManagement.Domain.Entities;

public sealed record ProductStockLocation(string WarehouseCode, string WarehouseName, int Quantity, DateTime UpdatedAt);
