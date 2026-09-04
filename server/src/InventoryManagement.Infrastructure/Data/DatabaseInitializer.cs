using Dapper;
using InventoryManagement.Application.Auth;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.Data;

/// <summary>
/// Initializes the PostgreSQL database schema, tables, indices, constraints, and default seed data.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IDbConnectionFactory connectionFactory,
        IPasswordHasher passwordHasher,
        ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _passwordHasher = passwordHasher;
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

            -- 5. Roles Table
            CREATE TABLE IF NOT EXISTS roles (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(50) UNIQUE NOT NULL,
                description VARCHAR(255),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            -- Seed standard roles if not present
            INSERT INTO roles (id, name, description)
            VALUES 
                ('a0000000-0000-0000-0000-000000000001', 'Admin', 'Full administrative access including warehouse scoping'),
                ('a0000000-0000-0000-0000-000000000002', 'User', 'Standard user with catalog and transfer access')
            ON CONFLICT (name) DO NOTHING;

            -- 6. Users Table
            CREATE TABLE IF NOT EXISTS users (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                username VARCHAR(100) UNIQUE NOT NULL,
                password_hash VARCHAR(255) NOT NULL,
                role_id UUID NOT NULL REFERENCES roles(id),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            -- Migration check for existing users table that had 'role' column instead of 'role_id'
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 
                    FROM information_schema.columns 
                    WHERE table_name = 'users' AND column_name = 'role'
                ) THEN
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'users' AND column_name = 'role_id'
                    ) THEN
                        ALTER TABLE users ADD COLUMN role_id UUID REFERENCES roles(id);
                        UPDATE users u SET role_id = r.id FROM roles r WHERE UPPER(u.role) = UPPER(r.name);
                        ALTER TABLE users ALTER COLUMN role_id SET NOT NULL;
                        ALTER TABLE users DROP COLUMN role;
                    END IF;
                END IF;
            END $$;

            -- Performance Indices
            CREATE INDEX IF NOT EXISTS idx_stock_product_code ON stock(product_code);
            CREATE INDEX IF NOT EXISTS idx_orders_product_code ON orders(product_code);
            CREATE INDEX IF NOT EXISTS idx_orders_source_warehouse ON orders(source_warehouse_code);
            CREATE INDEX IF NOT EXISTS idx_users_username ON users(UPPER(username));
            CREATE INDEX IF NOT EXISTS idx_users_role_id ON users(role_id);
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));

        // Seed default accounts if users table is empty
        var userCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition("SELECT COUNT(*) FROM users;", cancellationToken: cancellationToken));

        if (userCount == 0)
        {
            _logger.LogInformation("Seeding default Admin and User accounts...");

            var adminHash = _passwordHasher.HashPassword("Admin123!");
            var userHash = _passwordHasher.HashPassword("User123!");

            const string seedSql = """
                INSERT INTO users (id, username, password_hash, role_id, created_at, updated_at)
                VALUES 
                    (gen_random_uuid(), 'admin', @AdminHash, (SELECT id FROM roles WHERE name = 'Admin'), NOW(), NOW()),
                    (gen_random_uuid(), 'user', @UserHash, (SELECT id FROM roles WHERE name = 'User'), NOW(), NOW());
            """;

            await connection.ExecuteAsync(
                new CommandDefinition(seedSql, new { AdminHash = adminHash, UserHash = userHash }, cancellationToken: cancellationToken));

            _logger.LogInformation("Default Admin and User accounts seeded successfully.");
        }

        _logger.LogInformation("PostgreSQL database schema initialized successfully.");
    }
}
