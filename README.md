# Inventory Management Service

[![CI Pipeline](https://github.com/elmarlegrange/inventory-management-service/actions/workflows/ci.yml/badge.svg)](https://github.com/elmarlegrange/inventory-management-service/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Latest-336791?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?logo=docker)

A high-performance, containerized .NET 10 REST API for multi-warehouse inventory management, real-time stock tracking, and race-condition-free stock transfers built with Clean Architecture, Dapper, and PostgreSQL.

---

## Features

- **Multi-Warehouse Catalog**: Register and manage unique products and regional warehouse facilities.
- **Granular Inventory Tracking**: Inspect stock distribution per product across all warehouses or per warehouse for all stocked items.
- **Atomic Stock Transfers (Orders)**: Transactional stock movements using PostgreSQL pessimistic row-locking (`SELECT ... FOR UPDATE`) to eliminate race conditions and enforce non-negative stock invariants (`CONSTRAINT chk_stock_quantity_non_negative CHECK (quantity >= 0)`).
- **RFC 7807 Error Handling**: Standardized `application/problem+json` error payloads with detailed shortfall breakdowns when stock is insufficient.
- **Automated Test Suite**: 100+ unit and integration tests powered by `xUnit`, `Shouldly`, `Moq`, and `Testcontainers.PostgreSql`.

---

## Tech Stack

| Layer / Concern | Technology |
| :--- | :--- |
| **Framework & Language** | .NET 10 / C# 14 |
| **Architecture Pattern** | Clean Architecture (Domain-Driven Design) |
| **Data Access** | Dapper (Raw SQL) |
| **Database** | PostgreSQL (`latest`) |
| **API Documentation** | Swagger / OpenAPI |
| **Unit Testing** | xUnit, Shouldly, Moq |
| **Integration Testing** | Testcontainers.PostgreSql, Microsoft.AspNetCore.Mvc.Testing |
| **Containerization & CI** | Docker, Docker Compose, GitHub Actions |

---

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Option 1: Running via Docker Compose (Recommended)

Start the PostgreSQL database and API service with a single command:

```bash
docker compose up --build -d
```

- **Swagger UI**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **API Base URL**: `http://localhost:8080`

To stop and remove containers:
```bash
docker compose down
```

---

### Option 2: Running Locally with .NET CLI

1. Ensure PostgreSQL is running and update the connection string in `src/InventoryManagement.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=inventory_db;Username=postgres;Password=postgres"
   }
   ```

2. Run the API:
   ```bash
   dotnet run --project src/InventoryManagement.Api
   ```

3. Navigate to Swagger: [https://localhost:7148/swagger](https://localhost:7148/swagger) or [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## API Reference

### Products (`/products`)

| Method | Endpoint | Description | Status Codes |
| :--- | :--- | :--- | :--- |
| `POST` | `/products` | Create a new product | `201 Created`, `400 Bad Request`, `409 Conflict` |
| `GET` | `/products` | List all products | `200 OK` |
| `GET` | `/products/{code}` | Get product details by code | `200 OK`, `404 Not Found` |
| `GET` | `/products/{code}/stock` | Get stock levels for a product across all warehouses | `200 OK`, `404 Not Found` |

#### Create Product Example
```http
POST /products
Content-Type: application/json

{
  "code": "PROD-001",
  "name": "Wireless Ergonomic Mouse"
}
```

---

### Warehouses (`/warehouses`)

| Method | Endpoint | Description | Status Codes |
| :--- | :--- | :--- | :--- |
| `POST` | `/warehouses` | Create a new warehouse | `201 Created`, `400 Bad Request`, `409 Conflict` |
| `GET` | `/warehouses` | List all warehouses | `200 OK` |
| `GET` | `/warehouses/{code}` | Get warehouse details by code | `200 OK`, `404 Not Found` |
| `GET` | `/warehouses/{code}/stock` | Get stock levels for all products in a warehouse | `200 OK`, `404 Not Found` |
| `POST` | `/warehouses/{code}/stock` | Add / initialize stock for a product in a warehouse | `200 OK`, `400 Bad Request`, `404 Not Found` |

#### Add Stock Example
```http
POST /warehouses/WH-NORTH/stock
Content-Type: application/json

{
  "productCode": "PROD-001",
  "quantity": 50
}
```

---

### Orders / Stock Transfers (`/orders`)

| Method | Endpoint | Description | Status Codes |
| :--- | :--- | :--- | :--- |
| `POST` | `/orders` | Transfer stock between warehouses | `200 OK`, `400 Bad Request`, `404 Not Found` |

#### Create Order Example
```http
POST /orders
Content-Type: application/json

{
  "productCode": "PROD-001",
  "sourceWarehouseCode": "WH-NORTH",
  "destinationWarehouseCode": "WH-SOUTH",
  "quantity": 10
}
```

#### Deficit Error Response (RFC 7807)
If requested quantity exceeds available stock, the API returns a structured ProblemDetails response:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Insufficient Stock",
  "status": 400,
  "detail": "Insufficient stock for product 'PROD-001' at warehouse 'WH-NORTH': required 10, but only 4 available (missing 6).",
  "instance": "/orders",
  "productCode": "PROD-001",
  "warehouseCode": "WH-NORTH",
  "requiredQuantity": 10,
  "availableQuantity": 4,
  "missingQuantity": 6
}
```

---

## Concurrency & Data Integrity

Stock transfers use **Pessimistic Row-Level Locking** inside an explicit database transaction:

```sql
SELECT quantity 
FROM stock 
WHERE UPPER(warehouse_code) = UPPER(@SourceWarehouseCode) 
  AND UPPER(product_code) = UPPER(@ProductCode) 
FOR UPDATE;
```

1. Acquires an exclusive row lock on the source stock record.
2. Evaluates stock sufficiency within the locked transaction.
3. Debits source warehouse stock.
4. Atomically credits destination warehouse stock using upsert (`ON CONFLICT (warehouse_code, product_code) DO UPDATE SET quantity = stock.quantity + EXCLUDED.quantity`).
5. Appends an audit entry to the `orders` table.
6. Commits the transaction atomically or rolls back cleanly on error.

---

## Running Tests

### Unit Tests
Executes 93 unit tests covering Domain entities, Application services, and API controllers using `Shouldly` assertions and `Moq`:
```bash
dotnet test tests/InventoryManagement.UnitTests
```

### Integration Tests
Executes end-to-end integration tests and the **20-thread concurrent race condition test** using real PostgreSQL instances managed by `Testcontainers`:
```bash
dotnet test tests/InventoryManagement.IntegrationTests
```

### Run Entire Test Suite
```bash
dotnet test
```

---

## Project Structure

```
inventory-management-service/
├── .github/
│   └── workflows/
│       └── ci.yml                        # GitHub Actions CI pipeline
├── src/
│   ├── InventoryManagement.Domain/       # Domain Entities, Exceptions & Interfaces
│   ├── InventoryManagement.Application/  # DTOs, Business Logic & Service Interfaces
│   ├── InventoryManagement.Infrastructure/ # PostgreSQL Schema, Dapper Repositories
│   └── InventoryManagement.Api/          # Controllers, Middleware & Configuration
├── tests/
│   ├── InventoryManagement.UnitTests/    # 93 Unit Tests (Shouldly + Moq)
│   └── InventoryManagement.IntegrationTests/ # 12 Testcontainers Integration Tests
├── Dockerfile                            # Multi-stage production container build
├── docker-compose.yml                    # Local multi-container orchestration
└── README.md
```
