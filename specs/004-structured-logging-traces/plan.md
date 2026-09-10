# Implementation Plan: Structured Logging, Distributed Traces and Correlation

**Branch**: `004-structured-logging-traces` | **Date**: 2026-09-09 | **Spec**: [specs/004-structured-logging-traces/spec.md](spec.md)

## Summary

This plan formalizes and consolidates the OpenTelemetry-based structured logging and distributed tracing architecture in accordance with the `RestoCore-Docs` specifications (`AGENTS.md`, `observabilidad-telemetria.md`, `prd-menu-qr.md`, `diagnostico-observabilidad-incidentes.md`). The solution establishes strict separation between point-in-time structured log events and accumulated span durations, guarantees bidirectional W3C trace correlation (`trace_id` and `span_id`), enriches spans with multi-tenant context (`tenant.id`, `tenant.slug`), enforces an anti-redundancy logging policy, and masks sensitive data.

---

## Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**:
- `OpenTelemetry` (1.18.0)
- `OpenTelemetry.Extensions.Hosting` (1.18.0)
- `OpenTelemetry.Instrumentation.AspNetCore` (1.18.0)
- `OpenTelemetry.Instrumentation.Http` (1.18.0)
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` (1.18.0)
- `Npgsql.OpenTelemetry` (10.0.3)
- `Microsoft.Extensions.Logging` (.NET 10 built-in)
- `Microsoft.Extensions.Caching.StackExchangeRedis` (10.0.12)  

**Storage**: PostgreSQL (persistencia relacional y JSONB), Redis (caché distribuido de ETags y respuestas)  
**Testing**: k6 (Docker runner y scripts en `scripts/k6/`), xUnit / WebApplicationFactory (pruebas de integración)  
**Target Platform**: Linux / Windows container runtime (.NET 10 Kestrel)  
**Project Type**: Web Service (Modular Minimal APIs)  
**Performance Goals**:
- Presupuesto de latencia < 500ms p95 en servidor para peticiones dinámicas.
- Menos de 150ms p95 en respuestas 304 Not Modified con caché ETag en carta QR.
- Menos de 10 segundos MTTR para diagnosticar incidentes correlacionando trazas con logs.  
**Constraints**:
- Prohibición estricta de logs de inicio y fin de método para cronometrar duraciones (delegado a spans).
- Cero fugas de información personal identificable (PII) o credenciales en logs.
- Todo log debe correlacionar con `trace_id` y `span_id`.  
**Scale/Scope**: Multi-tenant isolation a nivel de middleware, spans de base de datos y logs estructurados.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Principle I: Spec-Driven Development (Spec-First)**: Feature spec created and validated in `specs/004-structured-logging-traces/spec.md`.
- [x] **Principle II: Multi-Tenancy and Declarative Authorization**: Enriched with `tenant.id` and `tenant.slug` in middleware and spans; authorization evaluated via OPA.
- [x] **Principle III: Clean Architecture and Domain Cohesion**: Telemetry extensions isolated in `RestoCore.Infrastructure/Telemetry`, logging middleware located in `RestoCore.Api`.
- [x] **Principle IV: Latency Budget & Asynchronous Decoupling**: Tracing audits latency budgets (< 500ms backend, < 2s LCP); anti-redundancy avoids logging overhead.
- [x] **Principle V: Relational & Flexible Persistence Discipline**: Npgsql spans instrumented to track query performance on PostgreSQL.
- [x] **Principle VI: Test-Driven Quality & Reliability**: k6 suite validates all endpoints and verifies latency thresholds under load.

---

## Project Structure

### Documentation (this feature)

```text
specs/004-structured-logging-traces/
├── spec.md              # Feature specification
├── plan.md              # This file (/speckit-plan output)
├── research.md          # Technical research and architecture decisions
├── data-model.md        # Telemetry schemas (Logs, Spans, ProblemDetails)
├── quickstart.md        # Operator diagnosis guide with Aspire Dashboard
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── contracts/
    └── telemetry-contract.md # OTLP exporter and structured logging contract
```

### Source Code (repository root)

```text
src/
├── RestoCore.Api/
│   ├── Program.cs                      # OpenTelemetry logging registration & structured request middleware
│   ├── Endpoints/                      # Minimal API route groups
│   └── Middleware/                     # TenantResolutionMiddleware with Activity enrichment
├── RestoCore.Application/              # Application features and handlers
└── RestoCore.Infrastructure/
    └── Telemetry/
        └── OpenTelemetryExtensions.cs  # OpenTelemetry Tracing, Metrics, and Logging configuration
```

**Structure Decision**: Clean architecture layout leveraging `RestoCore.Infrastructure/Telemetry` for telemetry setup, `RestoCore.Api` for middleware and endpoint handlers, and `scripts/k6` for load and stress validation.

---

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Ninguna | N/A | Cumplimiento total de principios constitucionales sin excepciones |
