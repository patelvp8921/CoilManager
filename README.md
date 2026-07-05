# CoilManager

CoilManager is the backend solution skeleton for an enterprise ERP application built on .NET 8, SQL Server, Clean Architecture, Entity Framework Core, JWT authentication, Serilog, AutoMapper, FluentValidation, and xUnit.

No business modules are included yet. The current codebase establishes the production project structure, build configuration, package management, dependency boundaries, API bootstrap, persistence bootstrap, and smoke tests.

## Solution layout

```text
src/
  CoilManager.API
  CoilManager.Application
  CoilManager.Domain
  CoilManager.Infrastructure
  CoilManager.Persistence
  CoilManager.Shared
tests/
  CoilManager.UnitTests
  CoilManager.IntegrationTests
```

## Architecture rules

- `CoilManager.Domain` contains enterprise domain primitives and must not depend on other solution projects.
- `CoilManager.Application` contains application contracts, use cases, validation, mapping, and orchestration.
- `CoilManager.Persistence` owns EF Core and SQL Server persistence concerns.
- `CoilManager.Infrastructure` owns external services and platform integrations.
- `CoilManager.API` is the delivery layer and composition root.
- Test projects reference only the layers they verify.

## Build

```powershell
dotnet restore
dotnet build CoilManager.sln
dotnet test CoilManager.sln
```

## Run the API

```powershell
dotnet run --project src/CoilManager.API
```

The development profile exposes Swagger and a health endpoint:

```text
GET http://localhost:5170/health
```

## Configuration

The API reads these sections from configuration:

- `ConnectionStrings:DefaultConnection`
- `Cors:AllowedOrigins`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:SigningKey`
- `Serilog`

Replace the default JWT signing key before deploying outside local development.
