using InventoryManagement.Domain.Entities;
using Shouldly;

namespace InventoryManagement.UnitTests.Domain.Entities;

public sealed class OrderTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiateOrder()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string productCode = " prod-1 ";
        const string source = " wh-a ";
        const string dest = " wh-b ";
        const int quantity = 15;

        // Act
        var order = new Order(id, productCode, source, dest, quantity);

        // Assert
        order.Id.ShouldBe(id);
        order.ProductCode.ShouldBe("PROD-1");
        order.SourceWarehouseCode.ShouldBe("WH-A");
        order.DestinationWarehouseCode.ShouldBe("WH-B");
        order.Quantity.ShouldBe(15);
        order.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldGenerateNewGuid()
    {
        // Act
        var order = new Order(Guid.Empty, "PROD-1", "WH-A", "WH-B", 5);

        // Assert
        order.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithNonPositiveQuantity_ShouldThrowArgumentOutOfRangeException(int invalidQuantity)
    {
        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new Order(Guid.NewGuid(), "PROD-1", "WH-A", "WH-B", invalidQuantity));
    }

    [Fact]
    public void Constructor_WithIdenticalSourceAndDestination_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            new Order(Guid.NewGuid(), "PROD-1", "WH-A", " wh-a ", 10));

        ex.Message.ShouldContain("cannot be identical");
    }

    [Theory]
    [InlineData(null, "WH-A", "WH-B")]
    [InlineData("PROD-1", null, "WH-B")]
    [InlineData("PROD-1", "WH-A", null)]
    [InlineData("   ", "WH-A", "WH-B")]
    public void Constructor_WithNullOrWhitespaceCodes_ShouldThrowArgumentException(
        string? productCode,
        string? sourceCode,
        string? destCode)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            new Order(Guid.NewGuid(), productCode!, sourceCode!, destCode!, 10));
    }
}
