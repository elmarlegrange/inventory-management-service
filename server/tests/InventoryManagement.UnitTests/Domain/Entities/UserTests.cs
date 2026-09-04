using InventoryManagement.Domain.Entities;
using Shouldly;

namespace InventoryManagement.UnitTests.Domain.Entities;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var user = new User(id, "admin", "hash123", roleId, now, now);

        // Assert
        user.Id.ShouldBe(id);
        user.Username.ShouldBe("admin");
        user.PasswordHash.ShouldBe("hash123");
        user.RoleId.ShouldBe(roleId);
        user.CreatedAt.ShouldBe(now);
        user.UpdatedAt.ShouldBe(now);
    }

    [Fact]
    public void Constructor_WithEmptyRoleId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            new User(Guid.NewGuid(), "admin", "hash123", Guid.Empty, DateTime.UtcNow, DateTime.UtcNow));
    }
}
