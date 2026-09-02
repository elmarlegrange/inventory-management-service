using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);
    Task CreateAsync(Product product, CancellationToken cancellationToken = default);
}
