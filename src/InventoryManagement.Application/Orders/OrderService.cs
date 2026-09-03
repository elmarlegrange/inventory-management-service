using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.ProductCode))
        {
            errors["productCode"] = new[] { "Product code is required and cannot be empty." };
        }
        else if (request.ProductCode.Trim().Length > 50)
        {
            errors["productCode"] = new[] { "Product code must not exceed 50 characters." };
        }

        if (string.IsNullOrWhiteSpace(request.SourceWarehouseCode))
        {
            errors["sourceWarehouseCode"] = new[] { "Source warehouse code is required and cannot be empty." };
        }
        else if (request.SourceWarehouseCode.Trim().Length > 50)
        {
            errors["sourceWarehouseCode"] = new[] { "Source warehouse code must not exceed 50 characters." };
        }

        if (string.IsNullOrWhiteSpace(request.DestinationWarehouseCode))
        {
            errors["destinationWarehouseCode"] = new[] { "Destination warehouse code is required and cannot be empty." };
        }
        else if (request.DestinationWarehouseCode.Trim().Length > 50)
        {
            errors["destinationWarehouseCode"] = new[] { "Destination warehouse code must not exceed 50 characters." };
        }

        if (request.Quantity <= 0)
        {
            errors["quantity"] = new[] { "Order quantity must be a positive integer (greater than zero)." };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Order validation failed.", errors);
        }

        var normalizedProductCode = request.ProductCode.Trim().ToUpperInvariant();
        var normalizedSourceCode = request.SourceWarehouseCode.Trim().ToUpperInvariant();
        var normalizedDestCode = request.DestinationWarehouseCode.Trim().ToUpperInvariant();

        if (normalizedSourceCode.Equals(normalizedDestCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOrderException("Source and destination warehouses cannot be identical.");
        }

        var productExists = await _productRepository.ExistsAsync(normalizedProductCode, cancellationToken);
        if (!productExists)
        {
            throw new EntityNotFoundException("Product", normalizedProductCode);
        }

        var sourceWarehouseExists = await _warehouseRepository.ExistsAsync(normalizedSourceCode, cancellationToken);
        if (!sourceWarehouseExists)
        {
            throw new EntityNotFoundException("Warehouse", normalizedSourceCode);
        }

        var destinationWarehouseExists = await _warehouseRepository.ExistsAsync(normalizedDestCode, cancellationToken);
        if (!destinationWarehouseExists)
        {
            throw new EntityNotFoundException("Warehouse", normalizedDestCode);
        }

        var order = await _orderRepository.CreateOrderAsync(
            normalizedProductCode,
            normalizedSourceCode,
            normalizedDestCode,
            request.Quantity,
            cancellationToken);

        return new OrderDto(
            order.Id,
            order.ProductCode,
            order.SourceWarehouseCode,
            order.DestinationWarehouseCode,
            order.Quantity,
            order.CreatedAt);
    }
}
