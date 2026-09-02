using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Interfaces;

public interface ITransferRepository
{
    Task<StockTransfer> ExecuteTransferAsync(
        string sku,
        string sourceWarehouseCode,
        string destinationWarehouseCode,
        int quantity,
        CancellationToken cancellationToken = default);
}
