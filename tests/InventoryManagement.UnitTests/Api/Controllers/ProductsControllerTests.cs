using InventoryManagement.Api.Controllers;
using InventoryManagement.Application.Products;
using InventoryManagement.Application.Stock;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace InventoryManagement.UnitTests.Api.Controllers;

public sealed class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly ProductsController _sut;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _sut = new ProductsController(_productServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithProducts()
    {
        // Arrange
        var products = new List<ProductDto> { new("PROD-1", "Product 1", DateTime.UtcNow) };
        _productServiceMock.Setup(s => s.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var actionResult = await _sut.GetAll(CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var value = okResult.Value.ShouldBeAssignableTo<IReadOnlyList<ProductDto>>();
        value.ShouldNotBeNull();
        value.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByCode_ShouldReturnOkWithProduct()
    {
        // Arrange
        var product = new ProductDto("PROD-1", "Product 1", DateTime.UtcNow);
        _productServiceMock.Setup(s => s.GetProductByCodeAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var actionResult = await _sut.GetByCode("PROD-1", CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var value = okResult.Value.ShouldBeOfType<ProductDto>();
        value.Code.ShouldBe("PROD-1");
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateProductRequest("PROD-NEW", "New Product");
        var created = new ProductDto("PROD-NEW", "New Product", DateTime.UtcNow);
        _productServiceMock.Setup(s => s.CreateProductAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var actionResult = await _sut.Create(request, CancellationToken.None);

        // Assert
        var createdAtResult = actionResult.Result.ShouldBeOfType<CreatedAtActionResult>();
        createdAtResult.ActionName.ShouldBe(nameof(ProductsController.GetByCode));
        var value = createdAtResult.Value.ShouldBeOfType<ProductDto>();
        value.Code.ShouldBe("PROD-NEW");
    }

    [Fact]
    public async Task GetStock_ShouldReturnOkWithLocations()
    {
        // Arrange
        var locations = new List<ProductStockLocationDto>
        {
            new("WH-A", "Warehouse A", 10, DateTime.UtcNow)
        };
        _productServiceMock.Setup(s => s.GetStockForProductAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        // Act
        var actionResult = await _sut.GetStock("PROD-1", CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var value = okResult.Value.ShouldBeAssignableTo<IReadOnlyList<ProductStockLocationDto>>();
        value.ShouldNotBeNull();
        value.Count.ShouldBe(1);
    }
}
