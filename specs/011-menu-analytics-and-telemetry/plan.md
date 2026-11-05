# Implementation Plan: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard

**Branch**: `feature/011-menu-analytics-and-telemetry` | **Date**: 2026-11-04 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/011-menu-analytics-and-telemetry/spec.md`

## Summary

Implement the phased telemetry ingestion and analytics dashboard architecture defined in **ADR-0011** and **ADR-0012**:
1. **Asynchronous Telemetry Ingestion (`POST /api/v1/analytics/collect`)**: Public, non-blocking endpoint returning `HTTP 202 Accepted` in `< 50ms p95`, validating `event_type` (`menu_view`, `item_view`, `category_click`) and `tenant_slug` without collecting PII, and enqueuing events into `IAnalyticsEventQueue` processed by `AnalyticsAggregationWorker`.
2. **Executive Analytics & Menu Engineering Matrix (`GET /api/v1/admin/analytics/dashboard`)**: OPA-protected administrative endpoint (`OpaRequirement("read_metrics")`) returning `priority_high_metrics` (`total_qr_visits`, `monthly_qr_visits`, `top_viewed_items`, `top_categories`, and the 4-quadrant `menu_engineering_matrix` classifying active items into `stars`, `plowhorses`, `puzzles`, `dogs` using average views and contribution margin `BasePrice - EffectiveCost` with a 30% cost fallback) and conditional `priority_medium_metrics` (`peak_hours` 0..23 and `device_breakdown`) for Pro-tier tenants while omitting QR-derived table occupancy/turnover metrics per ADR-0012.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0  
**Primary Dependencies**: ASP.NET Core Minimal APIs, Entity Framework Core 10 (Npgsql), StackExchange.Redis / `System.Threading.Channels`, Open Policy Agent (OPA), xUnit, Moq, FluentAssertions  
**Storage**: PostgreSQL 16 (roll-up summary tables scoped by `TenantId` and `DateUtc`) + Redis 7 / in-memory bounded channel for non-blocking event buffering  
**Testing**: xUnit (`RestoCore.UnitTests`, `RestoCore.IntegrationTests`, `RestoCore.SecurityTests`)  
**Target Platform**: Linux / Windows Server containers (Docker)  
**Project Type**: Multi-tenant SaaS REST API Backend (Clean Architecture + CQRS)  
**Performance Goals**: `POST /api/v1/analytics/collect` `< 50ms p95` at 500 req/s (`SC-001`); `GET /api/v1/admin/analytics/dashboard` `< 300ms p95` over 30-day window (`SC-002`)  
**Constraints**: Zero PII stored (`FR-003`, `SC-004`); zero table dwell-time/turnover heuristics per ADR-0012 (`FR-009`); strict `TenantId` isolation (`FR-010`)  
**Scale/Scope**: Multi-tenant catalog up to 200 items per tenant, 1000 concurrent QR burst events

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate Evaluation | Status |
|-----------|-----------------|--------|
| **I. Spec-Driven Development** | Endpoints `POST /api/v1/analytics/collect` and `GET /api/v1/admin/analytics/dashboard` match `RestoCore-Docs/specs/openapi.yaml` and `specs/011-menu-analytics-and-telemetry/spec.md`. | PASS |
| **II. Multi-Tenancy & Declarative Authz** | All roll-up entities implement `ITenantScopedEntity` with Global Query Filters in `ApplicationDbContext`. Dashboard route enforces OPA policy `read_metrics` (`owner` / `tenant_admin`). | PASS |
| **III. Clean Architecture & Domain Cohesion** | Domain entities in `RestoCore.Domain`, CQRS commands/queries/validators in `RestoCore.Application`, EF Core repositories & background worker in `RestoCore.Infrastructure`, Minimal API endpoints in `RestoCore.Api`. | PASS |
| **IV. Latency Budget & Async Decoupling** | Telemetry ingestion responds `HTTP 202 Accepted` in `< 50ms` via asynchronous queueing and background roll-up aggregation. | PASS |
| **V. Relational & Flexible Persistence** | PostgreSQL stores indexed daily/hourly tenant roll-up tables with composite unique indexes on `(TenantId, DateUtc, ...)`. | PASS |
| **VI. Test-Driven Quality (TDD)** | Unit tests for matrix math/edge cases, ingestion validation, and Pro-tier gating + integration/security tests for HTTP 202/200/400/403 and cross-tenant isolation. | PASS |

## Project Structure

### Documentation (this feature)

```text
specs/011-menu-analytics-and-telemetry/
├── plan.md              # Implementation plan (/speckit-plan output)
├── research.md          # Phase 0 architectural & technical decisions
├── data-model.md        # Phase 1 domain & roll-up entity models
├── quickstart.md        # Phase 1 verification guide
├── contracts/
│   └── contracts.md     # Phase 1 OpenAPI 3.1 endpoint contracts
└── tasks.md             # Phase 2 task breakdown (/speckit-tasks output)
```

### Source Code (repository root)

```text
src/
├── RestoCore.Domain/
│   └── Entities/
│       ├── MenuItem.cs                              # Add EstimatedCost (decimal?)
│       ├── Tenant.cs                                # Add PlanTier (string)
│       ├── DailyMenuTrafficSummary.cs               # Daily QR visits & device counts
│       ├── DailyItemViewSummary.cs                  # Daily dish view counts
│       ├── DailyCategoryInteractionSummary.cs       # Daily category click counts
│       └── HourlyMenuTrafficSummary.cs              # Hourly visit counts (0..23)
├── RestoCore.Application/
│   ├── Common/Interfaces/
│   │   ├── IAnalyticsEventQueue.cs                  # Non-blocking event queue contract
│   │   └── Repositories/IAnalyticsRepository.cs     # Roll-up persistence & dashboard query contract
│   └── Features/Analytics/
│       ├── Commands/CollectAnalyticsEventCommand.cs # Validation & async enqueue handler
│       ├── Queries/GetAnalyticsDashboardQuery.cs    # Dashboard & Menu Engineering Matrix handler
│       └── DTOs/AnalyticsDtos.cs                    # OpenAPI-aligned request/response DTOs
├── RestoCore.Infrastructure/
│   ├── Messaging/AnalyticsEventQueue.cs             # Channel/Redis-backed event queue
│   ├── Workers/AnalyticsAggregationWorker.cs        # Background batch roll-up processor
│   └── Persistence/
│       ├── Configurations/AnalyticsConfiguration.cs # EF Core indexes & constraints
│       └── Repositories/AnalyticsRepository.cs      # EF Core roll-up queries & upserts
└── RestoCore.Api/
    └── Endpoints/
        └── AnalyticsEndpoints.cs                    # POST /api/v1/analytics/collect & GET /api/v1/admin/analytics/dashboard

tests/
├── RestoCore.UnitTests/
│   └── Features/Analytics/
│       ├── CollectAnalyticsEventCommandHandlerTests.cs
│       └── GetAnalyticsDashboardQueryHandlerTests.cs
├── RestoCore.IntegrationTests/
│   └── Endpoints/AnalyticsEndpointsTests.cs
└── RestoCore.SecurityTests/
    └── MultiTenancy/AnalyticsTenantIsolationTests.cs
```

**Structure Decision**: Follows the established 4-project Clean Architecture layout (`RestoCore.Domain`, `RestoCore.Application`, `RestoCore.Infrastructure`, `RestoCore.Api`) with 3 test suites under `tests/`.

## Complexity Tracking

No Constitution violations required.
