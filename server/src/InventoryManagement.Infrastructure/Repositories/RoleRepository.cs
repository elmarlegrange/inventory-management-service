using Dapper;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Models;

namespace InventoryManagement.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id, name, description, created_at
            FROM roles
            WHERE UPPER(name) = UPPER(@Name);
        """;

        var model = await connection.QuerySingleOrDefaultAsync<RoleDbModel>(
            new CommandDefinition(sql, new { Name = name }, cancellationToken: cancellationToken));

        if (model is null) return null;

        return new Role(model.id, model.name, model.description, model.created_at);
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id, name, description, created_at
            FROM roles
            WHERE id = @Id;
        """;

        var model = await connection.QuerySingleOrDefaultAsync<RoleDbModel>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        if (model is null) return null;

        return new Role(model.id, model.name, model.description, model.created_at);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id, name, description, created_at
            FROM roles
            ORDER BY name ASC;
        """;

        var models = await connection.QueryAsync<RoleDbModel>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return models.Select(m => new Role(m.id, m.name, m.description, m.created_at)).ToList();
    }

    public async Task<Role> CreateRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            INSERT INTO roles (id, name, description, created_at)
            VALUES (@Id, @Name, @Description, @CreatedAt);
        """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt
                },
                cancellationToken: cancellationToken));

        return role;
    }
}
