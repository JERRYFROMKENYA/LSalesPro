# L-SalesPro Microservices API

## Overview
L-SalesPro is a distributed backend system built with .NET 8.0, following Clean Architecture and microservices best practices. It demonstrates enterprise-level design, gRPC inter-service communication, and robust authentication/authorization.

**Microservices:**
- **Authentication Service** (Port 5001): User authentication, JWT, user management, gRPC for internal validation.
- **Sales & Customer Service** (Port 5002): Customer/order management, analytics, gRPC client to Inventory.
- **Inventory Service** (Port 5003): Product/warehouse/stock management, gRPC server for stock operations.
- **API Gateway** (Port 5000): Single entry point, routing, cross-cutting concerns.

## Features
- Clean Architecture (Domain, Application, Infrastructure, API)
- Entity Framework Core (Code First, per-service DB, migrations)
- JWT authentication & role-based authorization
- gRPC for internal service communication
- RESTful APIs with versioning and OpenAPI/Swagger
- In-memory and distributed caching (IMemoryCache, Redis-ready)
- Health checks, structured logging (Serilog), global exception handling
- Test projects for unit, integration, and gRPC tests
- Seed data for users, customers, products, warehouses

## Setup Guide

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (for production, SQLite for dev)
- [Postman](https://www.postman.com/) (for API testing)
- [Docker](https://www.docker.com/) (optional, for containerization)

### Installation Steps
1. **Clone the repository:**
   ```sh
   git clone https://github.com/jerryfromkenya/lsalespro.git
   cd lsalespro
   ```
2. **Restore dependencies:**
   ```sh
   dotnet restore
   ```
3. **Apply database migrations and seed data:**
   For each service (example for AuthService):
   ```sh
   cd src/AuthService/AuthService.Infrastructure
   dotnet ef database update
   ```
4. **Run all services:**
   ```sh
   ./start-all.ps1
   ```
   Or run each service individually:
   ```sh
   dotnet run --project src/AuthService/AuthService.Api
   dotnet run --project src/SalesService/SalesService.Api
   dotnet run --project src/InventoryService/InventoryService.Api
   dotnet run --project src/ApiGateway/ApiGateway.csproj
   ```
5. **Access Swagger UI:**
   - AuthService: [http://localhost:5001/swagger](http://localhost:5001/swagger)
   - SalesService: [http://localhost:5002/swagger](http://localhost:5002/swagger)
   - InventoryService: [http://localhost:5003/swagger](http://localhost:5003/swagger)
   - API Gateway: [http://localhost:5000/swagger](http://localhost:5000/swagger)

### Configuration
- Each service has its own `appsettings.json` and `appsettings.Development.json`.
- Update connection strings as needed for your environment.
- JWT settings are in the AuthService configuration.

### Seeding Data
- Users, roles, permissions, customers, products, and warehouses are seeded on first run.
- Default users (for login testing):
  - **Username:** `LEYS-1001` / **Password:** `SecurePass123!`
  - **Username:** `LEYS-1002` / **Password:** `SecurePass456!`

### Testing
- **Unit & Integration Tests:**
  ```sh
  dotnet test
  ```.


### API Endpoints
#### AuthService
- `POST /api/v1/auth/login` – User login (returns JWT)
- `POST /api/v1/auth/logout` – Logout
- `POST /api/v1/auth/refresh` – Refresh token
- `GET /api/v1/auth/user` – Get current user profile

#### SalesService
- `GET /api/v1/customers` – List customers
- `POST /api/v1/customers` – Create customer
- `GET /api/v1/orders` – List orders
- ...and more 

#### InventoryService
- `GET /api/v1/products` – List products
- `POST /api/v1/products` – Create product
- ...and more

### gRPC Contracts
- Proto files are in `src/Shared/Protos/`
- Example: `AuthService.proto`, `InventoryService.proto`

### Troubleshooting
- **401 Unauthorized:** Ensure you use a valid JWT from `/api/v1/auth/login` in the `Authorization: Bearer` header.
- **Database errors:** Ensure migrations are applied and connection strings are correct.
- **Port conflicts:** Make sure all services use their assigned ports.


