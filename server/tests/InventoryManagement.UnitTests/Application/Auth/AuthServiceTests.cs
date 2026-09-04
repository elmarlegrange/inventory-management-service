using InventoryManagement.Application.Auth;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;
using Moq;
using Shouldly;

namespace InventoryManagement.UnitTests.Application.Auth;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _sut = new AuthService(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponseDto()
    {
        // Arrange
        var request = new LoginRequest("admin", "Admin123!");
        var roleId = Guid.NewGuid();
        var user = new User(Guid.NewGuid(), "admin", "hashed_pwd", roleId, DateTime.UtcNow, DateTime.UtcNow);
        var role = new Role(roleId, "Admin");
        var expectedExpiry = DateTime.UtcNow.AddHours(24);

        _userRepoMock.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roleRepoMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _passwordHasherMock.Setup(h => h.VerifyPassword("Admin123!", "hashed_pwd"))
            .Returns(true);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(user, "Admin"))
            .Returns(("mock.jwt.token", expectedExpiry));

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Token.ShouldBe("mock.jwt.token");
        result.Username.ShouldBe("admin");
        result.Role.ShouldBe("Admin");
        result.ExpiresAt.ShouldBe(expectedExpiry);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    [InlineData("admin", "")]
    [InlineData("admin", "   ")]
    public async Task LoginAsync_WithEmptyCredentials_ShouldThrowInvalidCredentialsException(string username, string password)
    {
        // Arrange
        var request = new LoginRequest(username, password);

        // Act & Assert
        await Should.ThrowAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("unknown", "Password123!");
        _userRepoMock.Setup(r => r.GetByUsernameAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Should.ThrowAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordVerificationFails_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("admin", "WrongPassword!");
        var user = new User(Guid.NewGuid(), "admin", "hashed_pwd", Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow);

        _userRepoMock.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword("WrongPassword!", "hashed_pwd"))
            .Returns(false);

        // Act & Assert
        await Should.ThrowAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WhenRoleNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new LoginRequest("admin", "Admin123!");
        var roleId = Guid.NewGuid();
        var user = new User(Guid.NewGuid(), "admin", "hashed_pwd", roleId, DateTime.UtcNow, DateTime.UtcNow);

        _userRepoMock.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword("Admin123!", "hashed_pwd"))
            .Returns(true);
        _roleRepoMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.LoginAsync(request));
        ex.Message.ShouldContain("valid assigned role");
    }
}
