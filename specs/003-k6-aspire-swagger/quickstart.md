# Developer Quickstart: k6 Stress Testing, Aspire Dashboard, and Swagger UI

**Feature**: `003-k6-aspire-swagger`
**Dependencies**: Docker Desktop / Engine, .NET 10 SDK, (Optional: k6 local CLI)

---

## 1. Start Development Stack with Aspire Dashboard

Spin up the local containerized environment including PostgreSQL, Redis, SeaweedFS, OPA, and the **Aspire Dashboard**:

```bash
docker compose -f docker-compose.dev.yml up -d
```

Verify that all containers are healthy and reachable:
- **Aspire Dashboard Web UI**: `http://localhost:18888`
- **Aspire OTLP Receiver**: Port `4317` (mapped to container port `18889`)

---

## 2. Run the RestoCore Backend API

Start the backend API in `Development` mode:

```bash
dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj
```

Open Swagger UI in your browser:
- **URL**: `http://localhost:5000/swagger`
- Explore all API endpoint groups (`Public Menu`, `Catalog Management`, `Kitchen`, `Tables`, `Tenants`, `Storage`).
- Use the `Authorize` button to supply Bearer JWT tokens for protected endpoints.

---

## 3. Run Load & Stress Tests with k6

Execute the stress testing suite using the automated PowerShell runner (native CLI or automatic Docker fallback via `grafana/k6:latest`):

```powershell
# Baseline load test (20 virtual users)
./scripts/run-stress-tests.ps1 -Scenario public-menu-load

# ETag 304 validation test (< 150ms p95 latency budget)
./scripts/run-stress-tests.ps1 -Scenario etag-cache-test

# Sustained ramp-up stress test (up to 100 virtual users)
./scripts/run-stress-tests.ps1 -Scenario public-menu-stress
```

---

## 4. Observe Real-Time Telemetry

While requests are handled:
1. Open the **Aspire Dashboard** at `http://localhost:18888`.
2. Inspect the **Traces** tab to see live request flamegraphs, EF Core SQL execution spans, and the `tenant.id` / `tenant.slug` tags.
3. Inspect the **Metrics** tab to view `http.server.request.duration` percentiles (p50, p90, p95, p99) and verify they stay within the latency budget (< 500ms dynamic, < 150ms cached).
4. Inspect the **Structured Logs** tab to view correlated log statements alongside their trace IDs.
