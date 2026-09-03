using InventoryManagement.Api.Controllers;
using InventoryManagement.Application.Stock;
using InventoryManagement.Application.Warehouses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace InventoryManagement.UnitTests.Api.Controllers;

public sealed class WarehousesControllerTests
{
    private readonly Mock<IWarehouseService> _warehouseServiceMock;
    private readonly WarehousesController _sut;

    public WarehousesControllerTests()
    {
        _warehouseServiceMock = new Mock<IWarehouseService>();
        _sut = new WarehousesController(_warehouseServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithWarehouses()
    {
        // Arrange
        var warehouses = new List<WarehouseDto> { new("WH-1", "Warehouse 1", DateTime.UtcNow) };
        _warehouseServiceMock.Setup(s => s.GetAllWarehousesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouses);

        // Act
        var actionResult = await _sut.GetAll(CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var value = okResult.Value.ShouldBeAssignableTo<IReadOnlyList<WarehouseDto>>();
        value.ShouldNotBeNull();
        value.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByCode_ShouldReturnOkWithWarehouse()
    {
        // Arrange
        var warehouse = new WarehouseDto("WH-1", "Warehouse 1", DateTime.UtcNow);
        _warehouseServiceMock.Setup(s => s.GetWarehouseByCodeAsync("WH-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        // Act
        var actionResult = await _sut.GetByCode("WH-1", CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var value = okResult.Value.ShouldBeOfType<WarehouseDto>();
        value.Code.ShouldBe("WH-1");
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateWarehouseRequest("WH-NEW", "New Warehouse");
        var created = new WarehouseDto("WH-NEW", "New Warehouse", DateTime.UtcNow);
        _warehouseServiceMock.Setup(s => s.CreateWarehouseAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var actionResult = await _sut.Create(request, CancellationToken.None);

        // Assert
        var createdAtResult = actionResult.Result.ShouldBeOfType<CreatedAtActionResult>();
        createdAtResult.ActionName.ShouldBe(nameof(WarehousesController.GetByCode));
        var value = createdAtResult.Value.ShouldBeOfType<WarehouseDto>();
        value.Code.ShouldBe("WH-NEW");
    }

    [Fact]
    public async Task GetStock_ShouldReturnOkWithItems()
    {
        // Arrange
        var items = new List<WarehouseStockItemDto>
        {
            new("PROD-1", "Product 1", 50, DateTime.UtcNow)
        };
        _warehouseServiceMock.Setup(s => s.GetStockForWarehouseAsync("WH-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        // Act
        var actionResult = await _sut.GetStock("WH-1", CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var value = okResult.Value.ShouldBeAssignableTo<IReadOnlyList<WarehouseStockItemDto>>();
        value.ShouldNotBeNull();
        value.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AddStock_ShouldReturnOk()
    {
        // Arrange
        var request = new AddStockItemRequest("PROD-1", 20);
        _warehouseServiceMock.Setup(s => s.AddStockToWarehouseAsync("WH-1", request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddStock("WH-1", request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkResult>();
    }
}
