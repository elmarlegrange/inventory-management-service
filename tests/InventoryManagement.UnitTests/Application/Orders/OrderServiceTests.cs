using InventoryManagement.Application.Orders;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;
using Moq;
using Shouldly;

namespace InventoryManagement.UnitTests.Application.Orders;

public sealed class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _warehouseRepoMock = new Mock<IWarehouseRepository>();
        _sut = new OrderService(_orderRepoMock.Object, _productRepoMock.Object, _warehouseRepoMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValid_ShouldCreateAndReturnOrderDto()
    {
        // Arrange
        var request = new CreateOrderRequest("PROD-1", "WH-SRC", "WH-DST", 10);
        var orderId = Guid.NewGuid();
        var createdOrder = new Order(orderId, "PROD-1", "WH-SRC", "WH-DST", 10, DateTime.UtcNow);

        _productRepoMock.Setup(r => r.ExistsAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-SRC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-DST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _orderRepoMock.Setup(r => r.CreateOrderAsync("PROD-1", "WH-SRC", "WH-DST", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        result.Id.ShouldBe(orderId);
        result.ProductCode.ShouldBe("PROD-1");
        result.SourceWarehouseCode.ShouldBe("WH-SRC");
        result.DestinationWarehouseCode.ShouldBe("WH-DST");
        result.Quantity.ShouldBe(10);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenSourceAndDestinationAreIdentical_ShouldThrowInvalidOrderException()
    {
        // Arrange
        var request = new CreateOrderRequest("PROD-1", "WH-SAME", " wh-same ", 5);

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOrderException>(() =>
            _sut.CreateOrderAsync(request));

        ex.Message.ShouldContain("cannot be identical");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task CreateOrderAsync_WhenQuantityNonPositive_ShouldThrowValidationException(int invalidQty)
    {
        // Arrange
        var request = new CreateOrderRequest("PROD-1", "WH-SRC", "WH-DST", invalidQty);

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() =>
            _sut.CreateOrderAsync(request));
    }

    [Fact]
    public async Task CreateOrderAsync_WhenProductDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var request = new CreateOrderRequest("PROD-404", "WH-SRC", "WH-DST", 5);
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-404", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var ex = await Should.ThrowAsync<EntityNotFoundException>(() =>
            _sut.CreateOrderAsync(request));

        ex.EntityName.ShouldBe("Product");
        ex.EntityKey.ShouldBe("PROD-404");
    }

    [Fact]
    public async Task CreateOrderAsync_WhenSourceWarehouseDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var request = new CreateOrderRequest("PROD-1", "WH-404", "WH-DST", 5);
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-404", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var ex = await Should.ThrowAsync<EntityNotFoundException>(() =>
            _sut.CreateOrderAsync(request));

        ex.EntityName.ShouldBe("Warehouse");
        ex.EntityKey.ShouldBe("WH-404");
    }

    [Fact]
    public async Task CreateOrderAsync_WhenDestinationWarehouseDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var request = new CreateOrderRequest("PROD-1", "WH-SRC", "WH-404", 5);
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-SRC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-404", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var ex = await Should.ThrowAsync<EntityNotFoundException>(() =>
            _sut.CreateOrderAsync(request));

        ex.EntityName.ShouldBe("Warehouse");
        ex.EntityKey.ShouldBe("WH-404");
    }

    [Fact]
    public async Task CreateOrderAsync_WhenStockIsInsufficient_ShouldPropagateInsufficientStockException()
    {
        // Arrange
        var request = new CreateOrderRequest("PROD-1", "WH-SRC", "WH-DST", 50);
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-SRC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-DST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _orderRepoMock.Setup(r => r.CreateOrderAsync("PROD-1", "WH-SRC", "WH-DST", 50, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InsufficientStockException("PROD-1", "WH-SRC", requiredQuantity: 50, availableQuantity: 12));

        // Act & Assert
        var ex = await Should.ThrowAsync<InsufficientStockException>(() =>
            _sut.CreateOrderAsync(request));

        ex.RequiredQuantity.ShouldBe(50);
        ex.AvailableQuantity.ShouldBe(12);
        ex.MissingQuantity.ShouldBe(38);
    }
}
