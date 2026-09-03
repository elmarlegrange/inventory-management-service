using System.Data;
using Dapper;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infrastructure.Data;

namespace InventoryManagement.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Order> CreateOrderAsync(
        string productCode,
        string sourceWarehouseCode,
        string destinationWarehouseCode,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            // 1. Lock source row via Pessimistic Lock (SELECT FOR UPDATE)
            const string lockSql = """
                SELECT quantity 
                FROM stock 
                WHERE UPPER(warehouse_code) = UPPER(@SourceWarehouseCode) 
                  AND UPPER(product_code) = UPPER(@ProductCode) 
                FOR UPDATE;
            """;

            var sourceQuantity = await connection.QuerySingleOrDefaultAsync<int?>(
                new CommandDefinition(
                    lockSql,
                    new { SourceWarehouseCode = sourceWarehouseCode, ProductCode = productCode },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            if (sourceQuantity is null || sourceQuantity.Value < quantity)
            {
                int available = sourceQuantity ?? 0;
                await transaction.RollbackAsync(cancellationToken);
                throw new InsufficientStockException(productCode, sourceWarehouseCode, quantity, available);
            }

            // 2. Debit source warehouse stock
            const string debitSql = """
                UPDATE stock 
                SET quantity = quantity - @Quantity, updated_at = NOW() 
                WHERE UPPER(warehouse_code) = UPPER(@SourceWarehouseCode) 
                  AND UPPER(product_code) = UPPER(@ProductCode);
            """;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    debitSql,
                    new { SourceWarehouseCode = sourceWarehouseCode, ProductCode = productCode, Quantity = quantity },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 3. Credit destination warehouse stock (Upsert)
            const string creditSql = """
                INSERT INTO stock (warehouse_code, product_code, quantity, updated_at)
                VALUES (UPPER(@DestinationWarehouseCode), UPPER(@ProductCode), @Quantity, NOW())
                ON CONFLICT (warehouse_code, product_code)
                DO UPDATE SET quantity = stock.quantity + EXCLUDED.quantity, updated_at = NOW();
            """;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    creditSql,
                    new { DestinationWarehouseCode = destinationWarehouseCode, ProductCode = productCode, Quantity = quantity },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 4. Record order entry in database
            var orderId = Guid.NewGuid();
            const string orderSql = """
                INSERT INTO orders (id, product_code, source_warehouse_code, destination_warehouse_code, quantity, created_at)
                VALUES (@Id, UPPER(@ProductCode), UPPER(@SourceWarehouseCode), UPPER(@DestinationWarehouseCode), @Quantity, NOW());
            """;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    orderSql,
                    new
                    {
                        Id = orderId,
                        ProductCode = productCode,
                        SourceWarehouseCode = sourceWarehouseCode,
                        DestinationWarehouseCode = destinationWarehouseCode,
                        Quantity = quantity
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 5. Commit transaction atomically
            await transaction.CommitAsync(cancellationToken);

            return new Order(orderId, productCode, sourceWarehouseCode, destinationWarehouseCode, quantity, DateTime.UtcNow);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
