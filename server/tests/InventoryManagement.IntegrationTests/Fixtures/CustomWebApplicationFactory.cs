using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Dapper;
using InventoryManagement.Application.Auth;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace InventoryManagement.IntegrationTests.Fixtures;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestJwtKey = "super_secret_dev_key_with_at_least_32_bytes_length!!";
    public const string TestIssuer = "InventoryManagement";
    public const string TestAudience = "InventoryManagement";

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
        var passwordHasher = new PasswordHasher();
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseInitializer>();
        var initializer = new DatabaseInitializer(connectionFactory, passwordHasher, logger);
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

    public static string GenerateTestToken(string username = "admin", string role = "Admin")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateTestToken("admin", "Admin"));
    }

    public HttpClient CreateAnonymousClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }

    public HttpClient CreateUserClient(string username = "user", string role = "User")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateTestToken(username, role));
        return client;
    }

    public HttpClient CreateAdminClient()
    {
        return CreateClient();
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
