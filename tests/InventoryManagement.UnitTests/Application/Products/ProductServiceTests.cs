using InventoryManagement.Application.Products;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Interfaces;
using Moq;
using Shouldly;

namespace InventoryManagement.UnitTests.Application.Products;

public sealed class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IStockRepository> _stockRepoMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _stockRepoMock = new Mock<IStockRepository>();
        _sut = new ProductService(_productRepoMock.Object, _stockRepoMock.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProductDtos()
    {
        // Arrange
        var products = new List<Product>
        {
            new("PROD-1", "Product One"),
            new("PROD-2", "Product Two")
        };
        _productRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.Count.ShouldBe(2);
        result[0].Code.ShouldBe("PROD-1");
        result[0].Name.ShouldBe("Product One");
        result[1].Code.ShouldBe("PROD-2");
        result[1].Name.ShouldBe("Product Two");
    }

    [Fact]
    public async Task GetProductByCodeAsync_WhenProductExists_ShouldReturnProductDto()
    {
        // Arrange
        var product = new Product("PROD-1", "Product One");
        _productRepoMock.Setup(r => r.GetByCodeAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductByCodeAsync("prod-1");

        // Assert
        result.ShouldNotBeNull();
        result.Code.ShouldBe("PROD-1");
        result.Name.ShouldBe("Product One");
    }

    [Fact]
    public async Task GetProductByCodeAsync_WhenProductDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByCodeAsync("NON-EXISTENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var ex = await Should.ThrowAsync<EntityNotFoundException>(() =>
            _sut.GetProductByCodeAsync("NON-EXISTENT"));

        ex.EntityName.ShouldBe("Product");
        ex.EntityKey.ShouldBe("NON-EXISTENT");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetProductByCodeAsync_WhenCodeIsInvalid_ShouldThrowValidationException(string? invalidCode)
    {
        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() =>
            _sut.GetProductByCodeAsync(invalidCode!));
    }

    [Fact]
    public async Task CreateProductAsync_WhenValid_ShouldCreateAndReturnProductDto()
    {
        // Arrange
        var request = new CreateProductRequest("PROD-NEW", "Brand New Product");
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-NEW", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _productRepoMock.Setup(r => r.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateProductAsync(request);

        // Assert
        result.Code.ShouldBe("PROD-NEW");
        result.Name.ShouldBe("Brand New Product");
        _productRepoMock.Verify(r => r.CreateAsync(It.Is<Product>(p => p.Code == "PROD-NEW"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WhenDuplicateCode_ShouldThrowDuplicateEntityException()
    {
        // Arrange
        var request = new CreateProductRequest("PROD-DUP", "Duplicate");
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-DUP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = await Should.ThrowAsync<DuplicateEntityException>(() =>
            _sut.CreateProductAsync(request));

        ex.EntityName.ShouldBe("Product");
        ex.EntityKey.ShouldBe("PROD-DUP");
    }

    [Theory]
    [InlineData("", "Valid Name")]
    [InlineData("ValidCode", "")]
    [InlineData("VeryLongCodeExceedingFiftyCharactersWhichShouldTriggerValidationException", "Valid Name")]
    public async Task CreateProductAsync_WhenInvalidInputs_ShouldThrowValidationException(string code, string name)
    {
        // Arrange
        var request = new CreateProductRequest(code, name);

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => _sut.CreateProductAsync(request));
    }

    [Fact]
    public async Task GetStockForProductAsync_WhenProductExists_ShouldReturnLocations()
    {
        // Arrange
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var locations = new List<ProductStockLocation>
        {
            new("WH-A", "North Hub", 10, DateTime.UtcNow),
            new("WH-B", "South Hub", 25, DateTime.UtcNow)
        };
        _stockRepoMock.Setup(r => r.GetProductStockDetailsAsync("PROD-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        // Act
        var result = await _sut.GetStockForProductAsync("prod-1");

        // Assert
        result.Count.ShouldBe(2);
        result[0].WarehouseCode.ShouldBe("WH-A");
        result[0].Quantity.ShouldBe(10);
        result[1].WarehouseCode.ShouldBe("WH-B");
        result[1].Quantity.ShouldBe(25);
    }

    [Fact]
    public async Task GetStockForProductAsync_WhenProductDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        _productRepoMock.Setup(r => r.ExistsAsync("PROD-404", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _sut.GetStockForProductAsync("prod-404"));
    }
}
