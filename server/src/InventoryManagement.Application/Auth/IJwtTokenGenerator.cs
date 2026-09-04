using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Auth;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user, string roleName);
}
