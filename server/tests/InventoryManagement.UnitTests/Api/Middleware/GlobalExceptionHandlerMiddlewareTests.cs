using System.Text.Json;
using InventoryManagement.Api.Middleware;
using InventoryManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace InventoryManagement.UnitTests.Api.Middleware;

public sealed class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationExceptionThrown_ShouldReturn400ProblemDetails()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new ValidationException("productCode", "Invalid product code format."),
            _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        context.Response.ContentType.ShouldBe("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problem.Title.ShouldBe("Validation Error");
    }

    [Fact]
    public async Task InvokeAsync_WhenInsufficientStockExceptionThrown_ShouldReturn400WithShortfallExtensions()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new InsufficientStockException("PROD-1", "WH-SRC", requiredQuantity: 10, availableQuantity: 4),
            _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        context.Response.ContentType.ShouldBe("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        responseBody.ShouldContain("Insufficient Stock");
        responseBody.ShouldContain("PROD-1");
        responseBody.ShouldContain("missingQuantity");
    }

    [Fact]
    public async Task InvokeAsync_WhenEntityNotFoundExceptionThrown_ShouldReturn404ProblemDetails()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new EntityNotFoundException("Product", "PROD-999"),
            _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        responseBody.ShouldContain("Resource Not Found");
        responseBody.ShouldContain("PROD-999");
    }

    [Fact]
    public async Task InvokeAsync_WhenDuplicateEntityExceptionThrown_ShouldReturn409ProblemDetails()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new DuplicateEntityException("Warehouse", "WH-01"),
            _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        responseBody.ShouldContain("Conflict - Duplicate Resource");
    }
}
