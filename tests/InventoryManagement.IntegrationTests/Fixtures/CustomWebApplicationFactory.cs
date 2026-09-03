using Dapper;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace InventoryManagement.IntegrationTests.Fixtures;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("inventory_test_db")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        // Initialize schema
        var connectionFactory = new NpgsqlConnectionFactory(_postgresContainer.GetConnectionString());
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseInitializer>();
        var initializer = new DatabaseInitializer(connectionFactory, logger);
        await initializer.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        var connectionFactory = new NpgsqlConnectionFactory(_postgresContainer.GetConnectionString());
        await using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
            TRUNCATE TABLE orders, stock, products, warehouses CASCADE;
        """;
        await connection.ExecuteAsync(sql);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbConnectionFactory>();
            services.AddSingleton<IDbConnectionFactory>(_ =>
                new NpgsqlConnectionFactory(_postgresContainer.GetConnectionString()));
        });
    }
}
