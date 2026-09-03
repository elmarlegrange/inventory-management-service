namespace InventoryManagement.Domain.Entities;

public sealed class Stock
{
    public string WarehouseCode { get; }
    public string ProductCode { get; }
    public int Quantity { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Stock(string warehouseCode, string productCode, int quantity, DateTime? updatedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(warehouseCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(productCode);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        WarehouseCode = warehouseCode.Trim().ToUpperInvariant();
        ProductCode = productCode.Trim().ToUpperInvariant();
        Quantity = quantity;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }

    public void SetQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
