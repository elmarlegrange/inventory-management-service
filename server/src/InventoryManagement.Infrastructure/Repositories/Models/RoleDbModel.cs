namespace InventoryManagement.Infrastructure.Repositories.Models;

public sealed record RoleDbModel(
    Guid id,
    string name,
    string? description,
    DateTime created_at
);
