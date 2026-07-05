# CoilManager

CoilManager is an enterprise ERP foundation for coil inventory and processing workflows. Sprint 1 focuses on the production skeleton only: layered .NET backend, Angular application shell, database script folders, documentation, and verification tests.

Business modules, including Raw Coil CRUD, are intentionally deferred.

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Clean Architecture
- Serilog
- AutoMapper
- FluentValidation
- xUnit
- Angular
- Angular Material
- SCSS

## Solution Layout

```text
src/
  CoilManager.API
  CoilManager.Application
  CoilManager.Domain
  CoilManager.Infrastructure
  CoilManager.Persistence
  CoilManager.Shared
  CoilManager.UI
tests/
  CoilManager.UnitTests
  CoilManager.IntegrationTests
database/
  Scripts
  SeedData
  StoredProcedures
  Migrations
docs/
```

## Architecture Rules

- `CoilManager.Domain` contains enterprise domain primitives and must not depend on other solution projects.
- `CoilManager.Application` contains application contracts, use cases, validation, mapping, and orchestration.
- `CoilManager.Persistence` owns EF Core and SQL Server persistence concerns.
- `CoilManager.Infrastructure` owns external services and platform integrations.
- `CoilManager.API` is the delivery layer and composition root.
- `CoilManager.UI` is the Angular frontend shell.

## Backend Build

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

## Frontend

```powershell
cd src/CoilManager.UI
npm install
npm start
```

Build the Angular app with:

```powershell
npm run build
```

The Angular development server runs on:

```text
http://localhost:4200
```

## Configuration

The API reads these sections from configuration:

- `ConnectionStrings:DefaultConnection`
- `Cors:AllowedOrigins`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`
- `JwtSettings:SigningKey`
- `JwtSettings:ExpiryMinutes`
- `Serilog`

Replace the default JWT signing key before deploying outside local development.

## Sprint 1 Status

- Backend solution and layered skeleton are in place.
- API bootstrap, health endpoint, global exception middleware, and Serilog configuration are in place.
- Persistence bootstrap and `CoilManagerDbContext` are in place.
- Angular workspace and application shell are in place.
- Raw Coil UI placeholder routes are in place without CRUD behavior.
- Database script folder structure and placeholder scripts are in place.
- Architecture, API, database, coding, branching, and sprint docs are in place.
- Unit and integration test projects include architecture and placeholder coverage.
