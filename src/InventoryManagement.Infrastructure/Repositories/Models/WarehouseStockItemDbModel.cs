namespace InventoryManagement.Infrastructure.Repositories.Models;

public sealed class WarehouseStockItemDbModel
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
