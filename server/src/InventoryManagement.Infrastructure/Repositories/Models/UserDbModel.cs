namespace InventoryManagement.Infrastructure.Repositories.Models;

public sealed class UserDbModel
{
    public Guid id { get; set; }
    public string username { get; set; } = string.Empty;
    public string password_hash { get; set; } = string.Empty;
    public Guid role_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}
