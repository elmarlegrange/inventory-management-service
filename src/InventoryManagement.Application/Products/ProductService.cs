using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Products;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(p => new ProductDto(p.Sku, p.Name, p.CreatedAt)).ToList();
    }

    public async Task<ProductDto> GetProductBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ValidationException("sku", "Product SKU must not be empty.");
        }

        var product = await _productRepository.GetBySkuAsync(sku.Trim(), cancellationToken);
        if (product is null)
        {
            throw new EntityNotFoundException("Product", sku.Trim().ToUpperInvariant());
        }

        return new ProductDto(product.Sku, product.Name, product.CreatedAt);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            errors["sku"] = new[] { "Product SKU is required and cannot be empty." };
        }
        else if (request.Sku.Trim().Length > 50)
        {
            errors["sku"] = new[] { "Product SKU must not exceed 50 characters." };
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = new[] { "Product name is required and cannot be empty." };
        }
        else if (request.Name.Trim().Length > 255)
        {
            errors["name"] = new[] { "Product name must not exceed 255 characters." };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Product validation failed.", errors);
        }

        var normalizedSku = request.Sku.Trim().ToUpperInvariant();
        var exists = await _productRepository.ExistsAsync(normalizedSku, cancellationToken);
        if (exists)
        {
            throw new DuplicateEntityException("Product", normalizedSku);
        }

        var product = new Product(normalizedSku, request.Name.Trim());
        await _productRepository.CreateAsync(product, cancellationToken);

        return new ProductDto(product.Sku, product.Name, product.CreatedAt);
    }
}
