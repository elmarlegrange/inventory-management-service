using System.Net;
using System.Net.Http.Json;
using InventoryManagement.Application.Products;
using InventoryManagement.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace InventoryManagement.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public sealed class ProductsApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateProduct_WhenValid_ShouldReturn201CreatedAndPersist()
    {
        // Arrange
        var request = new CreateProductRequest("PROD-01", "Mechanical Keyboard");

        // Act
        var response = await _client.PostAsJsonAsync("/products", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.ShouldNotBeNull();
        product.Code.ShouldBe("PROD-01");
        product.Name.ShouldBe("Mechanical Keyboard");

        // Verify retrieval
        var getResponse = await _client.GetAsync("/products/PROD-01");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateProduct_WhenDuplicateCode_ShouldReturn409Conflict()
    {
        // Arrange
        var request = new CreateProductRequest("PROD-DUP", "First Insert");
        await _client.PostAsJsonAsync("/products", request);

        // Act
        var duplicateResponse = await _client.PostAsJsonAsync("/products", new CreateProductRequest("PROD-DUP", "Second Insert"));

        // Assert
        duplicateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await duplicateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldNotBeNull();
        problem.Title.ShouldContain("Conflict");
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnProductList()
    {
        // Arrange
        await _client.PostAsJsonAsync("/products", new CreateProductRequest("P-A", "Item A"));
        await _client.PostAsJsonAsync("/products", new CreateProductRequest("P-B", "Item B"));

        // Act
        var response = await _client.GetAsync("/products");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.ShouldNotBeNull();
        products.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetProductByCode_WhenNotFound_ShouldReturn404NotFound()
    {
        // Act
        var response = await _client.GetAsync("/products/NON-EXISTENT");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldNotBeNull();
        problem.Title.ShouldContain("Resource Not Found");
    }
}
