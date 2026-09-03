using InventoryManagement.Api.Controllers;
using InventoryManagement.Application.Orders;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace InventoryManagement.UnitTests.Api.Controllers;

public sealed class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _sut = new OrdersController(_orderServiceMock.Object);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnOkWithOrderDto()
    {
        // Arrange
        var request = new CreateOrderRequest("PROD-1", "WH-SRC", "WH-DST", 10);
        var orderDto = new OrderDto(Guid.NewGuid(), "PROD-1", "WH-SRC", "WH-DST", 10, DateTime.UtcNow);
        _orderServiceMock.Setup(s => s.CreateOrderAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var actionResult = await _sut.CreateOrder(request, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var value = okResult.Value.ShouldBeOfType<OrderDto>();
        value.ProductCode.ShouldBe("PROD-1");
        value.Quantity.ShouldBe(10);
    }
}
