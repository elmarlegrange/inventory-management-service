namespace InventoryManagement.Domain.Entities;

public sealed class Role
{
    public Guid Id { get; }
    public string Name { get; }
    public string? Description { get; }
    public DateTime CreatedAt { get; }

    public Role(Guid id, string name, string? description = null, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        Id = id;
        Name = name.Trim();
        Description = description?.Trim();
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }
}
