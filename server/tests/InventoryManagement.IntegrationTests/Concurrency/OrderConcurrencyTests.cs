using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using InventoryManagement.Application.Orders;
using InventoryManagement.Application.Products;
using InventoryManagement.Application.Stock;
using InventoryManagement.Application.Warehouses;
using InventoryManagement.IntegrationTests.Fixtures;
using Shouldly;

namespace InventoryManagement.IntegrationTests.Concurrency;

[Collection("IntegrationTests")]
public sealed class OrderConcurrencyTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrderConcurrencyTests(CustomWebApplicationFactory factory)
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
    public async Task ParallelOrders_With20ConcurrentThreads_ShouldPreventRaceConditionsAndMaintainInvariant()
    {
        // -------------------------------------------------------------
        // ARRANGE:
        // Product P-RACE with exactly 10 units at WH-A, 0 units at WH-B.
        // -------------------------------------------------------------
        const string productCode = "P-RACE";
        const string sourceWarehouse = "WH-RACE-A";
        const string destWarehouse = "WH-RACE-B";
        const int initialStock = 10;
        const int concurrentRequests = 20;

        await _client.PostAsJsonAsync("/products", new CreateProductRequest(productCode, "Race Condition Test Item"));
        await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest(sourceWarehouse, "Source Warehouse A"));
        await _client.PostAsJsonAsync("/warehouses", new CreateWarehouseRequest(destWarehouse, "Destination Warehouse B"));
        await _client.PostAsJsonAsync($"/warehouses/{sourceWarehouse}/stock", new AddStockItemRequest(productCode, initialStock));

        // -------------------------------------------------------------
        // ACT:
        // Launch 20 concurrent threads trying to transfer 1 unit each.
        // -------------------------------------------------------------
        var responses = new ConcurrentBag<HttpResponseMessage>();

        var tasks = Enumerable.Range(0, concurrentRequests).Select(async _ =>
        {
            var request = new CreateOrderRequest(productCode, sourceWarehouse, destWarehouse, 1);
            var response = await _client.PostAsJsonAsync("/orders", request);
            responses.Add(response);
        });

        await Task.WhenAll(tasks);

        // -------------------------------------------------------------
        // ASSERT:
        // Exactly 10 requests MUST succeed (200 OK)
        // Exactly 10 requests MUST fail with 400 Bad Request (Insufficient Stock)
        // -------------------------------------------------------------
        var allResponses = responses.ToList();
        var successCount = allResponses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var failureCount = allResponses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

        successCount.ShouldBe(10, $"Expected exactly 10 successful transfers, but got {successCount}.");
        failureCount.ShouldBe(10, $"Expected exactly 10 rejected transfers due to insufficient stock, but got {failureCount}.");

        // -------------------------------------------------------------
        // FINAL DATABASE INVARIANT VERIFICATION:
        // WH-A must have exactly 0 units (never negative, non-negative CHECK preserved).
        // WH-B must have exactly 10 units.
        // -------------------------------------------------------------
        var srcStockResponse = await _client.GetAsync($"/warehouses/{sourceWarehouse}/stock");
        var srcItems = await srcStockResponse.Content.ReadFromJsonAsync<List<WarehouseStockItemDto>>();
        srcItems.ShouldNotBeNull();
        srcItems.Single(i => i.ProductCode == productCode).Quantity.ShouldBe(0);

        var dstStockResponse = await _client.GetAsync($"/warehouses/{destWarehouse}/stock");
        var dstItems = await dstStockResponse.Content.ReadFromJsonAsync<List<WarehouseStockItemDto>>();
        dstItems.ShouldNotBeNull();
        dstItems.Single(i => i.ProductCode == productCode).Quantity.ShouldBe(10);
    }
}
