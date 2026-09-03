using InventoryManagement.Domain.Entities;
using Shouldly;

namespace InventoryManagement.UnitTests.Domain.Entities;

public sealed class ProductTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiateProduct()
    {
        // Arrange
        const string code = " prod-001 ";
        const string name = " Widget Pro ";

        // Act
        var product = new Product(code, name);

        // Assert
        product.Code.ShouldBe("PROD-001");
        product.Name.ShouldBe("Widget Pro");
        product.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCode_ShouldThrowArgumentException(string? invalidCode)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Product(invalidCode!, "Widget"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Product("PROD-001", invalidName!));
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var product = new Product("PROD-001", "Old Name");

        // Act
        product.UpdateName(" New Name ");

        // Assert
        product.Name.ShouldBe("New Name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var product = new Product("PROD-001", "Valid Name");

        // Act & Assert
        Should.Throw<ArgumentException>(() => product.UpdateName(invalidName!));
    }
}
