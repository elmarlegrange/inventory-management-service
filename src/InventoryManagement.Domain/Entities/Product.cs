namespace InventoryManagement.Domain.Entities;

public sealed class Product
{
    public string Sku { get; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; }

    public Product(string sku, string name, DateTime? createdAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Sku = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }
}
