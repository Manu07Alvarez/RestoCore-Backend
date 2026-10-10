# Implementation Plan: Repositories, Unit of Work, and Concurrency Control

**Branch**: `feature/006-repository-unitofwork-concurrency` | **Date**: 2026-10-10 | **Spec**: [specs/006-repository-unitofwork-concurrency/spec.md](file:///r:/Programando/RestoCore/resto-core-back/specs/006-repository-unitofwork-concurrency/spec.md)

**Input**: Feature specification from `specs/006-repository-unitofwork-concurrency/spec.md`

---

## Summary

Migrate the backend persistence layer from direct invocation of the monolithic `IApplicationDbContext` God Node to domain-scoped Repositories (`ICatalogRepository`, `IKitchenRepository`, `IMenuLayoutRepository`, `ITenantRepository`) and an explicit `IUnitOfWork`. Introduce encapsulated optimistic concurrency retries with exponential backoff on high-contention endpoints (such as kitchen availability toggles) while ensuring zero regressions across existing unit and integration test suites.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10.0 (net10.0)

**Primary Dependencies**: EF Core 10.0 (`Npgsql.EntityFrameworkCore.PostgreSQL`), MediatR, FluentValidation, Microsoft.Extensions.DependencyInjection

**Storage**: PostgreSQL 16+ (Dockerized, multitenant with tenant isolation via EF Core query filters)

**Testing**: xUnit, Moq, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing (`WebApplicationFactory`), Testcontainers

**Target Platform**: Linux containers / Windows dev environment

**Performance Goals**: Dynamic API response processing < 500ms; zero unhandled concurrency exceptions under 50 simultaneous requests

**Constraints**: Preserves clean architecture layers: Interfaces defined in `Application.Common.Interfaces.Repositories`, implementations in `Infrastructure.Persistence.Repositories`

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Principle I: Spec-Driven Development**: Full OpenAPI contracts and behavior preserved; internal persistence layer refactoring adheres strictly to `spec.md`.
- **Principle II: Multi-Tenancy**: All repositories execute within existing tenant isolation rules (global query filters via `TenantId`).
- **Principle III: Clean Architecture & Domain Cohesion**: **GREATLY ENHANCED**. Handlers no longer couple to EF Core namespaces or monolithic `IApplicationDbContext`. Interfaces live in `Application`, implementations in `Infrastructure`.
- **Principle IV: Latency Budget & Asynchronous Decoupling**: Direct projected `AsNoTracking()` queries maintain <500ms server response budget.
- **Principle V: Relational Discipline**: PostgreSQL remains single source of truth; transactional boundaries are now explicit via `IUnitOfWork`.
- **Principle VI: Test-Driven Quality (NON-NEGOTIABLE)**: Full existing test suite (43+ tests) must pass, plus new tests for concurrency retries and repository mocks.

---

## Project Structure

### Documentation (this feature)

```text
specs/006-repository-unitofwork-concurrency/
├── plan.md              # This file
├── research.md          # Architectural research & decisions
├── data-model.md        # Interface definitions and retry workflow
├── quickstart.md        # Handler injection guidelines
└── tasks.md             # Generated in tasks phase
```

### Source Code

```text
src/RestoCore.Application/
├── Common/
│   └── Interfaces/
│       ├── IUnitOfWork.cs
│       └── Repositories/
│           ├── ICatalogRepository.cs
│           ├── IKitchenRepository.cs
│           ├── IMenuLayoutRepository.cs
│           └── ITenantRepository.cs
└── Features/
    ├── Categories/Commands/          # Handlers updated to ICatalogRepository & IUnitOfWork
    ├── Kitchen/Commands/             # Handlers updated to IKitchenRepository
    ├── MenuItems/Commands/           # Handlers updated to ICatalogRepository & IUnitOfWork
    ├── MenuLayout/Commands/          # Handlers updated to IMenuLayoutRepository & IUnitOfWork
    ├── PublicMenu/Queries/           # Handlers updated to ICatalogRepository
    ├── QRCodes/Queries/              # Handlers updated to ITenantRepository
    └── Tenants/Commands/             # Handlers updated to ITenantRepository & IUnitOfWork

src/RestoCore.Infrastructure/
├── Persistence/
│   ├── Repositories/
│   │   ├── CatalogRepository.cs
│   │   ├── KitchenRepository.cs
│   │   ├── MenuLayoutRepository.cs
│   │   └── TenantRepository.cs
│   ├── UnitOfWork.cs
│   └── DependencyInjection.cs       # Service registration for repositories and unit of work

tests/RestoCore.UnitTests/
├── Features/Kitchen/                # Unit tests for optimistic retry behavior
└── Infrastructure/                  # Tests for Repository implementations
```
