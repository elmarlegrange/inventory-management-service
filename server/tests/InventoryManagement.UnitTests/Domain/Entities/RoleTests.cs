using InventoryManagement.Domain.Entities;
using Shouldly;

namespace InventoryManagement.UnitTests.Domain.Entities;

public sealed class RoleTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Manager";
        var desc = "Warehouse Manager";

        // Act
        var role = new Role(id, name, desc);

        // Assert
        role.Id.ShouldBe(id);
        role.Name.ShouldBe(name);
        role.Description.ShouldBe(desc);
        role.CreatedAt.ShouldNotBe(default);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Role(Guid.NewGuid(), invalidName!));
    }
}
