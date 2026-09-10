# Feature Specification: Stress Testing (k6), Distributed Telemetry (Aspire Dashboard), and Development API Documentation (Swagger UI)

**Feature Branch**: `003-k6-aspire-swagger`
**Created**: 2026-09-08
**Status**: Draft
**Input**: User description: "Implementar suite de pruebas de carga y estrés con k6 para validar los presupuestos de latencia (<500ms en endpoints dinamicos y <150ms p95 en carta QR publica con soporte de ETag), integrar .NET Aspire Dashboard en docker-compose.dev.yml como visor OTLP en tiempo real para telemetria distribuida (trazas con tenant.id, metricas y logs estructurados), y habilitar interfaz interactiva Swagger UI / OpenAPI en ambiente de desarrollo para facilitar la integracion del equipo frontend."

---

## User Scenarios & Testing

### User Story 1 - Interactive API Exploration & Frontend Development Integration (Priority: P1)

As a frontend developer integrating with RestoCore, I want to access an interactive API explorer (Swagger UI) in the local development environment so that I can inspect schemas, test endpoints with JWT authentication, and accelerate UI implementation without guessing contracts.

**Why this priority**: Directly unblocks the frontend team by providing immediate interactive contract visibility for all catalog, table, storage, and QR endpoints.

**Independent Test**:
Launch the API in `Development` environment (`ASPNETCORE_ENVIRONMENT=Development`), navigate to `/swagger` (or root explorer endpoint), verify that OpenAPI schemas load correctly, and successfully execute a request through the UI using a Bearer token.

**Acceptance Scenarios**:
1. **Given** the application runs with `ASPNETCORE_ENVIRONMENT=Development`, **When** a developer navigates to `/swagger` or `/swagger/index.html`, **Then** the Swagger UI is rendered displaying all v1 API endpoints grouped by tag (Admin, Public, Kitchen, Storage, Health).
2. **Given** a developer configures an Authorization Bearer token in the Swagger UI `Authorize` dialog, **When** they execute a protected endpoint (e.g., `POST /api/v1/admin/categories`), **Then** the token is included in the request header and the API processes the authenticated command.
3. **Given** the application runs in `Production` environment, **When** any client requests `/swagger`, **Then** the Swagger UI is disabled and returns 404 Not Found.

---

### User Story 2 - Real-Time Distributed Telemetry via .NET Aspire Dashboard (Priority: P1)

As a backend platform engineer, I want to visualize distributed traces, OpenTelemetry metrics, and structured logs inside the .NET Aspire Dashboard in real time within Docker Compose so that I can inspect query latencies, trace requests across middleware, and verify tenant tag enrichment.

**Why this priority**: Fulfills the core observability requirement and architectural inviolable for distributed tracing, allowing real-time profiling during development and load testing.

**Independent Test**:
Start the stack with `docker compose -f docker-compose.dev.yml up -d`, navigate to the Aspire Dashboard at `http://localhost:18888`, trigger API operations (e.g. `GET /api/v1/public/menus/{tenantSlug}`), and confirm that traces, spans with `tenant.id` attribute, and logs appear on the dashboard.

**Acceptance Scenarios**:
1. **Given** the `aspire-dashboard` service is configured in `docker-compose.dev.yml`, **When** the container stack boots up, **Then** the Aspire Dashboard is accessible on port `18888` without requiring authentication in development mode.
2. **Given** the API backend is configured to export OTLP telemetry to the Aspire Dashboard OTLP endpoint (port 4317 gRPC or 4318 HTTP), **When** an incoming request is handled, **Then** a distributed trace is captured containing HTTP status, route, EF Core database execution spans, and the `tenant.id` tag.
3. **Given** an error or exception occurs in the API, **When** viewing the logs tab in Aspire Dashboard, **Then** the structured log entry is displayed with its exception details and correlated trace ID.

---

### User Story 3 - Load & Stress Testing with k6 (Priority: P2)

As a performance engineer and QA lead, I want to execute automated k6 load and stress tests against the live API stack so that I can validate that public digital menu reads meet the strict latency budget (p95 < 150ms with ETag 304, dynamic responses < 500ms) under sustained concurrency.

**Why this priority**: Validates the architectural latency budget defined in the project constitution under controlled stress conditions before release.

**Independent Test**:
Run `k6 run scripts/k6/public-menu-stress.js` (or via Docker container `grafana/k6`) against the running environment, generating concurrent virtual users (VUs), and verify that latency thresholds (`p(95) < 150ms` for cached 304, `p(95) < 500ms` for dynamic 200) pass successfully.

**Acceptance Scenarios**:
1. **Given** a populated menu catalog for a tenant, **When** k6 simulates 50 concurrent virtual users requesting `GET /api/v1/public/menus/{tenantSlug}` with cached ETag (`If-None-Match`), **Then** 100% of responses return `304 Not Modified` with p95 latency under 150ms.
2. **Given** a high concurrency stress test simulating ramp-up to 100+ virtual users, **When** k6 queries dynamic menu endpoints, **Then** the server response time remains under 500ms for 95% of requests and error rate remains below 1%.
3. **Given** the stress test is executing, **When** observing the Aspire Dashboard, **Then** live request rate, duration metrics, and trace flamegraphs reflect the load profile in real time.

---

### Edge Cases

- How does Swagger UI handle complex `JSONB` properties like `BrandingConfig` and `ModifierOption` lists? Swagger schemas must provide example payloads and property descriptions.
- What happens if the Aspire Dashboard container is offline or restarting when the API starts? The OpenTelemetry OTLP exporter in the API must handle transient connection failures gracefully without crashing the backend process.
- What happens if k6 hits a non-existent tenant slug during a stress test? The API must consistently return `404 Not Found` with standard `ProblemDetails` within the latency budget without memory leaks.
- How does the system handle high concurrency cold-cache requests versus warm-cache requests? Cold requests must complete in under 500ms; subsequent requests utilizing ETags must respond with `304 Not Modified` in under 150ms.

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST enable Swagger UI (`Swashbuckle.AspNetCore` or ASP.NET Core OpenAPI built-in explorer) exclusively when running in `Development` environment at `/swagger`.
- **FR-002**: Swagger UI MUST include JWT Bearer authorization configuration allowing developers to input bearer tokens for protected endpoints.
- **FR-003**: System MUST configure .NET Aspire Dashboard in `docker-compose.dev.yml` using the official container image (`mcr.microsoft.com/dotnet/aspire-dashboard:latest` or stable tag), exposing UI on port 18888 and OTLP on ports 4317 (gRPC) and 4318 (HTTP).
- **FR-004**: System MUST configure OpenTelemetry in `RestoCore.Infrastructure` / `RestoCore.Api` to export traces, metrics, and structured logs via OTLP to the Aspire Dashboard endpoint in development mode.
- **FR-005**: All exported traces and metrics MUST be enriched with `tenant.id`, `service.name=RestoCore.Api`, and relevant operational tags.
- **FR-006**: System MUST provide modular k6 stress testing scripts in `scripts/k6/` covering:
  - Base load test: steady state concurrency.
  - Stress test: ramp-up to peak load to find saturation point.
  - ETag cache validation test: verifying `304 Not Modified` performance budget.
- **FR-007**: k6 test scripts MUST define programmatic thresholds asserting `http_req_duration{status:304}: p(95) < 150ms`, `http_req_duration{status:200}: p(95) < 500ms`, and `http_req_failed: rate < 0.01`.
- **FR-008**: System MUST provide a unified command or script to execute k6 tests either locally or via a transient Docker container (`docker run --rm -i grafana/k6`).

### Key Entities

- **TelemetryProfile**: Configuration parameters defining OTLP endpoint URI, export protocol (gRPC/HTTP), and instrumentation filters.
- **StressScenario**: k6 test specification containing stages, virtual users (VUs), duration, target URLs, and assertion thresholds.

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Swagger UI loads in the browser at `http://localhost:5000/swagger` in under 1.5 seconds in Development mode, listing 100% of defined API endpoints.
- **SC-002**: .NET Aspire Dashboard displays live traces, metrics, and structured logs within 2 seconds of API request execution.
- **SC-003**: 100% of traces for tenant-scoped operations contain the `tenant.id` resource/span attribute in the Aspire Dashboard trace viewer.
- **SC-004**: k6 stress test confirms that cached public menu requests (`ETag` / 304) maintain p95 latency under 150ms under at least 50 concurrent virtual users.
- **SC-005**: k6 stress test confirms that dynamic requests maintain p95 latency under 500ms with an error rate of 0% under standard load conditions.

---

## Assumptions

- k6 can be executed either via locally installed k6 binary or via the official `grafana/k6` Docker container without requiring local installation.
- Aspire Dashboard runs in development mode without mandatory authentication tokens (`DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true`) for developer convenience in local docker-compose.
- ASP.NET Core Minimal APIs leverage existing Swagger / OpenAPI tooling compatible with .NET 10.
