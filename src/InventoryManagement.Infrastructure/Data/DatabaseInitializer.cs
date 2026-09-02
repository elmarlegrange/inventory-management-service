using Dapper;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.Data;

/// <summary>
/// Initializes the PostgreSQL database schema, tables, indices, and constraints.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing PostgreSQL database schema...");

        const string sql = """
            -- 1. Products Table
            CREATE TABLE IF NOT EXISTS products (
                sku VARCHAR(50) PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            -- 2. Warehouses Table
            CREATE TABLE IF NOT EXISTS warehouses (
                code VARCHAR(50) PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            -- 3. Stock Table (Composite PK + Invariant Check Constraint)
            CREATE TABLE IF NOT EXISTS stock (
                warehouse_code VARCHAR(50) NOT NULL REFERENCES warehouses(code) ON DELETE CASCADE,
                sku VARCHAR(50) NOT NULL REFERENCES products(sku) ON DELETE CASCADE,
                quantity INT NOT NULL DEFAULT 0,
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                PRIMARY KEY (warehouse_code, sku),
                CONSTRAINT chk_stock_quantity_non_negative CHECK (quantity >= 0)
            );

            -- 4. Stock Transfers / Audit Trail
            CREATE TABLE IF NOT EXISTS stock_transfers (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                sku VARCHAR(50) NOT NULL REFERENCES products(sku),
                source_warehouse_code VARCHAR(50) NOT NULL REFERENCES warehouses(code),
                destination_warehouse_code VARCHAR(50) NOT NULL REFERENCES warehouses(code),
                quantity INT NOT NULL CHECK (quantity > 0),
                transferred_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            -- Performance Indices
            CREATE INDEX IF NOT EXISTS idx_stock_sku ON stock(sku);
            CREATE INDEX IF NOT EXISTS idx_transfers_sku ON stock_transfers(sku);
            CREATE INDEX IF NOT EXISTS idx_transfers_source ON stock_transfers(source_warehouse_code);
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));

        _logger.LogInformation("PostgreSQL database schema initialized successfully.");
    }
}
