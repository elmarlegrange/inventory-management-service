using InventoryManagement.Application.Stock;
using InventoryManagement.Application.Warehouses;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;
using Moq;
using Shouldly;

namespace InventoryManagement.UnitTests.Application.Warehouses;

public sealed class WarehouseServiceTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IStockRepository> _stockRepoMock;
    private readonly WarehouseService _sut;

    public WarehouseServiceTests()
    {
        _warehouseRepoMock = new Mock<IWarehouseRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _stockRepoMock = new Mock<IStockRepository>();
        _sut = new WarehouseService(_warehouseRepoMock.Object, _productRepoMock.Object, _stockRepoMock.Object);
    }

    [Fact]
    public async Task GetAllWarehousesAsync_ShouldReturnAllWarehouseDtos()
    {
        // Arrange
        var warehouses = new List<Warehouse>
        {
            new("WH-1", "Main Hub"),
            new("WH-2", "Secondary Hub")
        };
        _warehouseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouses);

        // Act
        var result = await _sut.GetAllWarehousesAsync();

        // Assert
        result.Count.ShouldBe(2);
        result[0].Code.ShouldBe("WH-1");
        result[0].Name.ShouldBe("Main Hub");
    }

    [Fact]
    public async Task GetWarehouseByCodeAsync_WhenWarehouseExists_ShouldReturnWarehouseDto()
    {
        // Arrange
        var warehouse = new Warehouse("WH-1", "Main Hub");
        _warehouseRepoMock.Setup(r => r.GetByCodeAsync("WH-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        // Act
        var result = await _sut.GetWarehouseByCodeAsync("wh-1");

        // Assert
        result.ShouldNotBeNull();
        result.Code.ShouldBe("WH-1");
        result.Name.ShouldBe("Main Hub");
    }

    [Fact]
    public async Task GetWarehouseByCodeAsync_WhenWarehouseDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        _warehouseRepoMock.Setup(r => r.GetByCodeAsync("WH-404", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        // Act & Assert
        var ex = await Should.ThrowAsync<EntityNotFoundException>(() =>
            _sut.GetWarehouseByCodeAsync("WH-404"));

        ex.EntityName.ShouldBe("Warehouse");
        ex.EntityKey.ShouldBe("WH-404");
    }

    [Fact]
    public async Task CreateWarehouseAsync_WhenValid_ShouldCreateAndReturnWarehouseDto()
    {
        // Arrange
        var request = new CreateWarehouseRequest("WH-NEW", "New Facility");
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-NEW", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _warehouseRepoMock.Setup(r => r.CreateAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateWarehouseAsync(request);

        // Assert
        result.Code.ShouldBe("WH-NEW");
        result.Name.ShouldBe("New Facility");
        _warehouseRepoMock.Verify(r => r.CreateAsync(It.Is<Warehouse>(w => w.Code == "WH-NEW"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateWarehouseAsync_WhenDuplicateCode_ShouldThrowDuplicateEntityException()
    {
        // Arrange
        var request = new CreateWarehouseRequest("WH-DUP", "Duplicate");
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-DUP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = await Should.ThrowAsync<DuplicateEntityException>(() =>
            _sut.CreateWarehouseAsync(request));

        ex.EntityName.ShouldBe("Warehouse");
        ex.EntityKey.ShouldBe("WH-DUP");
    }

    [Fact]
    public async Task GetStockForWarehouseAsync_WhenWarehouseExists_ShouldReturnItems()
    {
        // Arrange
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var items = new List<WarehouseStockItem>
        {
            new("PROD-A", "Widget A", 50, DateTime.UtcNow),
            new("PROD-B", "Widget B", 30, DateTime.UtcNow)
        };
        _stockRepoMock.Setup(r => r.GetWarehouseStockDetailsAsync("WH-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        // Act
        var result = await _sut.GetStockForWarehouseAsync("wh-1");

        // Assert
        result.Count.ShouldBe(2);
        result[0].ProductCode.ShouldBe("PROD-A");
        result[0].ProductName.ShouldBe("Widget A");
        result[0].Quantity.ShouldBe(50);
    }

    [Fact]
    public async Task AddStockToWarehouseAsync_WhenValid_ShouldCallUpsert()
    {
        // Arrange
        const string warehouseCode = "wh-a";
        var request = new AddStockItemRequest("prod-1", 100);

        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-A", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _stockRepoMock.Setup(r => r.UpsertStockAsync("WH-A", "PROD-1", 100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.AddStockToWarehouseAsync(warehouseCode, request);

        // Assert
        _stockRepoMock.Verify(r => r.UpsertStockAsync("WH-A", "PROD-1", 100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddStockToWarehouseAsync_WhenWarehouseNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var request = new AddStockItemRequest("PROD-1", 10);
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-404", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _sut.AddStockToWarehouseAsync("WH-404", request));
    }

    [Fact]
    public async Task AddStockToWarehouseAsync_WhenProductNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var request = new AddStockItemRequest("PROD-404", 10);
        _warehouseRepoMock.Setup(r => r.ExistsAsync("WH-A", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-404", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _sut.AddStockToWarehouseAsync("WH-A", request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task AddStockToWarehouseAsync_WhenQuantityNonPositive_ShouldThrowValidationException(int invalidQty)
    {
        // Arrange
        var request = new AddStockItemRequest("PROD-1", invalidQty);

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() =>
            _sut.AddStockToWarehouseAsync("WH-A", request));
    }
}
