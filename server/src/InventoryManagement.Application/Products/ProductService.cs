using InventoryManagement.Application.Stock;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Products;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IStockRepository _stockRepository;

    public ProductService(IProductRepository productRepository, IStockRepository stockRepository)
    {
        _productRepository = productRepository;
        _stockRepository = stockRepository;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(p => new ProductDto(p.Code, p.Name, p.CreatedAt)).ToList();
    }

    public async Task<ProductDto> GetProductByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ValidationException("code", "Product code must not be empty.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var product = await _productRepository.GetByCodeAsync(normalizedCode, cancellationToken);
        if (product is null)
        {
            throw new EntityNotFoundException("Product", normalizedCode);
        }

        return new ProductDto(product.Code, product.Name, product.CreatedAt);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            errors["code"] = new[] { "Product code is required and cannot be empty." };
        }
        else if (request.Code.Trim().Length > 50)
        {
            errors["code"] = new[] { "Product code must not exceed 50 characters." };
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

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var exists = await _productRepository.ExistsAsync(normalizedCode, cancellationToken);
        if (exists)
        {
            throw new DuplicateEntityException("Product", normalizedCode);
        }

        var product = new Product(normalizedCode, request.Name.Trim());
        await _productRepository.CreateAsync(product, cancellationToken);

        return new ProductDto(product.Code, product.Name, product.CreatedAt);
    }

    public async Task<IReadOnlyList<ProductStockLocationDto>> GetStockForProductAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ValidationException("code", "Product code must not be empty.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var exists = await _productRepository.ExistsAsync(normalizedCode, cancellationToken);
        if (!exists)
        {
            throw new EntityNotFoundException("Product", normalizedCode);
        }

        var locations = await _stockRepository.GetProductStockDetailsAsync(normalizedCode, cancellationToken);
        return locations.Select(l => new ProductStockLocationDto(l.WarehouseCode, l.WarehouseName, l.Quantity, l.UpdatedAt)).ToList();
    }
}
