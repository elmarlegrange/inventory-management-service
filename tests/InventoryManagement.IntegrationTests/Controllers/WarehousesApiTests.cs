using System.Net;
using System.Net.Http.Json;
using InventoryManagement.Application.Products;
using InventoryManagement.Application.Stock;
using InventoryManagement.Application.Warehouses;
using InventoryManagement.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace InventoryManagement.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public sealed class WarehousesApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WarehousesApiTests(CustomWebApplicationFactory factory)
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
    public async Task CreateWarehouse_WhenValid_ShouldReturn201CreatedAndPersist()
    {
        // Arrange
        var request = new CreateWarehouseRequest("WH-MAIN", "Central Distribution");

        // Act
        var response = await _client.PostAsJsonAsync("/warehouses", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var warehouse = await response.Content.ReadFromJsonAsync<WarehouseDto>();
        warehouse.ShouldNotBeNull();
        warehouse.Code.ShouldBe("WH-MAIN");
        warehouse.Name.ShouldBe("Central Distribution");
    }

    [Fact]
    public async Task CreateWarehouse_WhenDuplicateCode_ShouldReturn409Conflict()
    {
        // Arrange
        var request = new CreateWarehouseRequest("WH-DUP", "Original");
        await _client.PostAsJsonAsync("/warehouses", request);

        // Act
        var duplicateResponse = await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest("WH-DUP", "Duplicate"));

        // Assert
        duplicateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddStock_WhenValid_ShouldSetStockAndReturn200()
    {
        // Arrange: Create Product and Warehouse first
        await _client.PostAsJsonAsync("/products", new CreateProductRequest("P-STOCK", "Stocked Product"));
        await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest("W-STOCK", "Stocked Warehouse"));

        // Act
        var addStockResponse = await _client.PostAsJsonAsync("/warehouses/W-STOCK/stock", new AddStockItemRequest("P-STOCK", 75));

        // Assert
        addStockResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify stock in warehouse
        var getStockResponse = await _client.GetAsync("/warehouses/W-STOCK/stock");
        getStockResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var items = await getStockResponse.Content.ReadFromJsonAsync<List<WarehouseStockItemDto>>();
        items.ShouldNotBeNull();
        items.ShouldContain(i => i.ProductCode == "P-STOCK" && i.Quantity == 75);

        // Verify stock across warehouses for product
        var productStockResponse = await _client.GetAsync("/products/P-STOCK/stock");
        productStockResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var locations = await productStockResponse.Content.ReadFromJsonAsync<List<ProductStockLocationDto>>();
        locations.ShouldNotBeNull();
        locations.ShouldContain(l => l.WarehouseCode == "W-STOCK" && l.Quantity == 75);
    }

    [Fact]
    public async Task AddStock_WhenQuantityNonPositive_ShouldReturn400BadRequest()
    {
        // Arrange
        await _client.PostAsJsonAsync("/products", new CreateProductRequest("P-QTY", "Item"));
        await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest("W-QTY", "Hub"));

        // Act
        var response = await _client.PostAsJsonAsync("/warehouses/W-QTY/stock", new AddStockItemRequest("P-QTY", 0));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
