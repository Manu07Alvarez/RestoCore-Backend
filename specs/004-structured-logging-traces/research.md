# Research: Observability, Structured Logging, Distributed Tracing and Correlation

**Feature**: `004-structured-logging-traces`
**Date**: 2026-09-09
**Status**: Completed

## 1. Context and Objective
Formalize the design and technical alignment of structured logging and distributed tracing according to the specifications established in `RestoCore-Docs` (`AGENTS.md`, `observabilidad-telemetria.md`, `prd-menu-qr.md`, `diagnostico-observabilidad-incidentes.md`).

---

## 2. Research Findings & Decisions

### Decision 1: Structured Logging Framework and Output Formatting
- **Decision**: Utilize Microsoft.Extensions.Logging integrated with OpenTelemetry Logging Provider (`OpenTelemetry.Logs` via `.WithLogging()`) exporting OTLP payloads directly to collectors (e.g., Aspire Dashboard / Grafana Loki / Jaeger).
- **Rationale**:
  - OpenTelemetry natively extracts and injects active W3C trace context (`trace_id` and `span_id`) into every log record emitted via `ILogger`.
  - Avoids proprietary logger wrappers by using standard structured message templates (e.g. `logger.LogInformation("... {TenantId} ...", ...)`), ensuring key-value attributes are serialized into JSON tags.
- **Alternatives Considered**:
  - *Serilog with explicit sinks*: More third-party dependencies without significant gain over standard .NET 10 OpenTelemetry ILogger integration.
  - *Unstructured Console text logs*: Rejected per Constitution (Principle V & VII) and RestoCore-Docs.

### Decision 2: Anti-Redundancy and Execution Boundary Logging
- **Decision**: Strictly prohibit method-level "Entering X" and "Exiting X in Y ms" logs. Use HTTP pipeline middleware for discrete request lifecycle boundaries and delegate internal elapsed time measurement exclusively to distributed spans.
- **Rationale**:
  - Method-level execution duration is an intrinsic property of a distributed trace span (`Activity.Duration`).
  - Emitting manual logs to measure durations bloats log aggregation, incurs high I/O overhead, and generates noise in incident resolution.
- **Alternatives Considered**:
  - *Logging start and stop in every handler/service*: Explicitly forbidden by RestoCore-Docs (`AGENTS.md` and `observabilidad-telemetria.md`).

### Decision 3: Context Enrichment with Multi-Tenancy (`tenant.id` & `tenant.slug`)
- **Decision**: Enrich the OpenTelemetry activity (`Activity.Current`) with `tenant.id` and `tenant.slug` tags during HTTP tenant resolution middleware, ensuring both spans and attached logs automatically inherit the tenant context.
- **Rationale**:
  - Operators need to filter logs and traces by `tenant_id` in Runbooks (`diagnostico-observabilidad-incidentes.md`).
  - By tagging `Activity.Current?.SetTag("tenant.id", tenantId)`, all downstream logs captured within that trace automatically carry tenant attributes in the OTLP resource/scope.
- **Alternatives Considered**:
  - *Manual parameter passing to every logger call*: Error-prone, leaks infrastructure into business domain logic.

### Decision 4: Sensitive Data Masking & PII Protection
- **Decision**: Implement log sanitization ensuring passwords, authorization tokens (JWT Bearer tokens), and personal identification details are omitted or redacted before emission.
- **Rationale**:
  - Inviolable security and privacy rule from RestoCore-Docs.
  - Public menu QR requests are anonymous by default; only tenant context is relevant.
- **Alternatives Considered**:
  - *Logging raw HTTP headers*: Dangerous and violates GDPR/compliance standards.

---

## 3. Technology Stack & Protocol
- **Transport / Protocol**: OTLP/gRPC (port 4317) and OTLP/HTTP (port 4318).
- **Format**: Protobuf / JSON structured attributes.
- **Standard**: W3C Trace Context (`traceparent`, `tracestate`).
- **Telemetry Collector in Dev**: .NET Aspire Dashboard (container `restocore-aspire-dashboard-dev`).
