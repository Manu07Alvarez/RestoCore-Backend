---

description: "Task list for Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard (Feature 011)"
---

# Tasks: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard

**Input**: Design documents from `/specs/011-menu-analytics-and-telemetry/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/contracts.md`

**Tests**: Included per Constitution Principle VI (Test-Driven Quality) and `SC-003`/`SC-005` in `spec.md`.

**Organization**: Tasks are grouped by user story (`US1`, `US2`, `US3`) to enable independent implementation and testing of each increment.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (`[US1]`, `[US2]`, `[US3]`)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Extend existing domain models and create shared analytics DTO structures.

- [X] T001 [P] Add `EstimatedCost` (`decimal?`) property to `MenuItem` entity in `src/RestoCore.Domain/Entities/MenuItem.cs`
- [X] T002 [P] Add `PlanTier` (`string`, default `"Standard"`) property to `Tenant` entity in `src/RestoCore.Domain/Entities/Tenant.cs`
- [X] T003 [P] Create OpenAPI-aligned request, response, and queue DTOs (`AnalyticsEventRequest`, `AnalyticsEventMessage`, `AnalyticsDashboardResponse`, `PriorityHighMetricsDto`, `MenuEngineeringMatrixDto`, `PriorityMediumMetricsDto`) in `src/RestoCore.Application/Features/Analytics/DTOs/AnalyticsDtos.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core persistence roll-up entities, EF Core configurations, repository contracts, and authorization policy required before user stories can execute.

**CRITICAL**: No user story work can begin until this phase is complete.

- [X] T004 [P] Create `DailyMenuTrafficSummary` entity implementing `ITenantScopedEntity` in `src/RestoCore.Domain/Entities/DailyMenuTrafficSummary.cs`
- [X] T005 [P] Create `DailyItemViewSummary` entity implementing `ITenantScopedEntity` in `src/RestoCore.Domain/Entities/DailyItemViewSummary.cs`
- [X] T006 [P] Create `DailyCategoryInteractionSummary` entity implementing `ITenantScopedEntity` in `src/RestoCore.Domain/Entities/DailyCategoryInteractionSummary.cs`
- [X] T007 [P] Create `HourlyMenuTrafficSummary` entity implementing `ITenantScopedEntity` in `src/RestoCore.Domain/Entities/HourlyMenuTrafficSummary.cs`
- [X] T008 Create EF Core entity configuration (`AnalyticsConfiguration.cs`) with composite unique indexes on `(TenantId, DateUtc, ...)` and update `MenuConfiguration.cs` / `TenantConfiguration.cs` in `src/RestoCore.Infrastructure/Persistence/Configurations/AnalyticsConfiguration.cs`
- [X] T009 Register `DbSet` properties and Global Query Filters for analytics roll-up entities in `src/RestoCore.Infrastructure/Persistence/ApplicationDbContext.cs` and `src/RestoCore.Application/Common/Interfaces/IApplicationDbContext.cs`
- [X] T010 [P] Define `IAnalyticsEventQueue` interface in `src/RestoCore.Application/Common/Interfaces/IAnalyticsEventQueue.cs`
- [X] T011 [P] Define `IAnalyticsRepository` interface in `src/RestoCore.Application/Common/Interfaces/Repositories/IAnalyticsRepository.cs`
- [X] T012 Implement `AnalyticsRepository` in `src/RestoCore.Infrastructure/Persistence/Repositories/AnalyticsRepository.cs`
- [X] T013 Register `AnalyticsReadOnly` authorization policy (`new OpaRequirement("read_metrics")`) and analytics services in `src/RestoCore.Api/Program.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 - Asynchronous Ingestion of Diner Interaction Events (Priority: P1) - MVP

**Goal**: Expose `POST /api/v1/analytics/collect` accepting `menu_view`, `item_view`, and `category_click` events, responding with `HTTP 202 Accepted` in `< 50ms p95` without storing PII, and aggregating counters asynchronously in the background.

**Independent Test**: Send valid and invalid `POST /api/v1/analytics/collect` requests; verify `202 Accepted` for valid events, `400 Bad Request` for malformed inputs, asynchronous roll-up updates via `AnalyticsAggregationWorker`, and zero PII persisted.

### Tests for User Story 1

- [X] T014 [P] [US1] Write unit tests for `CollectAnalyticsEventCommandHandler` validation and non-blocking enqueue behavior in `tests/RestoCore.UnitTests/Features/Analytics/CollectAnalyticsEventCommandHandlerTests.cs`
- [X] T015 [P] [US1] Write integration tests for `POST /api/v1/analytics/collect` (`202 Accepted` and `400 Bad Request` cases) in `tests/RestoCore.IntegrationTests/Endpoints/AnalyticsEndpointsTests.cs`

### Implementation for User Story 1

- [X] T016 [P] [US1] Implement `AnalyticsEventQueue` using bounded `System.Threading.Channels.Channel<AnalyticsEventMessage>` in `src/RestoCore.Infrastructure/Messaging/AnalyticsEventQueue.cs`
- [X] T017 [US1] Implement `CollectAnalyticsEventCommand` and `CollectAnalyticsEventCommandHandler` (validating `event_type`, `tenant_slug`, `item_id`/`category_id`, and `device_type`) in `src/RestoCore.Application/Features/Analytics/Commands/CollectAnalyticsEventCommand.cs`
- [X] T018 [US1] Implement `AnalyticsAggregationWorker` background service to consume queued events and increment daily/hourly roll-ups in `src/RestoCore.Infrastructure/Workers/AnalyticsAggregationWorker.cs`
- [X] T019 [US1] Expose `POST /api/v1/analytics/collect` endpoint in `src/RestoCore.Api/Endpoints/AnalyticsEndpoints.cs` and map it in `src/RestoCore.Api/Program.cs`

**Checkpoint**: User Story 1 is functional and independently testable.

---

## Phase 4: User Story 2 - Executive Analytics Dashboard & Menu Engineering Matrix (Priority: P2)

**Goal**: Expose `GET /api/v1/admin/analytics/dashboard` secured by JWT and OPA (`read_metrics`), returning `priority_high_metrics` (`total_qr_visits`, `monthly_qr_visits`, `top_viewed_items`, `top_categories`, and the 4-quadrant `menu_engineering_matrix` with 30% default cost fallback and deterministic edge-case handling).

**Independent Test**: Authenticate as `owner` or `tenant_admin` and query `GET /api/v1/admin/analytics/dashboard?from=...&to=...`; verify `200 OK` with accurate quadrant classifications (`stars`, `plowhorses`, `puzzles`, `dogs`), soft-deleted item exclusion from active matrix, and `403 Forbidden` for `cook`/`waiter`.

### Tests for User Story 2

- [X] T020 [P] [US2] Write unit tests for `GetAnalyticsDashboardQueryHandler` covering Menu Engineering Matrix classification, 30% cost fallback, zero-view catalogs, uniform-variance catalogs, and soft-deleted dish handling in `tests/RestoCore.UnitTests/Features/Analytics/GetAnalyticsDashboardQueryHandlerTests.cs`
- [X] T021 [P] [US2] Write multi-tenant isolation security tests for analytics roll-ups and dashboard queries in `tests/RestoCore.SecurityTests/MultiTenancy/AnalyticsTenantIsolationTests.cs`

### Implementation for User Story 2

- [X] T022 [US2] Implement `GetAnalyticsDashboardQuery` and `GetAnalyticsDashboardQueryHandler` computing `priority_high_metrics` and `menu_engineering_matrix` in `src/RestoCore.Application/Features/Analytics/Queries/GetAnalyticsDashboardQuery.cs`
- [X] T023 [US2] Expose `GET /api/v1/admin/analytics/dashboard` protected by `AnalyticsReadOnly` policy in `src/RestoCore.Api/Endpoints/AnalyticsEndpoints.cs`

**Checkpoint**: User Stories 1 and 2 are both functional and independently testable.

---

## Phase 5: User Story 3 - Temporal Demand Curve & Device Distribution for Pro Tenants (Priority: P3)

**Goal**: Populate `priority_medium_metrics` (`peak_hours` 0..23 and `device_breakdown` percentages) when `Tenant.PlanTier` is `"Pro"` or `"Enterprise"`, return `null` for `"Standard"` tier tenants, and strictly omit QR table occupancy/turnover metrics per ADR-0012.

**Independent Test**: Query `GET /api/v1/admin/analytics/dashboard` for both a `"Pro"` tenant and a `"Standard"` tenant; verify `priority_medium_metrics` is populated only for `"Pro"` and contains zero table dwell/turnover fields.

### Tests for User Story 3

- [X] T024 [P] [US3] Add unit tests in `tests/RestoCore.UnitTests/Features/Analytics/GetAnalyticsDashboardQueryHandlerTests.cs` verifying `priority_medium_metrics` population for Pro tenants, `null` for Standard tenants, and ADR-0012 compliance
- [X] T025 [P] [US3] Add integration tests in `tests/RestoCore.IntegrationTests/Endpoints/AnalyticsEndpointsTests.cs` for `GET /api/v1/admin/analytics/dashboard` across Pro/Standard tiers and OPA role authorization

### Implementation for User Story 3

- [X] T026 [US3] Extend `GetAnalyticsDashboardQueryHandler` in `src/RestoCore.Application/Features/Analytics/Queries/GetAnalyticsDashboardQuery.cs` to compute `peak_hours` and `device_breakdown` when `Tenant.PlanTier` is `"Pro"` or `"Enterprise"`
- [X] T027 [US3] Update `DatabaseSeeder` in `src/RestoCore.Infrastructure/Persistence/Seed/DatabaseSeeder.cs` to seed sample `EstimatedCost`, `PlanTier = "Pro"`, and baseline analytics roll-up data for local verification

**Checkpoint**: All user stories (`US1`, `US2`, `US3`) are independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: End-to-end verification, documentation sync check, and knowledge graph update.

- [X] T028 Run `.agents/skills/sync-docs-specs/scripts/compare-docs.ps1` to confirm 100% endpoint coverage for `POST /api/v1/analytics/collect` and `GET /api/v1/admin/analytics/dashboard`
- [X] T029 Execute full unit, security, and integration test suites via `dotnet test RestoCore.slnx` and validate `specs/011-menu-analytics-and-telemetry/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 — blocks all user stories.
- **Phase 3 (`US1` - P1)**: Depends on Phase 2.
- **Phase 4 (`US2` - P2)**: Depends on Phase 2 (can run in parallel with `US1` or sequentially).
- **Phase 5 (`US3` - P3)**: Depends on Phase 4 (`GetAnalyticsDashboardQueryHandler`).
- **Phase 6 (Polish)**: Depends on Phases 3, 4, and 5 completion.

### Parallel Opportunities

- **Phase 1**: `T001`, `T002`, and `T003` touch distinct files and can run in parallel.
- **Phase 2**: `T004`, `T005`, `T006`, `T007`, `T010`, and `T011` can run in parallel before `T008`/`T009`/`T012`.
- **Phase 3 (`US1`)**: `T014`, `T015`, and `T016` can run in parallel.
- **Phase 4 (`US2`)**: `T020` and `T021` can run in parallel.
- **Phase 5 (`US3`)**: `T024` and `T025` can run in parallel.

---

## Parallel Example: User Story 1

```bash
# Run US1 tests and queue implementation in parallel:
Task T014: "Write unit tests for CollectAnalyticsEventCommandHandler in tests/RestoCore.UnitTests/Features/Analytics/CollectAnalyticsEventCommandHandlerTests.cs"
Task T015: "Write integration tests for POST /api/v1/analytics/collect in tests/RestoCore.IntegrationTests/Endpoints/AnalyticsEndpointsTests.cs"
Task T016: "Implement AnalyticsEventQueue in src/RestoCore.Infrastructure/Messaging/AnalyticsEventQueue.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)
1. Complete Phase 1 (Setup) and Phase 2 (Foundational).
2. Complete Phase 3 (`US1`: `POST /api/v1/analytics/collect` + `AnalyticsAggregationWorker`).
3. Validate `< 50ms` `HTTP 202 Accepted` ingestion independently.

### Incremental Delivery
1. Deliver `US1` (Asynchronous Telemetry Ingestion).
2. Deliver `US2` (Executive Dashboard & Menu Engineering Matrix).
3. Deliver `US3` (Pro Plan Temporal Demand & Device Breakdown).
