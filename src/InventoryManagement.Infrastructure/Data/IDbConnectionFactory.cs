using Npgsql;

namespace InventoryManagement.Infrastructure.Data;

/// <summary>
/// Factory abstraction for creating asynchronous PostgreSQL database connections.
/// </summary>
public interface IDbConnectionFactory
{
    Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
