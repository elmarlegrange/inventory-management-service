using InventoryManagement.Domain.Exceptions;
using Shouldly;

namespace InventoryManagement.UnitTests.Domain.Exceptions;

public sealed class DomainExceptionTests
{
    [Fact]
    public void InsufficientStockException_ShouldCalculateShortfallCorrectly()
    {
        // Arrange & Act
        var exception = new InsufficientStockException("PROD-99", "WH-NORTH", requiredQuantity: 100, availableQuantity: 35);

        // Assert
        exception.ProductCode.ShouldBe("PROD-99");
        exception.WarehouseCode.ShouldBe("WH-NORTH");
        exception.RequiredQuantity.ShouldBe(100);
        exception.AvailableQuantity.ShouldBe(35);
        exception.MissingQuantity.ShouldBe(65);
        exception.Message.ShouldContain("required 100, but only 35 available (missing 65)");
    }

    [Fact]
    public void EntityNotFoundException_ShouldSetPropertiesAndMessage()
    {
        // Arrange & Act
        var exception = new EntityNotFoundException("Product", "PROD-404");

        // Assert
        exception.EntityName.ShouldBe("Product");
        exception.EntityKey.ShouldBe("PROD-404");
        exception.Message.ShouldBe("Product with key 'PROD-404' was not found.");
    }

    [Fact]
    public void DuplicateEntityException_ShouldSetPropertiesAndMessage()
    {
        // Arrange & Act
        var exception = new DuplicateEntityException("Warehouse", "WH-01");

        // Assert
        exception.EntityName.ShouldBe("Warehouse");
        exception.EntityKey.ShouldBe("WH-01");
        exception.Message.ShouldBe("Warehouse with key 'WH-01' already exists.");
    }

    [Fact]
    public void ValidationException_WithFieldAndError_ShouldPopulateErrorsDictionary()
    {
        // Arrange & Act
        var exception = new ValidationException("quantity", "Must be positive.");

        // Assert
        exception.Errors.ContainsKey("quantity").ShouldBeTrue();
        exception.Errors["quantity"].ShouldContain("Must be positive.");
    }

    [Fact]
    public void InvalidOrderException_ShouldSetMessage()
    {
        // Arrange & Act
        var exception = new InvalidOrderException("Invalid transfer routing.");

        // Assert
        exception.Message.ShouldBe("Invalid transfer routing.");
    }
}
