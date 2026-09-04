namespace InventoryManagement.Domain.Entities;

public sealed class User
{
    public Guid Id { get; }
    public string Username { get; }
    public string PasswordHash { get; }
    public Guid RoleId { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public User(Guid id, string username, string passwordHash, Guid roleId, DateTime createdAt, DateTime updatedAt)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        Id = id;
        Username = username.Trim();
        PasswordHash = passwordHash;
        RoleId = roleId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
