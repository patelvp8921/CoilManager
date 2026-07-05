# Database Design

CoilManager targets SQL Server with Entity Framework Core owned by the Persistence layer.

## Folder Layout

- `database/Scripts` contains hand-authored bootstrap scripts.
- `database/SeedData` contains seed scripts.
- `database/StoredProcedures` contains stored procedure scripts when needed.
- `database/Migrations` is reserved for migration artifacts or release scripts.

## Current State

Only bootstrap placeholders are included. Business tables are deferred until business modules are implemented.
