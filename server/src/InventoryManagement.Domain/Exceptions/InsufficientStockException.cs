namespace InventoryManagement.Domain.Exceptions;

public sealed class InsufficientStockException : DomainException
{
    public string ProductCode { get; }
    public string WarehouseCode { get; }
    public int RequiredQuantity { get; }
    public int AvailableQuantity { get; }
    public int MissingQuantity { get; }

    public InsufficientStockException(string productCode, string warehouseCode, int requiredQuantity, int availableQuantity)
        : base($"Insufficient stock for product '{productCode}' at warehouse '{warehouseCode}': required {requiredQuantity}, but only {availableQuantity} available (missing {requiredQuantity - availableQuantity}).")
    {
        ProductCode = productCode;
        WarehouseCode = warehouseCode;
        RequiredQuantity = requiredQuantity;
        AvailableQuantity = availableQuantity;
        MissingQuantity = Math.Max(0, requiredQuantity - availableQuantity);
    }
}
