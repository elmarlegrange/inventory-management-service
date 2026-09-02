namespace InventoryManagement.Infrastructure.Repositories.Models;

public sealed class ProductStockLocationDbModel
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
