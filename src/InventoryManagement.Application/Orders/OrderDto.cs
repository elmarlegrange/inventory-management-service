namespace InventoryManagement.Application.Orders;

public sealed record OrderDto(
    Guid Id,
    string ProductCode,
    string SourceWarehouseCode,
    string DestinationWarehouseCode,
    int Quantity,
    DateTime CreatedAt);
