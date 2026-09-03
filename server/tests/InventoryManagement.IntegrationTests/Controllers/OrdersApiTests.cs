using System.Net;
using System.Net.Http.Json;
using InventoryManagement.Application.Orders;
using InventoryManagement.Application.Products;
using InventoryManagement.Application.Stock;
using InventoryManagement.Application.Warehouses;
using InventoryManagement.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace InventoryManagement.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public sealed class OrdersApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrdersApiTests(CustomWebApplicationFactory factory)
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
    public async Task CreateOrder_WhenValid_ShouldTransferStockAtomically()
    {
        // Arrange: Setup Product and 2 Warehouses with 50 units in Source
        await _client.PostAsJsonAsync("/products", new CreateProductRequest("PROD-ORD", "Order Product"));
        await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest("WH-SRC", "Source Warehouse"));
        await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest("WH-DST", "Destination Warehouse"));
        await _client.PostAsJsonAsync("/warehouses/WH-SRC/stock", new AddStockItemRequest("PROD-ORD", 50));

        // Act: Transfer 20 units
        var orderRequest = new CreateOrderRequest("PROD-ORD", "WH-SRC", "WH-DST", 20);
        var orderResponse = await _client.PostAsJsonAsync("/orders", orderRequest);

        // Assert: Order created
        orderResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDto>();
        order.ShouldNotBeNull();
        order.ProductCode.ShouldBe("PROD-ORD");
        order.SourceWarehouseCode.ShouldBe("WH-SRC");
        order.DestinationWarehouseCode.ShouldBe("WH-DST");
        order.Quantity.ShouldBe(20);

        // Verify Source stock decremented (50 -> 30)
        var srcStockResponse = await _client.GetAsync("/warehouses/WH-SRC/stock");
        var srcItems = await srcStockResponse.Content.ReadFromJsonAsync<List<WarehouseStockItemDto>>();
        srcItems.ShouldNotBeNull();
        srcItems.Single(i => i.ProductCode == "PROD-ORD").Quantity.ShouldBe(30);

        // Verify Destination stock incremented (0 -> 20)
        var dstStockResponse = await _client.GetAsync("/warehouses/WH-DST/stock");
        var dstItems = await dstStockResponse.Content.ReadFromJsonAsync<List<WarehouseStockItemDto>>();
        dstItems.ShouldNotBeNull();
        dstItems.Single(i => i.ProductCode == "PROD-ORD").Quantity.ShouldBe(20);
    }

    [Fact]
    public async Task CreateOrder_WhenInsufficientStock_ShouldReturn400WithShortfallProblemDetails()
    {
        // Arrange: Only 5 units available in Source
        await _client.PostAsJsonAsync("/products", new CreateProductRequest("PROD-DEF", "Deficit Product"));
        await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest("WH-S1", "Source 1"));
        await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest("WH-D1", "Dest 1"));
        await _client.PostAsJsonAsync("/warehouses/WH-S1/stock", new AddStockItemRequest("PROD-DEF", 5));

        // Act: Attempt to transfer 15 units (shortfall = 10)
        var orderRequest = new CreateOrderRequest("PROD-DEF", "WH-S1", "WH-D1", 15);
        var orderResponse = await _client.PostAsJsonAsync("/orders", orderRequest);

        // Assert
        orderResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var responseContent = await orderResponse.Content.ReadAsStringAsync();
        responseContent.ShouldContain("Insufficient Stock");
        responseContent.ShouldContain("missingQuantity");
        responseContent.ShouldContain("10");

        // Verify source stock was NOT decremented (still 5)
        var srcStockResponse = await _client.GetAsync("/warehouses/WH-S1/stock");
        var srcItems = await srcStockResponse.Content.ReadFromJsonAsync<List<WarehouseStockItemDto>>();
        srcItems.ShouldNotBeNull();
        srcItems.Single(i => i.ProductCode == "PROD-DEF").Quantity.ShouldBe(5);
    }

    [Fact]
    public async Task CreateOrder_WhenSourceAndDestinationAreIdentical_ShouldReturn400BadRequest()
    {
        // Arrange
        await _client.PostAsJsonAsync("/products", new CreateProductRequest("PROD-SAME", "Product"));
        await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest("WH-SAME", "Warehouse"));

        // Act
        var orderResponse = await _client.PostAsJsonAsync("/orders", new CreateOrderRequest("PROD-SAME", "WH-SAME", "WH-SAME", 5));

        // Assert
        orderResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
