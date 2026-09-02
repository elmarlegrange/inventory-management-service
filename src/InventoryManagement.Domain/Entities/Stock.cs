namespace InventoryManagement.Domain.Entities;

public sealed class Stock
{
    public string WarehouseCode { get; }
    public string Sku { get; }
    public int Quantity { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Stock(string warehouseCode, string sku, int quantity, DateTime? updatedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(warehouseCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        WarehouseCode = warehouseCode.Trim().ToUpperInvariant();
        Sku = sku.Trim().ToUpperInvariant();
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
