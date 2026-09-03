using InventoryManagement.Domain.Entities;
using Shouldly;

namespace InventoryManagement.UnitTests.Domain.Entities;

public sealed class WarehouseTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiateWarehouse()
    {
        // Arrange
        const string code = " wh-north ";
        const string name = " North Distribution Hub ";

        // Act
        var warehouse = new Warehouse(code, name);

        // Assert
        warehouse.Code.ShouldBe("WH-NORTH");
        warehouse.Name.ShouldBe("North Distribution Hub");
        warehouse.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCode_ShouldThrowArgumentException(string? invalidCode)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Warehouse(invalidCode!, "Main Hub"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Warehouse("WH-01", invalidName!));
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var warehouse = new Warehouse("WH-01", "Old Hub");

        // Act
        warehouse.UpdateName(" Modernized Hub ");

        // Assert
        warehouse.Name.ShouldBe("Modernized Hub");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var warehouse = new Warehouse("WH-01", "Valid Hub");

        // Act & Assert
        Should.Throw<ArgumentException>(() => warehouse.UpdateName(invalidName!));
    }
}
