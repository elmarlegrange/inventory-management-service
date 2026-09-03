namespace InventoryManagement.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; }
    public string ProductCode { get; }
    public string SourceWarehouseCode { get; }
    public string DestinationWarehouseCode { get; }
    public int Quantity { get; }
    public DateTime CreatedAt { get; }

    public Order(
        Guid id,
        string productCode,
        string sourceWarehouseCode,
        string destinationWarehouseCode,
        int quantity,
        DateTime? createdAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceWarehouseCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationWarehouseCode);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (sourceWarehouseCode.Trim().Equals(destinationWarehouseCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Source and destination warehouses cannot be identical.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ProductCode = productCode.Trim().ToUpperInvariant();
        SourceWarehouseCode = sourceWarehouseCode.Trim().ToUpperInvariant();
        DestinationWarehouseCode = destinationWarehouseCode.Trim().ToUpperInvariant();
        Quantity = quantity;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }
}
