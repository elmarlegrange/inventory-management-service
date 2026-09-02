namespace InventoryManagement.Infrastructure.Repositories.Models;

public sealed class ProductDbModel
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
