namespace InventoryManagement.Infrastructure.Repositories.Models;

public sealed class StockDbModel
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
