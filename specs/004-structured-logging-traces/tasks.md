# Tasks: Structured Logging, Distributed Traces and Correlation

**Input**: Design documents from `specs/004-structured-logging-traces/` (`spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/telemetry-contract.md`, `quickstart.md`)

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: k6 stress & load scripts and automated endpoint validations included per Constitution Principle VI.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `- [ ] [ID] [P?] [Story?] Description with file path`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`)
- Exact file paths included in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verification and baseline configuration of telemetry and logging infrastructure

- [X] T001 Verify OTLP gRPC endpoint and port bindings in `docker-compose.dev.yml`
- [X] T002 Verify OpenTelemetry and Redis caching package dependencies in `src/RestoCore.Infrastructure/RestoCore.Infrastructure.csproj`
- [X] T003 [P] Configure OpenTelemetry service name and OTLP endpoint configuration settings in `src/RestoCore.Api/appsettings.Development.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core telemetry registration and context propagation that MUST be complete before user stories can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Setup OpenTelemetry Resource, Tracing, Metrics, and Logging exporters in `src/RestoCore.Infrastructure/Telemetry/OpenTelemetryExtensions.cs`
- [X] T005 [P] Register OpenTelemetry telemetry and logging pipeline in `src/RestoCore.Api/Program.cs`
- [X] T006 [P] Implement multi-tenant context tagging (`tenant.id`, `tenant.slug`) into active `Activity.Current` within `src/RestoCore.Infrastructure/MultiTenancy/TenantResolutionMiddleware.cs`

**Checkpoint**: Foundation ready - structured logs and distributed trace context propagation enabled.

---

## Phase 3: User Story 1 - Observabilidad y Diagnóstico Forense de Incidentes por Tenant (Priority: P1) 🎯 MVP

**Goal**: Deliver structured JSON logs containing timestamp, level, tenant identification, HTTP status, and error code for rapid root cause analysis.

**Independent Test**: Trigger operations and error conditions against API endpoints and verify structured log attributes (`tenant_id`, `http.status_code`, `error.code`) are correctly indexed and visible in Aspire Dashboard / OTLP collectors.

### Implementation for User Story 1

- [X] T007 [US1] Implement structured request execution logging middleware in `src/RestoCore.Api/Program.cs` capturing method, route, status code, and tenant identifiers
- [X] T008 [P] [US1] Integrate `tenant_id` and `error.code` structured attributes into `src/RestoCore.Api/Middlewares/GlobalExceptionHandler.cs`
- [X] T009 [US1] Ensure ProblemDetails payload returns matching `traceId` and `errorCode` in `src/RestoCore.Api/Middlewares/GlobalExceptionHandler.cs`
- [X] T010 [US1] Validate User Story 1 by executing API requests and verifying structured logs in Aspire Dashboard at `http://localhost:18888/structuredlogs`

**Checkpoint**: User Story 1 is fully functional and testable independently.

---

## Phase 4: User Story 2 - Salto Bidireccional entre Métricas de Latencia y Causa Raíz mediante Trazas (Priority: P2)

**Goal**: Guarantee bidirectional W3C trace context correlation (`trace_id`, `span_id`) across ASP.NET Core request pipeline and database spans, enabling instant navigation from latency graphs to root cause logs.

**Independent Test**: Execute k6 load tests and inspect distributed traces in Aspire Dashboard, ensuring every trace correlates with its child spans (PostgreSQL / Npgsql) and associated log records.

### Tests for User Story 2

- [X] T011 [P] [US2] Create k6 all-endpoints test script exercising public and admin routes in `scripts/k6/all-endpoints-test.js`
- [X] T012 [P] [US2] Update test runner script `scripts/run-stress-tests.ps1` to support `-Scenario all-endpoints-test`

### Implementation for User Story 2

- [X] T013 [US2] Configure Npgsql distributed tracing instrumentation with SQL query telemetry in `src/RestoCore.Infrastructure/Telemetry/OpenTelemetryExtensions.cs`
- [X] T014 [US2] Verify W3C trace context header propagation (`traceparent`) in incoming HTTP requests in `src/RestoCore.Api/Program.cs`
- [X] T015 [US2] Execute k6 test suite `scripts/run-stress-tests.ps1 -Scenario all-endpoints-test` and verify trace latency p95 < 500ms

**Checkpoint**: User Stories 1 and 2 work cohesively with full trace-to-log correlation.

---

## Phase 5: User Story 3 - Cumplimiento de Políticas Anti-Redundancia y Protección de Privacidad (Priority: P3)

**Goal**: Enforce strict anti-redundancy rules (prohibit method-level stopwatch logs; delegate duration to spans) and ensure zero PII/credential leakage.

**Independent Test**: Codebase audit verifying absence of trivial start/end method logs and automated inspection confirming no passwords, Bearer tokens, or customer personal details are logged.

### Implementation for User Story 3

- [X] T016 [P] [US3] Audit application handlers in `src/RestoCore.Application/` to verify removal of redundant method-level duration logs
- [X] T017 [P] [US3] Implement sensitive data sanitization (redacting `Authorization` header and authentication payloads) in `src/RestoCore.Api/Program.cs`
- [X] T018 [US3] Perform static verification confirming compliance with telemetry privacy rules defined in `specs/004-structured-logging-traces/spec.md`

**Checkpoint**: All three user stories are functionally and architecturally verified.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, quickstart verification, and operational runbook alignment

- [X] T019 [P] Update diagnostic runbook and quickstart verification in `specs/004-structured-logging-traces/quickstart.md`
- [X] T020 Run full regression verification using `scripts/run-stress-tests.ps1` across all scenarios (`public-menu-load`, `etag-cache-test`, `all-endpoints-test`)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately.
- **Foundational (Phase 2)**: Depends on Phase 1 completion - blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Phase 2 - delivers core MVP.
- **User Story 2 (Phase 4)**: Depends on Phase 2; builds on Phase 3 logging middleware.
- **User Story 3 (Phase 5)**: Depends on Phase 3 and 4 completion.
- **Polish (Phase 6)**: Depends on all user stories completion.

### Parallel Opportunities

- T003 (appsettings configuration) can run in parallel with T001 and T002.
- T005 and T006 can run in parallel once T004 is defined.
- T008 can be developed in parallel with T007.
- T011 and T012 can be developed in parallel.
- T016 and T017 can be audited in parallel.

---

## Implementation Strategy

### MVP First (User Story 1 Only)
1. Complete Phase 1 (Setup) and Phase 2 (Foundational).
2. Complete Phase 3 (User Story 1 - Structured Logging by Tenant).
3. Validate structured logs in Aspire Dashboard at `http://localhost:18888/structuredlogs`.

### Incremental Delivery
1. Foundation & US1 -> Structured logs with tenant ID & status codes (MVP).
2. User Story 2 -> End-to-end W3C tracing, Npgsql spans, and k6 validation.
3. User Story 3 -> Anti-redundancy audit & PII protection.
4. Phase 6 -> Full stress suite execution and documentation ratification.
