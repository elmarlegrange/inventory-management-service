using InventoryManagement.Application.Orders;
using InventoryManagement.Application.Products;
using InventoryManagement.Application.Warehouses;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddSingleton<Auth.IPasswordHasher, Auth.PasswordHasher>();
        services.AddScoped<Auth.IJwtTokenGenerator, Auth.JwtTokenGenerator>();
        services.AddScoped<Auth.IAuthService, Auth.AuthService>();

        return services;
    }
}
