# Inventory Management Service

[![CI Pipeline](https://github.com/elmarlegrange/inventory-management-service/actions/workflows/ci.yml/badge.svg)](https://github.com/elmarlegrange/inventory-management-service/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Vue.js 3](https://img.shields.io/badge/Vue.js-3.5-4FC08D?logo=vuedotjs)
![TypeScript](https://img.shields.io/badge/TypeScript-5.8-3178C6?logo=typescript)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Latest-336791?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)
![Nginx](https://img.shields.io/badge/Nginx-Alpine-009639?logo=nginx)
![JWT Auth](https://img.shields.io/badge/Security-JWT%20RBAC-000000?logo=jsonwebtokens)

A high-performance, full-stack multi-warehouse inventory management service and real-time stock transfer engine. Built with a containerized **.NET 10 REST API** adhering to Clean Architecture principles, **PostgreSQL** with pessimistic row-locking for race-condition-free transfers, and a responsive **Vue 3 / TypeScript SPA** served via an **Nginx** reverse proxy.

---

## Features

- **Multi-Warehouse Management**: Register distribution facilities and monitor centralized inventory across all locations.
- **Product Catalog & Stock Tracking**: Manage products and view real-time item availability per facility or aggregated across the entire network.
- **Reliable Stock Transfers**: Move stock between warehouses with automated checks to guarantee inventory never goes negative or gets oversold.
- **Role-Based Access Control**:
  - **Admin**: Exclusive authority to register new warehouses and allocate initial stock, alongside full catalog and transfer permissions.
  - **User**: Standard access to manage products, initiate stock transfers, and query inventory levels across facilities.
- **Modern Web Dashboard**: Responsive single-page interface with dedicated views for products, warehouses, and transfer requests.
- **1-Click Demo Sign-In**: Easily switch between Admin and User roles to explore the application and verify permission boundaries.
- **Clear Deficit Reporting**: Helpful error messages and shortfall calculations when a requested transfer exceeds available stock.
- **Automated Verification**: Comprehensive test suite covering end-to-end user workflows and high-concurrency transfer scenarios.

---

## Tech Stack

| Layer / Concern | Technology | Details |
| :--- | :--- | :--- |
| **Backend API** | .NET 10 / C# 14 | Minimal hosting, controllers, scoped services |
| **Architecture** | Clean Architecture (DDD) | Domain, Application, Infrastructure, Api layers |
| **Data Access** | Dapper | Raw high-performance SQL queries & transactions |
| **Database** | PostgreSQL (`latest`) | Normalized schema, foreign keys, check constraints, row-locking |
| **Authentication** | JWT & PBKDF2 | HMAC-SHA256 tokens, SHA-512 with 16-byte cryptographically secure salt |
| **Frontend SPA** | Vue 3 + TypeScript + Vite | Composition API (`<script setup>`), Axios interceptors |
| **Web Server / Proxy** | Nginx (`alpine`) | Single-entry reverse proxy for SPA and API routes |
| **API Docs** | Swagger / OpenAPI | Interactive UI with JWT Bearer token authorization |
| **Unit Testing** | xUnit, Shouldly, Moq | 115 unit tests covering domain entities, services, controllers |
| **Integration Testing** | Testcontainers, WebApplicationFactory | 18 real-database integration tests including 20-thread concurrency |
| **Containerization & CI** | Docker, Docker Compose, GitHub Actions | Multi-stage Docker builds and automated CI pipelines |

---

## Default Demo Accounts

On initial database startup, standard accounts are automatically seeded with salted PBKDF2 hashes:

| Username | Password | Role | Permissions |
| :--- | :--- | :--- | :--- |
| `admin` | `Admin123!` | **Admin** | Full access: Create warehouses, provision stock, manage products, transfer inventory |
| `user` | `User123!` | **User** | Standard access: Manage products, transfer inventory, view stock (warehouse scoping blocked with `403 Forbidden`) |

---

## Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Docker Engine + Docker Compose)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) *(optional, for local CLI development)*
- [Node.js 20+](https://nodejs.org/) *(optional, for local frontend development)*

---

### Option 1: Running via Docker Compose (Recommended)

Spin up the entire stack (PostgreSQL database, .NET 10 API, and Nginx Vue 3 client) with a single command:

```bash
docker compose up --build -d
```

Once running, access the services:
- **Vue 3 Web Application**: [http://localhost:3000](http://localhost:3000)
- **Backend API**: `http://localhost:8080`
- **Swagger UI**: [http://localhost:8080/swagger](http://localhost:8080/swagger)

To stop and clean up containers:
```bash
docker compose down
```

---

### Option 2: Running Locally for Development

#### 1. Start PostgreSQL
```bash
docker run -d --name inventory-postgres -p 5432:5432 -e POSTGRES_DB=inventory_db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres postgres:latest
```

#### 2. Run the .NET 10 API
```bash
dotnet run --project server/src/InventoryManagement.Api
```
The API listens on `http://localhost:8080` (or `http://localhost:5000` / `https://localhost:5001`). Swagger UI is available at `/swagger`.

#### 3. Run the Vue 3 Frontend
```bash
cd apps/web
npm install
npm run dev
```
The Vite dev server starts on [http://localhost:3000](http://localhost:3000) and automatically proxies `/auth`, `/products`, `/warehouses`, and `/orders` requests to the API on port `8080`.

---

## API Reference

All mutating and query endpoints require a valid JWT Bearer token supplied in the request header:
```http
Authorization: Bearer <token>
```

### Authentication (`/auth`)

| Method | Endpoint | Access | Description | Status Codes |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/auth/login` | Anonymous | Authenticate with credentials and receive a JWT | `200 OK`, `400 Bad Request`, `401 Unauthorized` |
| `GET` | `/auth/me` | Authenticated | Inspect current authenticated identity and role | `200 OK`, `401 Unauthorized` |

#### Login Request Example
```http
POST /auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin123!"
}
```

#### Login Response Example
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-09-05T15:00:00Z",
  "username": "admin",
  "role": "Admin"
}
```

---

### Warehouses (`/warehouses`)

| Method | Endpoint | Required Role | Description | Status Codes |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/warehouses` | **Admin** | Register a new warehouse facility | `201 Created`, `400`, `401`, `403`, `409` |
| `GET` | `/warehouses` | Admin, User | List all warehouse locations | `200 OK`, `401 Unauthorized` |
| `GET` | `/warehouses/{code}` | Admin, User | Get warehouse details by code | `200 OK`, `401`, `404 Not Found` |
| `GET` | `/warehouses/{code}/stock` | Admin, User | List all stock stored in a warehouse | `200 OK`, `401`, `404 Not Found` |
| `POST` | `/warehouses/{code}/stock` | **Admin** | Provision stock for a product in a warehouse | `200 OK`, `400`, `401`, `403`, `404` |

#### Provision Stock Example (Admin Only)
```http
POST /warehouses/WH-NORTH/stock
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "productCode": "PROD-001",
  "quantity": 100
}
```

---

### Products (`/products`)

| Method | Endpoint | Required Role | Description | Status Codes |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/products` | Admin, User | Create a new catalog product | `201 Created`, `400`, `401`, `409` |
| `GET` | `/products` | Admin, User | List all catalog products | `200 OK`, `401 Unauthorized` |
| `GET` | `/products/{code}` | Admin, User | Get product details by code | `200 OK`, `401`, `404 Not Found` |
| `GET` | `/products/{code}/stock` | Admin, User | View stock levels across all warehouses | `200 OK`, `401`, `404 Not Found` |

#### Create Product Example
```http
POST /products
Authorization: Bearer <token>
Content-Type: application/json

{
  "code": "PROD-001",
  "name": "Wireless Ergonomic Keyboard"
}
```

---

### Orders & Stock Transfers (`/orders`)

| Method | Endpoint | Required Role | Description | Status Codes |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/orders` | Admin, User | Transfer stock atomically between warehouses | `200 OK`, `400`, `401`, `404` |

#### Create Transfer Order Example
```http
POST /orders
Authorization: Bearer <token>
Content-Type: application/json

{
  "productCode": "PROD-001",
  "sourceWarehouseCode": "WH-NORTH",
  "destinationWarehouseCode": "WH-SOUTH",
  "quantity": 25
}
```

#### Deficit Error Response (RFC 7807)
When requested transfer stock exceeds available inventory, the engine returns an RFC 7807 ProblemDetails payload with shortfall calculations:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Insufficient Stock",
  "status": 400,
  "detail": "Insufficient stock for product 'PROD-001' at warehouse 'WH-NORTH': required 25, but only 10 available (missing 15).",
  "instance": "/orders",
  "productCode": "PROD-001",
  "warehouseCode": "WH-NORTH",
  "requiredQuantity": 25,
  "availableQuantity": 10,
  "missingQuantity": 15
}
```

---

## Running Tests

### Unit Tests (115 tests)
Executes all unit tests across Domain entities, Application services, Auth components, and API controllers:
```bash
dotnet test server/tests/InventoryManagement.UnitTests
```

### Integration Tests (18 tests)
Executes end-to-end integration tests, including role-based authorization suites and the **20-thread concurrent race-condition test** using isolated PostgreSQL containers managed by `Testcontainers`:
```bash
dotnet test server/tests/InventoryManagement.IntegrationTests
```

### Run Entire Backend Suite
```bash
dotnet test server/InventoryManagement.slnx
```

### Frontend Typecheck & Production Build
```bash
cd apps/web
npm run build
```

---

## Project Structure

```
inventory-management-service/
├── .github/
│   └── workflows/
│       └── ci.yml                            # Automated CI build, test & coverage pipeline
├── apps/
│   └── web/                                  # Vue 3 / Vite TypeScript SPA Client
│       ├── src/
│       │   ├── api/                          # Axios API clients (auth, products, warehouses, orders)
│       │   ├── components/                   # Vue components (auth, common, products, warehouses, orders)
│       │   ├── composables/                  # Reactive state composables (useAuth)
│       │   ├── types/                        # TypeScript domain & DTO interfaces
│       │   ├── App.vue                       # Root application layout with auth header
│       │   └── main.ts                       # App entrypoint
│       ├── nginx.conf                        # Nginx reverse proxy configuration
│       ├── Dockerfile                        # Multi-stage static build & Nginx container
│       └── vite.config.ts                    # Vite dev proxy configuration
├── server/                                   # .NET 10 Backend Solution
│   ├── src/
│   │   ├── InventoryManagement.Domain/       # Entities (Role, User, Stock, Warehouse, Product), Exceptions
│   │   ├── InventoryManagement.Application/  # DTOs, AuthService, PasswordHasher, JwtTokenGenerator
│   │   ├── InventoryManagement.Infrastructure/ # DatabaseInitializer, Dapper Repositories (PostgreSQL)
│   │   └── InventoryManagement.Api/          # Controllers (Auth, Products, Warehouses, Orders), Middleware
│   ├── tests/
│   │   ├── InventoryManagement.UnitTests/    # 115 Unit Tests (xUnit + Shouldly + Moq)
│   │   └── InventoryManagement.IntegrationTests/ # 18 Integration Tests (Testcontainers + AuthApiTests)
│   ├── Dockerfile                            # Multi-stage .NET 10 SDK build & runtime container
│   └── InventoryManagement.slnx              # Solution file
├── docker-compose.yml                        # 3-tier multi-container configuration (DB, API, Web)
└── README.md
```

