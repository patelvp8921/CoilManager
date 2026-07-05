# Developer Setup

## Prerequisites

- .NET SDK 8.0
- SQL Server LocalDB or SQL Server Developer Edition
- Visual Studio 2022 or another .NET-capable IDE
- Node.js only when working on the Angular UI

## Restore and Build

From the repository root:

```powershell
dotnet restore CoilManager.sln --configfile NuGet.config
dotnet build CoilManager.sln --no-restore
dotnet test CoilManager.sln --no-build
```

## EF Core Tooling

This repository uses a local .NET tool manifest for EF Core tooling.

Install or restore tools:

```powershell
dotnet tool restore
```

The manifest installs `dotnet-ef` version `8.0.8`.

## Local Database

The development connection string is defined in:

```text
src/CoilManager.API/appsettings.Development.json
```

Default value:

```text
Server=(localdb)\MSSQLLocalDB;Database=CoilManager_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Update the connection string locally if you use SQL Server Developer Edition or a named SQL Server instance.

## Apply Migrations

Create the local database by applying migrations manually:

```powershell
dotnet tool run dotnet-ef database update --project src/CoilManager.Persistence --startup-project src/CoilManager.API --context ApplicationDbContext
```

The API does not auto-migrate by default. To opt in for local development only, set:

```json
{
  "Database": {
    "ApplyMigrationsOnStartup": true
  }
}
```

Keep this disabled in shared and production environments unless the deployment process explicitly requires application-driven migrations.

## Add a Migration

```powershell
dotnet tool run dotnet-ef migrations add <MigrationName> --project src/CoilManager.Persistence --startup-project src/CoilManager.API --context ApplicationDbContext --output-dir Migrations
```

Review generated migrations before committing them.

## Run the API

```powershell
dotnet run --project src/CoilManager.API
```

Health checks are available at:

```text
GET /health
```
