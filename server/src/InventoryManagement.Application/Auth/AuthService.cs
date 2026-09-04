using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidCredentialsException("Username and password are required.");
        }

        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);
        if (role is null)
        {
            throw new InvalidOperationException("User account does not have a valid assigned role.");
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user, role.Name);

        return new AuthResponseDto(token, expiresAt, user.Username, role.Name);
    }
}
