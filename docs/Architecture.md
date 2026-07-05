# Architecture

CoilManager follows Clean Architecture with separate projects for Domain, Application, Persistence, Infrastructure, API, Shared, and UI concerns.

## Layers

- Domain contains enterprise primitives and domain rules.
- Application contains contracts, validation, mapping, and use-case orchestration.
- Persistence owns Entity Framework Core and SQL Server access.
- Infrastructure owns external services and platform integrations.
- API is the ASP.NET Core delivery layer and composition root.
- UI is the Angular application shell.

Business modules are not implemented yet. Sprint 1 establishes structure, dependency boundaries, and build verification.
