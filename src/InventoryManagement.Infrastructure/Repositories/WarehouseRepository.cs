using Dapper;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Models;
using Npgsql;

namespace InventoryManagement.Infrastructure.Repositories;

public sealed class WarehouseRepository : IWarehouseRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public WarehouseRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT code, name, created_at AS CreatedAt
            FROM warehouses
            ORDER BY created_at DESC;
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<WarehouseDbModel>(new CommandDefinition(sql, cancellationToken: cancellationToken));

        return result.Select(r => new Warehouse(r.Code, r.Name, r.CreatedAt)).ToList();
    }

    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT code, name, created_at AS CreatedAt
            FROM warehouses
            WHERE UPPER(code) = UPPER(@Code);
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<WarehouseDbModel>(
            new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken));

        return result is null ? null : new Warehouse(result.Code, result.Name, result.CreatedAt);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM warehouses
            WHERE UPPER(code) = UPPER(@Code);
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task CreateAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO warehouses (code, name, created_at)
            VALUES (@Code, @Name, @CreatedAt);
        """;

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new { warehouse.Code, warehouse.Name, warehouse.CreatedAt },
                cancellationToken: cancellationToken));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new DuplicateEntityException("Warehouse", warehouse.Code);
        }
    }
}
