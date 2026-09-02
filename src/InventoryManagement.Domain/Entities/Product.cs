namespace InventoryManagement.Domain.Entities;

public sealed class Product
{
    public string Code { get; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; }

    public Product(string code, string name, DateTime? createdAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }
}
