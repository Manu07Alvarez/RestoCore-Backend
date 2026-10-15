# Implementation Plan: Category ABMC & Restaurant Contact Profile Management

**Branch**: `feature/009-category-abmc-restaurant-profile` | **Date**: 2026-10-15 | **Spec**: [specs/009-category-abmc-restaurant-profile/spec.md](file:///r:/Programando/RestoCore/resto-core-back/specs/009-category-abmc-restaurant-profile/spec.md)

**Input**: Feature specification from `specs/009-category-abmc-restaurant-profile/spec.md` derived from canonical `RestoCore-Docs` (`docs/prd-admin-crud.md` @ commit `42f8e6c`, `docs/ears-menu.md`, and `specs/openapi.yaml`).

---

## 1. Summary

Implement administrative capabilities for the full lifecycle ABMC of menu categories (`GET`, `POST`, `PUT`, `PATCH`, `DELETE`) with a non-destructive referential integrity guardrail on deletion (rejecting with `HTTP 409 Conflict` if the category has dishes associated), manage establishment commercial identity and contact information via `/api/v1/admin/tenant/profile` (`GET`, `PUT`, `PATCH`), and expose these contact details (`phone_number`, `address`, `business_hours`) in the public digital QR menu payload within the latency budget (< 500ms).

---

## 2. Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**: ASP.NET Core Minimal APIs, Entity Framework Core 10, Npgsql.EntityFrameworkCore.PostgreSQL, MediatR-style CQRS handlers  
**Storage**: PostgreSQL (`categories`, `tenants`, `menu_items`)  
**Testing**: xUnit, FluentAssertions, Moq (`RestoCore.UnitTests`)  
**Target Platform**: Linux containers / Windows dev  
**Project Type**: Web API backend microservice  
**Performance Goals**: Dynamic API response < 500ms; Public QR menu LCP < 2s; CDN cache invalidation via `ICdnPurgeService` upon mutations  
**Constraints**: Multi-tenancy isolation (`ITenantContext`), Declarative OPA authorization (`TenantAdminOnly`), Non-destructive referential integrity guardrail (HTTP 409 Conflict)  
**Scale/Scope**: Multi-tenant SaaS architecture supporting hundreds of tenant restaurants  

---

## 3. Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Evaluation | Status |
| :--- | :--- | :--- |
| **I. Spec-Driven Development** | All routes, status codes (`200`, `201`, `204`, `400`, `401`, `403`, `404`, `409`), and payload structures strictly comply with `RestoCore-Docs/specs/openapi.yaml`. | PASS |
| **II. Multi-Tenancy & Authorization** | Every query filters by `TenantId`. Administrative endpoints are protected via `TenantAdminOnly` OPA policies (`manage_menu`). | PASS |
| **III. Clean Architecture** | Clean layering: Domain (`Tenant`, `Category`), Application (`Features/Categories`, `Features/Tenants`, `Features/PublicMenu`), Infrastructure (EF Core configurations, repositories), API (Minimal APIs). | PASS |
| **IV. Latency Budget & Async Decoupling** | No extra joins in `GetPublicMenuQueryHandler`; contact columns are fetched with `Tenant`. CDN cache purged via `ICdnPurgeService`. Dynamic responses < 500ms. | PASS |
| **V. Persistence Discipline** | Entity configurations in PostgreSQL with column length limits, filtered indexes, and foreign keys. `DeleteBehavior.Restrict` preserved. | PASS |
| **VI. Test-Driven Quality** | Unit tests cover all commands/handlers: category creation, listing, detail, full update, partial update, 409 conflict guardrail, tenant profile get/put/patch, and public menu contact mapping. | PASS |

---

## 4. Project Structure

### Documentation (this feature)
```text
specs/009-category-abmc-restaurant-profile/
├── spec.md              # Feature specification
├── plan.md              # Implementation plan (this file)
├── research.md          # Architectural research & decisions
├── data-model.md        # Domain models, schema extensions, and CQRS commands/queries
├── contracts/
│   └── contracts.md     # OpenAPI REST request/response contracts
└── quickstart.md        # Verification and testing guide
```

### Source Code Changes Planned
```text
src/
├── RestoCore.Domain/
│   └── Entities/
│       └── Tenant.cs                                # Add PhoneNumber, Address, BusinessHours
├── RestoCore.Application/
│   ├── Common/Interfaces/Repositories/
│   │   └── ICatalogRepository.cs                   # Add GetCategoriesByTenantAsync, HasMenuItemsForCategoryAsync
│   ├── Features/Categories/
│   │   ├── Commands/
│   │   │   ├── CreateCategoryCommand.cs             # Update with sort_order and is_visible mapping
│   │   │   ├── UpdateCategoryCommand.cs             # Full update (PUT)
│   │   │   ├── PatchCategoryCommand.cs              # Partial update (PATCH)
│   │   │   └── DeleteCategoryCommand.cs             # Delete with 409 guardrail check
│   │   ├── Queries/
│   │   │   ├── GetCategoriesQuery.cs                # List ordered categories
│   │   │   └── GetCategoryByIdQuery.cs              # Get category detail
│   │   └── DTOs/
│   │       └── CategoryResponse.cs                  # CategoryDetailDto response
│   ├── Features/Tenants/
│   │   ├── Commands/
│   │   │   ├── UpdateTenantProfileCommand.cs        # Full update (PUT)
│   │   │   └── PatchTenantProfileCommand.cs         # Partial update (PATCH)
│   │   ├── Queries/
│   │   │   └── GetTenantProfileQuery.cs             # Get tenant profile
│   │   └── DTOs/
│   │       └── TenantProfileResponse.cs             # TenantProfileDto response
│   └── Features/PublicMenu/
│       ├── DTOs/
│       │   └── PublicMenuResponse.cs                # Add PhoneNumber, Address, BusinessHours
│       └── Queries/
│           └── GetPublicMenuQuery.cs                # Map PhoneNumber, Address, BusinessHours
├── RestoCore.Infrastructure/
│   └── Persistence/
│       ├── Configurations/TenantConfiguration.cs    # Map PhoneNumber, Address, BusinessHours
│       └── Repositories/CatalogRepository.cs        # Implement new ICatalogRepository methods
└── RestoCore.Api/
    └── Endpoints/
        ├── AdminCatalogEndpoints.cs                 # Map Category GET, GET/{id}, PUT, PATCH, DELETE
        └── AdminProfileEndpoints.cs                 # Map Tenant Profile GET, PUT, PATCH

tests/RestoCore.UnitTests/
├── Features/
│   ├── Categories/
│   │   ├── GetCategoriesQueryHandlerTests.cs
│   │   ├── GetCategoryByIdQueryHandlerTests.cs
│   │   ├── CreateCategoryCommandHandlerTests.cs
│   │   ├── UpdateCategoryCommandHandlerTests.cs
│   │   ├── PatchCategoryCommandHandlerTests.cs
│   │   └── DeleteCategoryCommandHandlerTests.cs     # 409 conflict and 204 success tests
│   ├── Tenants/
│   │   ├── GetTenantProfileQueryHandlerTests.cs
│   │   ├── UpdateTenantProfileCommandHandlerTests.cs
│   │   └── PatchTenantProfileCommandHandlerTests.cs
│   └── PublicMenu/
│       └── PublicMenuContactProfileTests.cs         # Validates contact fields in public menu
```

---

## 5. Complexity Tracking

> No violations of the RestoCore Backend Constitution were detected. All components maintain clean layering, multi-tenancy isolation, and strict contract adherence.

| Item | Assessment | Notes |
| :--- | :--- | :--- |
| **Referential Integrity Guardrail** | Low / Standard | Evaluates associated dishes before category deletion to prevent orphaned records and provide human-friendly 409 responses. |
| **Profile Schema Extension** | Low / Standard | Adds 3 nullable fields to `tenants` table with standard EF Core mapping. Zero migration risk. |
| **Performance Overhead** | None | Public menu query already retrieves the `Tenant` entity; mapping 3 additional string properties adds zero database calls. |
