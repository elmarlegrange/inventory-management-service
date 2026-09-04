using InventoryManagement.Application.Auth;
using Shouldly;

namespace InventoryManagement.UnitTests.Application.Auth;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void HashPassword_ShouldReturnSaltAndHashFormat()
    {
        // Act
        var hash = _sut.HashPassword("Admin123!");

        // Assert
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.ShouldContain(':');
        var parts = hash.Split(':');
        parts.Length.ShouldBe(2);
        Convert.FromBase64String(parts[0]).Length.ShouldBe(16); // 16 bytes salt
        Convert.FromBase64String(parts[1]).Length.ShouldBe(32); // 32 bytes hash
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _sut.HashPassword(password);

        // Act
        var isValid = _sut.VerifyPassword(password, hash);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var hash = _sut.HashPassword("CorrectPassword");

        // Act
        var isValid = _sut.VerifyPassword("WrongPassword", hash);

        // Assert
        isValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalidhash")]
    [InlineData("part1:part2:part3")]
    public void VerifyPassword_WithMalformedHash_ShouldReturnFalse(string malformedHash)
    {
        // Act
        var isValid = _sut.VerifyPassword("Password", malformedHash);

        // Assert
        isValid.ShouldBeFalse();
    }
}
