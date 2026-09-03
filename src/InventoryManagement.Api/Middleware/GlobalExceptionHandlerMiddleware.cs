using System.Net;
using System.Text.Json;
using InventoryManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace InventoryManagement.Api.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate _next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        this._next = _next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = MapExceptionToProblemDetails(context, exception);

        _logger.LogWarning(exception, "Handled exception mapped to HTTP {StatusCode}: {Title} - {Detail}",
            statusCode, problemDetails.Title, problemDetails.Detail);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }

    private static (int StatusCode, ProblemDetails ProblemDetails) MapExceptionToProblemDetails(
        HttpContext context,
        Exception exception)
    {
        var instance = context.Request.Path.Value;

        switch (exception)
        {
            case ValidationException validationEx:
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = validationEx.Message,
                    Instance = instance,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };
                problem.Extensions["errors"] = validationEx.Errors;
                return (StatusCodes.Status400BadRequest, problem);
            }

            case InsufficientStockException stockEx:
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Insufficient Stock",
                    Detail = stockEx.Message,
                    Instance = instance,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };
                problem.Extensions["productCode"] = stockEx.ProductCode;
                problem.Extensions["warehouseCode"] = stockEx.WarehouseCode;
                problem.Extensions["requiredQuantity"] = stockEx.RequiredQuantity;
                problem.Extensions["availableQuantity"] = stockEx.AvailableQuantity;
                problem.Extensions["missingQuantity"] = stockEx.MissingQuantity;
                return (StatusCodes.Status400BadRequest, problem);
            }

            case InvalidOrderException invalidOrderEx:
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid Order Request",
                    Detail = invalidOrderEx.Message,
                    Instance = instance,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };
                return (StatusCodes.Status400BadRequest, problem);
            }

            case DuplicateEntityException duplicateEx:
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict - Duplicate Resource",
                    Detail = duplicateEx.Message,
                    Instance = instance,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
                };
                problem.Extensions["entityName"] = duplicateEx.EntityName;
                problem.Extensions["entityKey"] = duplicateEx.EntityKey;
                return (StatusCodes.Status409Conflict, problem);
            }

            case EntityNotFoundException notFoundEx:
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource Not Found",
                    Detail = notFoundEx.Message,
                    Instance = instance,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                };
                problem.Extensions["entityName"] = notFoundEx.EntityName;
                problem.Extensions["entityKey"] = notFoundEx.EntityKey;
                return (StatusCodes.Status404NotFound, problem);
            }

            case PostgresException pgEx when pgEx.SqlState == PostgresErrorCodes.UniqueViolation:
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict - Unique Constraint Violation",
                    Detail = pgEx.MessageText,
                    Instance = instance,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
                };
                return (StatusCodes.Status409Conflict, problem);
            }

            case PostgresException pgEx when pgEx.SqlState == PostgresErrorCodes.CheckViolation:
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request - Check Constraint Violation",
                    Detail = "Database constraint violation: quantity cannot be negative or violate invariants.",
                    Instance = instance,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };
                return (StatusCodes.Status400BadRequest, problem);
            }

            case ArgumentException argEx:
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = argEx.Message,
                    Instance = instance,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };
                return (StatusCodes.Status400BadRequest, problem);
            }

            default:
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while processing your request.",
                    Instance = instance,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return (StatusCodes.Status500InternalServerError, problem);
            }
        }
    }
}
