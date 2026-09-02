using InventoryManagement.Application.Stock;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Warehouses;

public sealed class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockRepository _stockRepository;

    public WarehouseService(
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IStockRepository stockRepository)
    {
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _stockRepository = stockRepository;
    }

    public async Task<IReadOnlyList<WarehouseDto>> GetAllWarehousesAsync(CancellationToken cancellationToken = default)
    {
        var warehouses = await _warehouseRepository.GetAllAsync(cancellationToken);
        return warehouses.Select(w => new WarehouseDto(w.Code, w.Name, w.CreatedAt)).ToList();
    }

    public async Task<WarehouseDto> GetWarehouseByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ValidationException("code", "Warehouse code must not be empty.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var warehouse = await _warehouseRepository.GetByCodeAsync(normalizedCode, cancellationToken);
        if (warehouse is null)
        {
            throw new EntityNotFoundException("Warehouse", normalizedCode);
        }

        return new WarehouseDto(warehouse.Code, warehouse.Name, warehouse.CreatedAt);
    }

    public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            errors["code"] = new[] { "Warehouse code is required and cannot be empty." };
        }
        else if (request.Code.Trim().Length > 50)
        {
            errors["code"] = new[] { "Warehouse code must not exceed 50 characters." };
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = new[] { "Warehouse name is required and cannot be empty." };
        }
        else if (request.Name.Trim().Length > 255)
        {
            errors["name"] = new[] { "Warehouse name must not exceed 255 characters." };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Warehouse validation failed.", errors);
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var exists = await _warehouseRepository.ExistsAsync(normalizedCode, cancellationToken);
        if (exists)
        {
            throw new DuplicateEntityException("Warehouse", normalizedCode);
        }

        var warehouse = new Warehouse(normalizedCode, request.Name.Trim());
        await _warehouseRepository.CreateAsync(warehouse, cancellationToken);

        return new WarehouseDto(warehouse.Code, warehouse.Name, warehouse.CreatedAt);
    }

    public async Task<IReadOnlyList<WarehouseStockItemDto>> GetStockForWarehouseAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ValidationException("code", "Warehouse code must not be empty.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var exists = await _warehouseRepository.ExistsAsync(normalizedCode, cancellationToken);
        if (!exists)
        {
            throw new EntityNotFoundException("Warehouse", normalizedCode);
        }

        var items = await _stockRepository.GetWarehouseStockDetailsAsync(normalizedCode, cancellationToken);
        return items.Select(i => new WarehouseStockItemDto(i.ProductCode, i.ProductName, i.Quantity, i.UpdatedAt)).ToList();
    }

    public async Task AddStockToWarehouseAsync(string code, AddStockItemRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ValidationException("code", "Warehouse code must not be empty.");
        }

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.ProductCode))
        {
            errors["productCode"] = new[] { "Product code is required and cannot be empty." };
        }
        else if (request.ProductCode.Trim().Length > 50)
        {
            errors["productCode"] = new[] { "Product code must not exceed 50 characters." };
        }

        if (request.Quantity <= 0)
        {
            errors["quantity"] = new[] { "Stock quantity must be a positive integer greater than zero." };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Add stock validation failed.", errors);
        }

        var normalizedWarehouseCode = code.Trim().ToUpperInvariant();
        var normalizedProductCode = request.ProductCode.Trim().ToUpperInvariant();

        var warehouseExists = await _warehouseRepository.ExistsAsync(normalizedWarehouseCode, cancellationToken);
        if (!warehouseExists)
        {
            throw new EntityNotFoundException("Warehouse", normalizedWarehouseCode);
        }

        var productExists = await _productRepository.ExistsAsync(normalizedProductCode, cancellationToken);
        if (!productExists)
        {
            throw new EntityNotFoundException("Product", normalizedProductCode);
        }

        await _stockRepository.UpsertStockAsync(normalizedWarehouseCode, normalizedProductCode, request.Quantity, cancellationToken);
    }
}
