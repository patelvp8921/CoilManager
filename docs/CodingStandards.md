# Coding Standards

## .NET

- Use nullable reference types and keep warnings treated as errors.
- Keep Domain independent of other solution projects.
- Register dependencies through each layer's `DependencyInjection` entry point.
- Prefer explicit contracts in Application for outer-layer services.
- Keep controllers thin and delegate orchestration to Application services.

## Angular

- Prefer standalone components.
- Use SCSS and Angular Material for shared UI patterns.
- Keep feature code under `src/app/features`.
- Keep reusable UI and utility code under `shared`.
- Keep app-wide services and interceptors under `core`.
