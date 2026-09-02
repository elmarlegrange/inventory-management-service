using Dapper;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Models;
using Npgsql;

namespace InventoryManagement.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT sku, name, created_at AS CreatedAt
            FROM products
            ORDER BY created_at DESC;
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<ProductDbModel>(new CommandDefinition(sql, cancellationToken: cancellationToken));

        return result.Select(r => new Product(r.Sku, r.Name, r.CreatedAt)).ToList();
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT sku, name, created_at AS CreatedAt
            FROM products
            WHERE UPPER(sku) = UPPER(@Sku);
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<ProductDbModel>(
            new CommandDefinition(sql, new { Sku = sku }, cancellationToken: cancellationToken));

        return result is null ? null : new Product(result.Sku, result.Name, result.CreatedAt);
    }

    public async Task<bool> ExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM products
            WHERE UPPER(sku) = UPPER(@Sku);
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Sku = sku }, cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO products (sku, name, created_at)
            VALUES (@Sku, @Name, @CreatedAt);
        """;

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new { product.Sku, product.Name, product.CreatedAt },
                cancellationToken: cancellationToken));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new DuplicateEntityException("Product", product.Sku);
        }
    }
}
