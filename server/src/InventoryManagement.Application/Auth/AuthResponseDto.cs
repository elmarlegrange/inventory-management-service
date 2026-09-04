namespace InventoryManagement.Application.Auth;

public sealed record AuthResponseDto(string Token, DateTime ExpiresAt, string Username, string Role);
