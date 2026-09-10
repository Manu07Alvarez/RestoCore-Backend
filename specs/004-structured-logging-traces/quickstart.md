# Quickstart: Diagnosing Incidents with Aspire Dashboard & OpenTelemetry

**Feature**: `004-structured-logging-traces`
**Date**: 2026-09-09

## 1. Prerequisites
Ensure development containers are active:

```powershell
docker-compose -f docker-compose.dev.yml up -d
```

Verify that `restocore-aspire-dashboard-dev` is listening on:
- Web Dashboard UI: `http://localhost:18888`
- OTLP gRPC endpoint: `http://localhost:4317`

---

## 2. Running Backend with Telemetry
Start the API locally:

```powershell
dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj
```

The API will automatically send distributed traces, metrics, and structured logs to the Aspire Dashboard.

---

## 3. Correlating Latency & Logs in Aspire Dashboard

1. Open your browser at `http://localhost:18888`.
2. Navigate to **Traces**:
   - Locate slow HTTP requests (e.g., requests to `/api/v1/tenants/{tenant_slug}/menu`).
   - Click on the trace to view the breakdown of spans (ASP.NET Core request, Npgsql database query).
   - Copy the `TraceId` (e.g. `4bf92f3577b34da6a3ce929d0e0e4736`).
3. Navigate to **Structured Logs**:
   - Filter by `TraceId` or search by `tenant_slug` / `tenant_id`.
   - Inspect the structured properties: `http.status_code`, `ElapsedMs`, `error.code`.
   - Click on a log entry to jump directly to its associated trace span or view full exception details.
