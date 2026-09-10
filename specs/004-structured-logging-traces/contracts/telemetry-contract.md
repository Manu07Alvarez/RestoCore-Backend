# Observability Contracts: Telemetry & Logging Protocol

**Feature**: `004-structured-logging-traces`
**Date**: 2026-09-09
**Status**: Ratified

## 1. OTLP Exporter Contract (OpenTelemetry Protocol)
The backend service exports all distributed telemetry via gRPC / HTTP using standard OTLP v1 specifications.

- **Protocol**: OTLP/gRPC over HTTP/2 or OTLP/HTTP over HTTP/1.1
- **Service Name Resource**: `RestoCore.Backend`
- **Default Endpoints**:
  - gRPC: `http://localhost:4317`
  - HTTP: `http://localhost:4318`

## 2. Injected W3C Trace Headers (Inbound & Outbound)
- `traceparent`: `00-{trace_id}-{span_id}-{trace_flags}`
  - Example: `00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01`
- `tracestate`: Optional vendor-specific state routing.

## 3. Standard Structured Log Message Contract
Application loggers MUST use structured parameter templates rather than string interpolation:

```csharp
// CORRECT: Structured template allows indexing and attribute extraction
logger.LogInformation(
    "HTTP {Method} {Path} completed with Status {StatusCode} in {ElapsedMs}ms for Tenant '{TenantSlug}'",
    method, path, statusCode, elapsedMs, tenantSlug);

// PROHIBITED: String interpolation destroys structured search and parameter indexing
logger.LogInformation($"HTTP {method} {path} completed with Status {statusCode}");
```

## 4. Log Levels Policy
- **Debug**: Detailed database queries, cache hits/misses, and internal routing (disabled in production).
- **Information**: High-level application events (server startup, tenant resolution, HTTP lifecycle completion).
- **Warning**: Expected non-critical issues (rate limit exceeded, invalid authorization tokens, cache timeouts).
- **Error**: Unhandled exceptions, infrastructure connection failures, OPA policy evaluation timeouts.
- **Critical**: System-wide failures leading to worker or service shutdown.
