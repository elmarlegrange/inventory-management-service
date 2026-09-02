namespace InventoryManagement.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
}
