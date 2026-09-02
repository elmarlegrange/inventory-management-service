using InventoryManagement.Application.Stock;

namespace InventoryManagement.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductStockLocationDto>> GetStockForProductAsync(string code, CancellationToken cancellationToken = default);
}
