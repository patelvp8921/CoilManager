# Database Design

CoilManager uses SQL Server through Entity Framework Core 8. The Persistence project owns the EF Core model, migrations, repository implementations, and seed structure.

## DbContext

`ApplicationDbContext` is the primary EF Core context and is registered from `CoilManager.Persistence.DependencyInjection`.

Current DbSets:

- `RawCoils`
- `Users`
- `Roles`
- `UserRoles`

`CoilManagerDbContext` remains as a compatibility wrapper around `ApplicationDbContext`.

## Schemas

- `app` contains operational ERP tables such as `RawCoils`.
- `auth` contains user and role tables.

## Entity Configuration

Entity mappings live in `src/CoilManager.Persistence/Configurations` and are applied with:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
```

Current configurations:

- `RawCoilConfiguration`
- `UserConfiguration`
- `RoleConfiguration`
- `UserRoleConfiguration`

## Audit Fields

Entities deriving from `AuditableEntity` receive audit values during `SaveChanges` and `SaveChangesAsync`.

- Added entities receive `CreatedAtUtc`.
- Modified entities receive `UpdatedAtUtc`.
- The user identifier fields are currently set to `null` until the current-user pipeline is finalized.

## Soft Delete

Entities deriving from `SoftDeletableEntity` are soft deleted by changing `Deleted` entries to `Modified` and setting:

- `IsDeleted`
- `DeletedAtUtc`
- `DeletedBy`

Global query filters are applied for soft-deletable entities so deleted rows are excluded by default.

## Migrations

The initial migration is:

```text
src/CoilManager.Persistence/Migrations/*_InitialCreate.cs
```

Create future migrations from the repository root:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add <MigrationName> --project src/CoilManager.Persistence --startup-project src/CoilManager.API --context ApplicationDbContext --output-dir Migrations
```

Apply migrations manually:

```powershell
dotnet tool run dotnet-ef database update --project src/CoilManager.Persistence --startup-project src/CoilManager.API --context ApplicationDbContext
```

## Seeding

The seed entry point is `CoilManager.Persistence.Seed.DatabaseSeeder`.

It is intentionally a placeholder in Sprint 1 Batch 2.5. Seed data should be added only when a module explicitly requires it.
