# Research & Technical Decisions: Stress Testing (k6), Aspire Dashboard (OTLP), and Swagger UI

**Feature**: `003-k6-aspire-swagger`
**Date**: 2026-09-08

---

## 1. Swagger UI & Interactive Documentation in ASP.NET Core (.NET 10)

### Context & Need
Frontend developers require interactive endpoint documentation with direct schema inspection and JWT Bearer authorization support for protected routes (Admin, Kitchen, Tenant management).

### Decision & Technical Strategy
- Utilize `Swashbuckle.AspNetCore` (already present in `src/RestoCore.Api/RestoCore.Api.csproj`, version `10.2.3`).
- Configure `AddSwaggerGen` with:
  - OpenAPI 3.0/3.1 document metadata (`RestoCore Backend API v1`).
  - `AddSecurityDefinition("Bearer", ...)` with standard `Bearer <JWT>` scheme.
  - `AddSecurityRequirement` ensuring lock icons appear next to protected endpoints (`TenantAdminOnly`, `KitchenOnly`, `SuperAdminOnly`).
  - XML documentation inclusion for parameter descriptions and error codes if generated.
- Restrict activation in HTTP pipeline strictly to `app.Environment.IsDevelopment()`:
  ```csharp
  if (app.Environment.IsDevelopment())
  {
      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
          c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestoCore API v1");
          c.RoutePrefix = "swagger";
      });
  }
  ```

---

## 2. Distributed Telemetry via .NET Aspire Dashboard in Docker Compose

### Context & Need
The platform requires an integrated, zero-friction visualization tool for OpenTelemetry distributed traces, metrics, and structured logs without setting up heavyweight distributed collectors (like full Prometheus + Grafana + Jaeger clusters) in local development.

### Container Configuration
- Image: `mcr.microsoft.com/dotnet/aspire-dashboard:latest`
- Ports:
  - `18888:18888` (Web UI)
  - `4317:4317` (OTLP gRPC receiver)
  - `4318:4318` (OTLP HTTP receiver)
- Environment Variables:
  - `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true` (Allows local development access without generating one-time login tokens).
  - `DASHBOARD__OTLP__AUTHMODE=Unsecured` (Accepts OTLP data without mutual TLS or API keys).

### OpenTelemetry Pipeline Enrichment
- In `RestoCore.Infrastructure/Telemetry/OpenTelemetryExtensions.cs`:
  - Enhance `AddRestoCoreTelemetry` to enrich traces with `tenant.id` using an `ActivityProcessor` or middleware enricher extracting the tenant ID from `ITenantContext`.
  - Add EF Core and HTTP client instrumentation (`AddHttpClientInstrumentation()`).
  - Add structured logging exporter: `builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter(...))` so that console and log messages correlate automatically with active trace IDs.

---

## 3. Stress & Performance Testing with k6

### Context & Need
Validate the architectural latency budgets (< 500ms for dynamic queries, < 150ms p95 for public QR menu with ETag 304) under concurrent load.

### Execution Strategy
- Install scripts in `scripts/k6/`:
  - `scripts/k6/public-menu-load.js`: Baseline load test (10-20 virtual users, 30s duration).
  - `scripts/k6/public-menu-stress.js`: Stress test ramping up to 50-100 VUs.
  - `scripts/k6/etag-cache-test.js`: Verifies `If-None-Match` returns 304 within 150ms p95.
- Provide a runner script `scripts/run-stress-tests.ps1` that executes k6 either natively (if `k6` CLI is in PATH) or via `docker run --rm -i --network host grafana/k6:latest run - < script.js`.
- Thresholds enforced:
  - `http_req_duration{status:304}: ["p(95)<150"]`
  - `http_req_duration{status:200}: ["p(95)<500"]`
  - `http_req_failed: ["rate<0.01"]`
