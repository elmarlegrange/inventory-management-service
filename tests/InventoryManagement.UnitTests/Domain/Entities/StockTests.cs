using InventoryManagement.Domain.Entities;
using Shouldly;

namespace InventoryManagement.UnitTests.Domain.Entities;

public sealed class StockTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiateStock()
    {
        // Arrange
        const string warehouseCode = " wh-a ";
        const string productCode = " prod-1 ";
        const int quantity = 42;

        // Act
        var stock = new Stock(warehouseCode, productCode, quantity);

        // Assert
        stock.WarehouseCode.ShouldBe("WH-A");
        stock.ProductCode.ShouldBe("PROD-1");
        stock.Quantity.ShouldBe(42);
        stock.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidWarehouseCode_ShouldThrowArgumentException(string? invalidWarehouseCode)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Stock(invalidWarehouseCode!, "PROD-1", 10));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidProductCode_ShouldThrowArgumentException(string? invalidProductCode)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Stock("WH-A", invalidProductCode!, 10));
    }

    [Fact]
    public void Constructor_WithNegativeQuantity_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => new Stock("WH-A", "PROD-1", -1));
    }

    [Fact]
    public void SetQuantity_WithValidQuantity_ShouldUpdateQuantityAndTimestamp()
    {
        // Arrange
        var stock = new Stock("WH-A", "PROD-1", 10);

        // Act
        stock.SetQuantity(25);

        // Assert
        stock.Quantity.ShouldBe(25);
        stock.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void SetQuantity_WithNegativeQuantity_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var stock = new Stock("WH-A", "PROD-1", 10);

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => stock.SetQuantity(-5));
    }
}
