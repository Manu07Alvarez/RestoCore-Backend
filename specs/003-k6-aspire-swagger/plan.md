# Implementation Plan: Stress Testing (k6), Distributed Telemetry (Aspire Dashboard), and Development API Documentation (Swagger UI)

**Branch**: `003-k6-aspire-swagger` | **Date**: 2026-09-08 | **Spec**: [specs/003-k6-aspire-swagger/spec.md](specs/003-k6-aspire-swagger/spec.md)

**Input**: Feature specification from `/specs/003-k6-aspire-swagger/spec.md`

---

## Summary

This feature enhances RestoCore development and quality assurance by:
1. Enabling an interactive, secured **Swagger UI** with JWT Bearer support exclusively in `Development` mode for rapid frontend integration.
2. Integrating **.NET Aspire Dashboard** (`mcr.microsoft.com/dotnet/aspire-dashboard:latest`) into `docker-compose.dev.yml` to visualize OTLP distributed traces, metrics, and structured logs enriched with `tenant.id` tags in real time.
3. Establishing automated **k6** load and stress testing scripts in `scripts/k6/` with assertions verifying that public QR digital menu responses adhere to the latency budget (< 500ms dynamic, < 150ms p95 with ETag 304).

---

## Technical Context

**Language/Version**: C# / .NET 10 (ASP.NET Core Minimal APIs)
**Primary Dependencies**: `Swashbuckle.AspNetCore` (v10.2.3), `OpenTelemetry` (v1.18.0), `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `Npgsql.OpenTelemetry`, `mcr.microsoft.com/dotnet/aspire-dashboard:latest`, `grafana/k6:latest`
**Storage**: PostgreSQL 16 (`restocore-postgres-dev`), Redis 7 (`restocore-redis-dev`), SeaweedFS S3 (`restocore-seaweedfs-dev`)
**Testing**: xUnit, FluentAssertions, WebApplicationFactory, k6 JavaScript stress scripts
**Target Platform**: Docker Compose / Linux containers, Windows / Cross-platform .NET 10
**Project Type**: Multi-tenant REST API Web Service & Developer Tooling
**Performance Goals**: Public QR menu cached reads: p95 < 150ms; dynamic queries: < 500ms; error rate: < 1% under 50-100 concurrent VUs
**Constraints**: Zero binary transfers through memory; strict multi-tenant data isolation; no emojis in documentation or code

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Principle I: Spec-Driven Development**: Adheres to OpenAPI 3.1 contracts and formal feature spec in `specs/003-k6-aspire-swagger/spec.md`. (PASS)
- **Principle II: Multi-Tenancy and Declarative Authorization**: Telemetry strictly enriches traces and metrics with `tenant.id`; Swagger UI supports JWT Bearer tokens for OPA policy evaluation. (PASS)
- **Principle III: Clean Architecture & Domain Cohesion**: Telemetry instrumentation encapsulated in `RestoCore.Infrastructure/Telemetry`; Swagger configuration localized to `RestoCore.Api`. (PASS)
- **Principle IV: Latency Budget & Asynchronous Decoupling**: k6 stress tests programmatically enforce < 500ms dynamic and < 150ms p95 cached (ETag/304) latency budgets. (PASS)
- **Principle V: Relational & Flexible Persistence Discipline**: OpenTelemetry instruments Npgsql and EF Core database commands, capturing query durations and execution spans. (PASS)
- **Principle VI: Test-Driven Quality & Reliability**: Unit, integration, security, and load tests validated automatically. (PASS)

---

## Project Structure

### Documentation (this feature)

```text
specs/003-k6-aspire-swagger/
├── plan.md              # Implementation plan
├── research.md          # Technical analysis (Aspire Dashboard, Swagger UI, k6)
├── data-model.md        # Compose schemas, settings, and k6 threshold contracts
├── quickstart.md        # Developer execution guide
├── contracts/           # OpenAPI observability documentation contracts
└── tasks.md             # Sequenced task breakdown (generated via /speckit-tasks)
```

### Source Code & Tooling

```text
docker-compose.dev.yml                     # Service: aspire-dashboard (ports 18888, 4317, 4318)
scripts/
├── k6/
│   ├── public-menu-load.js                # Baseline load test
│   ├── public-menu-stress.js              # Ramp-up stress test (50-100 VUs)
│   └── etag-cache-test.js                 # ETag 304 performance test (<150ms p95)
└── run-stress-tests.ps1                   # Automated test runner (k6 CLI / Docker fallback)
src/
├── RestoCore.Api/
│   ├── Program.cs                         # Swagger UI with JWT Bearer configuration & conditional dev middleware
│   └── appsettings.Development.json       # OTLP endpoint configured to http://localhost:4317
└── RestoCore.Infrastructure/
    └── Telemetry/
        └── OpenTelemetryExtensions.cs     # OpenTelemetry tracing, metrics, logging, and tenant.id tag enricher
```
