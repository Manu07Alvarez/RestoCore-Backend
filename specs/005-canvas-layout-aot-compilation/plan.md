# Implementation Plan: Canvas Menu Layout & AOT Perimeter Compilation

**Branch**: `003-k6-aspire-swagger` | **Date**: 2026-09-17 | **Spec**: [specs/005-canvas-layout-aot-compilation/spec.md](./spec.md)

**Input**: Feature specification from `specs/005-canvas-layout-aot-compilation/spec.md`

---

## Summary

Implement the visual Canvas Mode (Modo Lienzo) for restaurant digital menus along with an Ahead-Of-Time (AOT) compilation pipeline and perimetric CDN cache management. Visual layout geometry (`layout_config` containing 2D coordinates, element dimensions, layer order `z_index`, and background settings) is persisted as a semi-structured JSONB entity on `Tenant` in PostgreSQL, strictly decoupled from transactional catalog entities (`MenuItem`, `Category`, `ModifierGroup`). The AOT compilation pipeline pre-flattens layout coordinates, prunes orphaned elements referencing deleted dishes, generates deterministic `version_hash` identifiers, updates ETag headers, and issues selective cache purge commands to the CDN edge.

---

## Technical Context

**Language/Version**: .NET 10 / C# 13
**Primary Dependencies**: ASP.NET Core Minimal APIs, Entity Framework Core 10 (Npgsql), FluentValidation, OpenTelemetry, System.Text.Json
**Storage**: PostgreSQL 16+ (`tenants.layout_config` JSONB column and `menu_publish_jobs` table), SeaweedFS (S3-compatible gateway for background media assets), Redis (distributed caching)
**Testing**: xUnit, FluentAssertions, Moq, Microsoft.AspNetCore.Mvc.Testing (`CustomWebApplicationFactory`)
**Target Platform**: Linux / Windows containers (Docker)
**Project Type**: Multi-tenant backend REST API
**Performance Goals**:
- Public menu server-side retrieval < 150ms
- Conditional HTTP 304 Not Modified evaluation < 20ms
- AOT compilation and CDN purge dispatch SLA < 3.0 seconds
**Constraints**:
- End-to-end mobile LCP budget < 2.0 seconds
- Zero data leakage across tenants
- Zero hardcoded authorization logic; all evaluations performed declaratively via OPA Rego
- Graceful degradation to default sequential category layout if canvas mode is disabled or assets are missing
**Scale/Scope**: Multi-tenant SaaS architecture supporting concurrent rush-hour QR menu scans and atomic administrative layout updates

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Requirement | Plan Compliance | Status |
| :--- | :--- | :--- | :---: |
| **I. Spec-Driven Development** | All contracts and behaviors must adhere strictly to OpenAPI 3.1. | Endpoints `PUT /api/v1/admin/menu/layout` and `POST /api/v1/admin/menu/publish` match `RestoCore-Docs/specs/openapi.yaml` exactly. | **PASS** |
| **II. Multi-Tenancy & Declarative Auth** | Strict tenant isolation; authorization via OPA Rego policies. | Requests enforce `tenant_id` scoping; OPA policy verifies `owner` or `tenant_admin` roles. | **PASS** |
| **III. Clean Architecture** | Decoupling into Domain, Application, Infrastructure, and Api layers. | Domain defines `LayoutConfig` and `MenuPublishJob`; Application orchestrates CQRS handlers; Infrastructure implements EF Core mappings and CDN adapters; Api registers Minimal API routes. | **PASS** |
| **IV. Latency Budget & Asynchronous Decoupling** | API responses within 500ms budget; AOT pre-flattening shifts computation away from mobile clients. | Pre-flattened geometry eliminates client calculations; `POST /publish` responds 202 Accepted asynchronously; public menu delivers `ETag` and `Cache-Control`. | **PASS** |
| **V. Relational & Flexible Persistence** | PostgreSQL as single source of truth; JSONB columns for flexible data. | `LayoutConfig` mapped to PostgreSQL `jsonb` column via EF Core `OwnsOne(..., b => b.ToJson())`. | **PASS** |
| **VI. Test-Driven Quality** | Unit and integration tests required prior to completion. | Unit tests for geometric validation and orphan pruning; integration tests for HTTP routes and OPA authorization. | **PASS** |

*Gate Status: ALL GATES PASSED.*

---

## Project Structure

### Documentation (this feature)

```text
specs/005-canvas-layout-aot-compilation/
├── spec.md              # Feature specification
├── plan.md              # This implementation plan
├── research.md          # Technical research & architectural decisions
├── data-model.md        # Domain entities, schemas, and persistence mapping
├── quickstart.md        # Local verification guide & test commands
├── contracts/
│   └── openapi-canvas.yaml # OpenAPI 3.1 contract excerpts
└── checklists/
    └── requirements.md  # Specification quality checklist
```

### Source Code Layout

```text
src/RestoCore.Domain/
├── Entities/
│   ├── Tenant.cs                                # Extended with LayoutConfig and CurrentVersionHash
│   └── MenuPublishJob.cs                        # Entity tracking AOT compilation runs
└── ValueObjects/
    ├── LayoutConfig.cs                          # Owned value object for canvas presentation
    └── CanvasElement.cs                         # Geometric coordinates (x, y, z_index, width, height)

src/RestoCore.Application/
├── Common/
│   └── Interfaces/
│       └── ICdnPurgeService.cs                  # Contract for CDN cache purging
└── Features/
    ├── MenuLayout/
    │   ├── Commands/
    │   │   ├── UpdateMenuLayoutCommand.cs       # Handler for PUT /api/v1/admin/menu/layout
    │   │   └── PublishMenuCommand.cs            # Handler for POST /api/v1/admin/menu/publish
    │   ├── DTOs/
    │   │   ├── LayoutConfigDto.cs
    │   │   ├── CanvasElementDto.cs
    │   │   ├── MenuPublishRequest.cs
    │   │   └── MenuPublishJobResponse.cs
    │   ├── Services/
    │   │   └── IMenuCompilationService.cs       # Pre-flattening and hash generation contract
    │   └── Validators/
    │       └── UpdateMenuLayoutValidator.cs     # FluentValidation for coordinates & dimensions
    └── PublicMenu/
        └── DTOs/
            └── PublicMenuResponse.cs            # Extended with LayoutConfigDto?

src/RestoCore.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs                  # DbSet<MenuPublishJob> registration
│   ├── Configurations/
│   │   ├── TenantConfiguration.cs               # JSONB mapping for LayoutConfig
│   │   └── MenuPublishJobConfiguration.cs       # Table configuration for menu_publish_jobs
│   └── Migrations/                              # EF Core migration for LayoutConfig & MenuPublishJobs
└── Services/
    ├── Cdn/
    │   └── LocalDevelopmentCdnPurgeService.cs   # Local dev/test CDN purge implementation
    └── Compilation/
        └── MenuCompilationService.cs            # AOT pre-flattening, orphan filtering & SHA256 hashing

src/RestoCore.Api/
└── Endpoints/
    ├── AdminMenuLayoutEndpoints.cs              # Minimal API routes for layout and publish
    └── PublicMenuEndpoints.cs                   # Updated with ETag evaluation and LayoutConfig delivery

tests/
├── RestoCore.UnitTests/
│   └── Features/
│       └── MenuLayout/
│           ├── UpdateMenuLayoutValidatorTests.cs
│           └── MenuCompilationServiceTests.cs
└── RestoCore.IntegrationTests/
    └── Endpoints/
        ├── AdminMenuLayoutEndpointsTests.cs
        ├── MenuPublishEndpointsTests.cs
        └── PublicMenuCanvasEndpointsTests.cs
```

---

## Complexity Tracking

No constitution violations detected. Standard Clean Architecture and EF Core JSONB mapping conventions are strictly maintained.