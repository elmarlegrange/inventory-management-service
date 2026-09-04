using System.Net;
using System.Net.Http.Json;
using InventoryManagement.Application.Auth;
using InventoryManagement.Application.Warehouses;
using InventoryManagement.IntegrationTests.Fixtures;
using Shouldly;

namespace InventoryManagement.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public sealed class AuthApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _anonymousClient;
    private readonly HttpClient _userClient;
    private readonly HttpClient _adminClient;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateAnonymousClient();
        _userClient = factory.CreateUserClient();
        _adminClient = factory.CreateAdminClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_WithValidAdminCredentials_ShouldReturn200AndToken()
    {
        // Act
        var response = await _anonymousClient.PostAsJsonAsync("/auth/login", new LoginRequest("admin", "Admin123!"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrWhiteSpace();
        result.Username.ShouldBe("admin");
        result.Role.ShouldBe("Admin");
    }

    [Fact]
    public async Task Login_WithValidUserCredentials_ShouldReturn200AndToken()
    {
        // Act
        var response = await _anonymousClient.PostAsJsonAsync("/auth/login", new LoginRequest("user", "User123!"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrWhiteSpace();
        result.Username.ShouldBe("user");
        result.Role.ShouldBe("User");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401Unauthorized()
    {
        // Act
        var response = await _anonymousClient.PostAsJsonAsync("/auth/login", new LoginRequest("admin", "WrongPassword!"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWarehouses_WhenUnauthenticated_ShouldReturn401Unauthorized()
    {
        // Act
        var response = await _anonymousClient.GetAsync("/warehouses");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateWarehouse_WithUserRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var request = new CreateWarehouseRequest("WH-FORBIDDEN", "Forbidden Facility");

        // Act
        var response = await _userClient.PostAsJsonAsync("/warehouses", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateWarehouse_WithAdminRole_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateWarehouseRequest("WH-ALLOWED", "Allowed Facility");

        // Act
        var response = await _adminClient.PostAsJsonAsync("/warehouses", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }
}
