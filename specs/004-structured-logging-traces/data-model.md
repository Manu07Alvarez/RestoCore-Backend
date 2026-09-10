# Data Model: Structured Logs, Distributed Traces and Telemetry Attributes

**Feature**: `004-structured-logging-traces`
**Date**: 2026-09-09
**Status**: Completed

## 1. Overview
Defines the structure and schema for telemetry events (Logs, Spans, and Error Payloads) generated across the backend application, ensuring compliance with OpenTelemetry standards and RestoCore architecture guidelines.

---

## 2. Entities & Schemas

### 2.1. Structured Log Event (JSON Schema)
Represents a point-in-time discrete event emitted by application code or middleware.

```json
{
  "timestamp": "2026-09-09T22:30:00.1234567Z",
  "level": "Information | Warning | Error | Fatal",
  "message": "HTTP GET /api/v1/tenants/bodegon-0320458/menu completed with Status 200",
  "attributes": {
    "trace_id": "4bf92f3577b34da6a3ce929d0e0e4736",
    "span_id": "00f067aa0ba902b7",
    "tenant_id": "ddc4e69e-5e22-4e34-bb92-91a7a37ec55f",
    "tenant_slug": "bodegon-0320458",
    "http.method": "GET",
    "http.route": "/api/v1/tenants/{tenant_slug}/menu",
    "http.status_code": 200,
    "error.code": null
  }
}
```

#### Field Specifications:
- `timestamp`: String (ISO 8601 UTC). Required.
- `level`: LogLevel enum (`Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`). Required.
- `message`: Formatted string template. Sensitive values MUST NOT be interpolated directly.
- `trace_id`: Hexadecimal 32-character string conforming to W3C Trace Context. Required when active.
- `span_id`: Hexadecimal 16-character string conforming to W3C Trace Context. Required when active.
- `tenant_id`: UUID string of the active tenant. Present if request is resolved to a tenant.
- `tenant_slug`: Normalized URL slug of the active tenant.
- `http.status_code`: Integer response status code (e.g., 200, 304, 401, 500).
- `error.code`: High-level business error identifier (e.g., `TENANT_NOT_FOUND`, `UNAUTHORIZED_ACCESS`, `DATABASE_TIMEOUT`).

---

### 2.2. Distributed Trace Span
Represents a delimited unit of work executed within the distributed processing graph.

```text
Root Span: HTTP GET /api/v1/tenants/{tenant_slug}/menu
├── SpanId: 00f067aa0ba902b7
├── ParentSpanId: None
├── Duration: 42.31ms
├── Attributes:
│   ├── service.name: "RestoCore.Backend"
│   ├── tenant.id: "ddc4e69e-5e22-4e34-bb92-91a7a37ec55f"
│   ├── tenant.slug: "bodegon-0320458"
│   ├── http.method: "GET"
│   ├── http.status_code: 200
│   └── server.address: "localhost:5221"
└── Child Span: db.query (Npgsql / PostgreSQL)
    ├── SpanId: a4e9102c918341b5
    ├── ParentSpanId: 00f067aa0ba902b7
    ├── Duration: 12.5ms
    └── Attributes:
        ├── db.system: "postgresql"
        ├── db.name: "restocore_dev"
        └── db.statement: "SELECT t.* FROM Tenants ..."
```

---

### 2.3. Error Representation (ProblemDetails Correlation)
When an exception or failure occurs, the emitted `ProblemDetails` response MUST match the trace attributes for immediate operator correlation:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred while processing the request.",
  "instance": "/api/v1/tenants/bodegon-0320458/menu",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
  "errorCode": "INTERNAL_SERVER_ERROR"
}
```
