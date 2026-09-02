namespace InventoryManagement.Application.Stock;

public sealed record AddStockItemRequest(string ProductCode, int Quantity);
