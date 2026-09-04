# Implementation Plan: Multi-Tenant Digital Menu & Operations Platform (QR-Based)

**Branch**: `001-multi-tenant-digital-menu` | **Date**: 2026-09-03 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-multi-tenant-digital-menu/spec.md`

---

## Summary

Implement the multi-tenant backend architecture for the RestoCore platform using C# 13 and .NET 9. The implementation decouples public anonymous diner menu queries from administrative CMS management and real-time kitchen stock operations. The design enforces strict data isolation using a `TenantId` discriminator column with Entity Framework Core Global Query Filters, cascading tenant resolution middleware, declarative authorization evaluated via Open Policy Agent (OPA) Rego policies, and distributed observability with OpenTelemetry.

---

## Technical Context

**Language/Version**: C# 13 / .NET 9.0 LTS

**Primary Dependencies**:
- ASP.NET Core 9 Minimal APIs
- Entity Framework Core 9 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- OpenTelemetry .NET SDK (`OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`)
- QRCoder & SkiaSharp (Vector SVG and raster PNG generation)
- FluentValidation & MediatR / FastEndpoints

**Storage**:
- Primary Database: PostgreSQL 16+ with `JSONB` columns and `GIN` indexes (`ADR-0003`)
- Cache & Stream Ingestion: Redis 7+ (`ADR-0007`)
- Asset Storage: SeaweedFS (direct client upload via pre-signed URLs, `ADR-0004`)

**Testing**:
- xUnit & FluentAssertions (Unit testing)
- Testcontainers for .NET (PostgreSQL, Redis) for integration and cross-tenant leak testing
- `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`)

**Target Platform**: Linux containers (Docker) / Cloud-native deployment

**Project Type**: REST Web API Service complying with OpenAPI 3.1.0

**Performance Goals**:
- Public menu response latency: < 150ms p95 under 500 concurrent requests per tenant
- Kitchen availability update propagation: < 500ms
- Vector QR generation: < 200ms

**Constraints**:
- Absolute cross-tenant data isolation (100% test pass rate on security suites)
- Dynamic requests processed in under 500ms server budget
- Zero emojis in documentation, specifications, or commits

**Scale/Scope**: Multi-tenant SaaS serving hundreds of restaurant tenants, thousands of concurrent dining sessions

---

## Constitution Check

*GATE: Passed prior to design phase. Re-evaluated post-design.*

- **Principle I (Spec-Driven Development):** PASS. Feature specification (`spec.md`) and OpenAPI 3.1 contract (`contracts/openapi.yaml`) established and versioned.
- **Principle II (Multi-Tenancy & Declarative Authorization):** PASS. Tenant context established via middleware, Global Query Filters applied, and OPA Rego policy handler decoupled from handlers.
- **Principle III (Clean Architecture & Domain Cohesion):** PASS. Layered structure (Domain, Application, Infrastructure, Api) adhering to the unified Domain Glossary.
- **Principle IV (Latency Budget & Asynchronous Decoupling):** PASS. Public menu optimized with ETag/Cache-Control headers, sub-150ms p95 latency target, SeaweedFS pre-signed URL workflow preserved.
- **Principle V (Relational & Flexible Persistence Discipline):** PASS. PostgreSQL with `JSONB` for theming/branding and modifiers with `GIN` indexing.
- **Principle VI (Test-Driven Quality & Reliability):** PASS. Complete test pyramid planned, including automated cross-tenant security test suites using Testcontainers.

---

## Project Structure

### Documentation (this feature)

```text
specs/001-multi-tenant-digital-menu/
├── spec.md              # Feature specification
├── plan.md              # Implementation plan (this document)
├── research.md          # Technical research & architecture decisions
├── data-model.md        # Entity definitions, schemas & indexes
├── quickstart.md        # Developer setup & execution guide
├── contracts/
│   └── openapi.yaml     # OpenAPI 3.1.0 contract
└── checklists/
    └── requirements.md  # Quality assurance validation checklist
```

### Source Code (repository root)

```text
src/
├── RestoCore.Domain/
│   ├── Entities/
│   │   ├── Tenant.cs
│   │   ├── Category.cs
│   │   ├── MenuItem.cs
│   │   ├── ModifierGroup.cs
│   │   ├── ModifierOption.cs
│   │   └── Table.cs
│   ├── Common/
│   │   ├── BaseEntity.cs
│   │   └── ITenantScopedEntity.cs
│   └── ValueObjects/
│       └── BrandingConfig.cs
│
├── RestoCore.Application/
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── IApplicationDbContext.cs
│   │   │   ├── ITenantContext.cs
│   │   │   └── IQrCodeService.cs
│   │   └── Behaviors/
│   ├── Features/
│   │   ├── PublicMenu/
│   │   │   └── Queries/GetPublicMenu/
│   │   ├── Categories/
│   │   ├── MenuItems/
│   │   ├── Kitchen/
│   │   │   └── Commands/ToggleItemAvailability/
│   │   ├── QrCodes/
│   │   │   └── Queries/GetTableQr/
│   │   └── Tenants/
│   │       └── Commands/ProvisionTenant/
│
├── RestoCore.Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/
│   │   └── Migrations/
│   ├── MultiTenancy/
│   │   ├── TenantContext.cs
│   │   └── TenantResolutionMiddleware.cs
│   ├── Authorization/
│   │   ├── OpaAuthorizationHandler.cs
│   │   └── OpaClient.cs
│   ├── Qr/
│   │   └── QrCodeService.cs
│   └── Telemetry/
│       └── OpenTelemetryExtensions.cs
│
└── RestoCore.Api/
    ├── Endpoints/
    │   ├── PublicMenuEndpoints.cs
    │   ├── KitchenEndpoints.cs
    │   ├── AdminCatalogEndpoints.cs
    │   └── AdminTenantEndpoints.cs
    ├── Middlewares/
    │   └── GlobalExceptionHandler.cs
    ├── Program.cs
    └── appsettings.json

tests/
├── RestoCore.UnitTests/
│   ├── Domain/
│   └── Features/
├── RestoCore.IntegrationTests/
│   ├── Endpoints/
│   └── Fixtures/
└── RestoCore.SecurityTests/
    └── MultiTenancy/
        └── CrossTenantIsolationTests.cs
```

**Structure Decision**: Clean Architecture with Vertical Slices within the Application layer (`Features/`). This ensures high cohesion per use case while keeping strict separation between Domain entities, Infrastructure concerns (PostgreSQL, OPA, Redis, QRCoder), and API transport.

---

## Complexity Tracking

| Violation / Complexity | Why Needed | Simpler Alternative Rejected Because |
| :--- | :--- | :--- |
| Testcontainers for Integration Tests | Eliminates differences between test mocks and actual PostgreSQL JSONB / GIN behavior | In-memory EF Core provider does not support JSONB operators, GIN indexes, or native SQL queries |
| Open Policy Agent (OPA) Integration | Mandatory adherence to ADR-0006 for declarative permission management across repos | Hardcoding `if (user.Role == ...)` in C# violates architectural governance and creates cross-repo policy drift |
