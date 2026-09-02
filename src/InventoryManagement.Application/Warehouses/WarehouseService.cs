using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Warehouses;

public sealed class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
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

        var warehouse = await _warehouseRepository.GetByCodeAsync(code.Trim(), cancellationToken);
        if (warehouse is null)
        {
            throw new EntityNotFoundException("Warehouse", code.Trim().ToUpperInvariant());
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
}
