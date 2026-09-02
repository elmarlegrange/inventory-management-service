using Dapper;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Models;

namespace InventoryManagement.Infrastructure.Repositories;

public sealed class StockRepository : IStockRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public StockRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Stock?> GetStockAsync(string warehouseCode, string productCode, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT warehouse_code AS WarehouseCode, product_code AS ProductCode, quantity AS Quantity, updated_at AS UpdatedAt
            FROM stock
            WHERE UPPER(warehouse_code) = UPPER(@WarehouseCode) AND UPPER(product_code) = UPPER(@ProductCode);
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<StockDbModel>(
            new CommandDefinition(sql, new { WarehouseCode = warehouseCode, ProductCode = productCode }, cancellationToken: cancellationToken));

        return result is null ? null : new Stock(result.WarehouseCode, result.ProductCode, result.Quantity, result.UpdatedAt);
    }

    public async Task<IReadOnlyList<Stock>> GetStockByWarehouseCodeAsync(string warehouseCode, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT warehouse_code AS WarehouseCode, product_code AS ProductCode, quantity AS Quantity, updated_at AS UpdatedAt
            FROM stock
            WHERE UPPER(warehouse_code) = UPPER(@WarehouseCode)
            ORDER BY product_code ASC;
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<StockDbModel>(
            new CommandDefinition(sql, new { WarehouseCode = warehouseCode }, cancellationToken: cancellationToken));

        return result.Select(r => new Stock(r.WarehouseCode, r.ProductCode, r.Quantity, r.UpdatedAt)).ToList();
    }

    public async Task<IReadOnlyList<Stock>> GetStockByProductCodeAsync(string productCode, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT warehouse_code AS WarehouseCode, product_code AS ProductCode, quantity AS Quantity, updated_at AS UpdatedAt
            FROM stock
            WHERE UPPER(product_code) = UPPER(@ProductCode)
            ORDER BY warehouse_code ASC;
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<StockDbModel>(
            new CommandDefinition(sql, new { ProductCode = productCode }, cancellationToken: cancellationToken));

        return result.Select(r => new Stock(r.WarehouseCode, r.ProductCode, r.Quantity, r.UpdatedAt)).ToList();
    }

    public async Task<IReadOnlyList<WarehouseStockItem>> GetWarehouseStockDetailsAsync(string warehouseCode, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT s.product_code AS ProductCode, p.name AS ProductName, s.quantity AS Quantity, s.updated_at AS UpdatedAt
            FROM stock s
            JOIN products p ON UPPER(s.product_code) = UPPER(p.code)
            WHERE UPPER(s.warehouse_code) = UPPER(@WarehouseCode)
            ORDER BY s.product_code ASC;
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<WarehouseStockItemDbModel>(
            new CommandDefinition(sql, new { WarehouseCode = warehouseCode }, cancellationToken: cancellationToken));

        return result.Select(r => new WarehouseStockItem(r.ProductCode, r.ProductName, r.Quantity, r.UpdatedAt)).ToList();
    }

    public async Task<IReadOnlyList<ProductStockLocation>> GetProductStockDetailsAsync(string productCode, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT s.warehouse_code AS WarehouseCode, w.name AS WarehouseName, s.quantity AS Quantity, s.updated_at AS UpdatedAt
            FROM stock s
            JOIN warehouses w ON UPPER(s.warehouse_code) = UPPER(w.code)
            WHERE UPPER(s.product_code) = UPPER(@ProductCode)
            ORDER BY s.warehouse_code ASC;
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<ProductStockLocationDbModel>(
            new CommandDefinition(sql, new { ProductCode = productCode }, cancellationToken: cancellationToken));

        return result.Select(r => new ProductStockLocation(r.WarehouseCode, r.WarehouseName, r.Quantity, r.UpdatedAt)).ToList();
    }

    public async Task UpsertStockAsync(string warehouseCode, string productCode, int quantity, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO stock (warehouse_code, product_code, quantity, updated_at)
            VALUES (UPPER(@WarehouseCode), UPPER(@ProductCode), @Quantity, NOW())
            ON CONFLICT (warehouse_code, product_code)
            DO UPDATE SET quantity = EXCLUDED.quantity, updated_at = NOW();
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { WarehouseCode = warehouseCode, ProductCode = productCode, Quantity = quantity },
            cancellationToken: cancellationToken));
    }
}
