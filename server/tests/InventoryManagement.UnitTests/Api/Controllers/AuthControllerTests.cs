using System.Security.Claims;
using InventoryManagement.Api.Controllers;
using InventoryManagement.Application.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace InventoryManagement.UnitTests.Api.Controllers;

public sealed class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _sut = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnOkWithAuthResponseDto()
    {
        // Arrange
        var request = new LoginRequest("admin", "Admin123!");
        var expectedResponse = new AuthResponseDto("mock-token", DateTime.UtcNow.AddHours(24), "admin", "Admin");

        _authServiceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.ShouldNotBeNull();
        okResult.Value.ShouldBe(expectedResponse);
    }

    [Fact]
    public void GetCurrentUser_ShouldReturnUserDtoFromClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = _sut.GetCurrentUser();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.ShouldNotBeNull();
        var userDto = okResult.Value as UserDto;
        userDto.ShouldNotBeNull();
        userDto.Id.ShouldBe(userId);
        userDto.Username.ShouldBe("admin");
        userDto.Role.ShouldBe("Admin");
    }
}
