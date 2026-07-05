# API Standards

## Conventions

- Use `/api` as the API base path for business endpoints.
- Use controller actions for initial sprint delivery.
- Return consistent HTTP status codes.
- Validate incoming requests before executing application workflows.
- Keep exception handling centralized through middleware.

## Health

The current health endpoint is:

```text
GET /health
```

Business API endpoints will be added in later batches.
