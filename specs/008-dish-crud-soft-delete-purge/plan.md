# Implementation Plan: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker

**Branch**: `feature/008-dish-crud-soft-delete-purge` | **Date**: 2026-10-11 | **Spec**: [specs/008-dish-crud-soft-delete-purge/spec.md](file:///r:/Programando/RestoCore/resto-core-back/specs/008-dish-crud-soft-delete-purge/spec.md)

**Input**: Feature specification from `specs/008-dish-crud-soft-delete-purge/spec.md` derived from canonical `RestoCore-Docs` (ADR-0010, `docs/prd-admin-crud.md`, `specs/openapi.yaml`).

---

## 1. Summary

Implement administrative endpoints for total (`PUT`) and partial (`PATCH`) modification of menu items with strict 1:N category validation and N:M dietary labels support, soft deletion (`DELETE`) with active orders check (HTTP 409 if dishes are being prepared in kitchen) and 7-day retention grace period accompanied by a human-friendly response, restoration (`POST /restore`) within 7 days, and an automated background service (`PurgeArchivedDishesWorker`) for scheduled physical cleanup of records and SeaweedFS images.

---

## 2. Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**: ASP.NET Core Minimal APIs, Entity Framework Core 10, Npgsql.EntityFrameworkCore.PostgreSQL, MediatR-style handlers  
**Storage**: PostgreSQL (`menu_items`, `categories`, `orders`), SeaweedFS object storage for image deletion  
**Testing**: xUnit, FluentAssertions, Moq (`RestoCore.UnitTests`)  
**Target Platform**: Linux containers / Windows dev  
**Project Type**: Web API backend microservice  
**Performance Goals**: Dynamic API updates < 500ms; LCP budget < 2s for public menu; background purge decoupled from request pipeline  
**Constraints**: Strict tenant isolation (`ITenantContext`), OPA authorization (`TenantAdminOnly`), Zero data loss on soft deletion (accounting compliance via immutable order snapshots)  

---

## 3. Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Evaluation | Status |
| :--- | :--- | :--- |
| **I. Spec-Driven Development** | Endpoints and DTOs match OpenAPI 3.1 in `RestoCore-Docs/specs/openapi.yaml`. | PASS |
| **II. Multi-Tenancy & Authorization** | All operations filter by `TenantId`. Endpoints protected with `TenantAdminOnly`. | PASS |
| **III. Clean Architecture** | Commands/Handlers in `Application/Features/MenuItems/`, persistence in `Infrastructure/`, endpoints in `Api/`. | PASS |
| **IV. Latency Budget & Async Decoupling** | Heavy image cleanup and purge offloaded to background worker; updates invalidate cache and complete < 500ms. | PASS |
| **V. Persistence Discipline** | Soft delete tracked in PostgreSQL with filtered indices; immutable order snapshots protected from cascade delete. | PASS |
| **VI. Test-Driven Quality** | Unit tests will validate validation failures, 409 conflict on active orders, soft delete retention, and restoration. | PASS |

---

## 4. Project Structure

### Documentation (this feature)
```text
specs/008-dish-crud-soft-delete-purge/
├── spec.md              # Feature specification
├── plan.md              # Implementation plan (this file)
├── research.md          # Architectural research & decisions
├── data-model.md        # Data models, DDL, and state transitions
├── contracts/
│   └── contracts.md     # OpenAPI REST request/response contracts
└── quickstart.md        # Verification and test instructions
```

### Source Code Changes Planned
```text
src/
├── RestoCore.Domain/
│   └── Entities/
│       └── MenuItem.cs                              # Add IsDeleted, DeletedAt
├── RestoCore.Application/
│   ├── Common/Interfaces/Repositories/
│   │   ├── ICatalogRepository.cs                   # Add GetMenuItemByIdWithDeletedAsync, GetArchivedMenuItemsOlderThanAsync
│   │   └── IOrderRepository.cs                     # Add HasActiveOrdersForMenuItemAsync
│   ├── Common/Interfaces/
│   │   └── IStorageService.cs                      # Add DeleteObjectAsync
│   └── Features/MenuItems/
│       ├── Commands/
│       │   ├── UpdateMenuItemCommand.cs             # PUT handler
│       │   ├── PatchMenuItemCommand.cs              # PATCH handler
│       │   ├── DeleteMenuItemCommand.cs             # Soft delete handler with active orders check
│       │   └── RestoreMenuItemCommand.cs            # Restore within 7 days handler
│       └── DTOs/
│           └── MenuItemArchiveResponse.cs           # Human-friendly response DTO
├── RestoCore.Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/MenuConfiguration.cs      # Configure filtered indices & properties
│   │   ├── ApplicationDbContext.cs                  # Apply global query filter (!IsDeleted)
│   │   └── Repositories/
│   │       ├── CatalogRepository.cs                # Implement methods
│   │       └── OrderRepository.cs                  # Implement active orders check
│   ├── Storage/
│   │   └── SeaweedFsStorageService.cs              # Implement DeleteObjectAsync
│   └── Workers/
│       └── PurgeArchivedDishesWorker.cs             # BackgroundService running scheduled purge
└── RestoCore.Api/
    └── Endpoints/
        └── AdminCatalogEndpoints.cs                 # Map PUT, PATCH, DELETE, POST /restore

tests/RestoCore.UnitTests/
└── Features/
    └── MenuItems/
        ├── UpdateMenuItemCommandHandlerTests.cs
        ├── PatchMenuItemCommandHandlerTests.cs
        ├── DeleteMenuItemCommandHandlerTests.cs
        └── RestoreMenuItemCommandHandlerTests.cs
```

---

## 5. Implementation Phases

- **Phase 0: Research & Consolidate** → [research.md](file:///r:/Programando/RestoCore/resto-core-back/specs/008-dish-crud-soft-delete-purge/research.md) (Complete)
- **Phase 1: Design & Contracts** → [data-model.md](file:///r:/Programando/RestoCore/resto-core-back/specs/008-dish-crud-soft-delete-purge/data-model.md), [contracts/contracts.md](file:///r:/Programando/RestoCore/resto-core-back/specs/008-dish-crud-soft-delete-purge/contracts/contracts.md), [quickstart.md](file:///r:/Programando/RestoCore/resto-core-back/specs/008-dish-crud-soft-delete-purge/quickstart.md) (Complete)
- **Phase 2: Task Generation** → Run `/speckit-tasks` to create `tasks.md`.
