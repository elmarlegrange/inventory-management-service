namespace InventoryManagement.Application.Orders;

public sealed record CreateOrderRequest(
    string ProductCode,
    string SourceWarehouseCode,
    string DestinationWarehouseCode,
    int Quantity);
