using Dapper;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.Data;

/// <summary>
/// Initializes the PostgreSQL 17 database schema, tables, indices, and constraints.
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
                code VARCHAR(50) PRIMARY KEY,
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
                product_code VARCHAR(50) NOT NULL REFERENCES products(code) ON DELETE CASCADE,
                quantity INT NOT NULL DEFAULT 0,
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                PRIMARY KEY (warehouse_code, product_code),
                CONSTRAINT chk_stock_quantity_non_negative CHECK (quantity >= 0)
            );

            -- 4. Orders Table
            CREATE TABLE IF NOT EXISTS orders (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                product_code VARCHAR(50) NOT NULL REFERENCES products(code),
                source_warehouse_code VARCHAR(50) NOT NULL REFERENCES warehouses(code),
                destination_warehouse_code VARCHAR(50) NOT NULL REFERENCES warehouses(code),
                quantity INT NOT NULL CHECK (quantity > 0),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            -- Performance Indices
            CREATE INDEX IF NOT EXISTS idx_stock_product_code ON stock(product_code);
            CREATE INDEX IF NOT EXISTS idx_orders_product_code ON orders(product_code);
            CREATE INDEX IF NOT EXISTS idx_orders_source_warehouse ON orders(source_warehouse_code);
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));

        _logger.LogInformation("PostgreSQL database schema initialized successfully.");
    }
}
