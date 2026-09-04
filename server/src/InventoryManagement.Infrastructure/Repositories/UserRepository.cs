using Dapper;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Models;

namespace InventoryManagement.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id, username, password_hash, role_id, created_at, updated_at
            FROM users
            WHERE UPPER(username) = UPPER(@Username);
        """;

        var model = await connection.QuerySingleOrDefaultAsync<UserDbModel>(
            new CommandDefinition(sql, new { Username = username }, cancellationToken: cancellationToken));

        if (model is null) return null;

        return new User(model.id, model.username, model.password_hash, model.role_id, model.created_at, model.updated_at);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id, username, password_hash, role_id, created_at, updated_at
            FROM users
            WHERE id = @Id;
        """;

        var model = await connection.QuerySingleOrDefaultAsync<UserDbModel>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        if (model is null) return null;

        return new User(model.id, model.username, model.password_hash, model.role_id, model.created_at, model.updated_at);
    }

    public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            INSERT INTO users (id, username, password_hash, role_id, created_at, updated_at)
            VALUES (@Id, @Username, @PasswordHash, @RoleId, @CreatedAt, @UpdatedAt);
        """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    Id = user.Id,
                    Username = user.Username,
                    PasswordHash = user.PasswordHash,
                    RoleId = user.RoleId,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                },
                cancellationToken: cancellationToken));

        return user;
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = "SELECT COUNT(*) FROM users;";
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
