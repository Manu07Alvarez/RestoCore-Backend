# Developer Quickstart: Containerized Test Environment & Controlled Verification

**Feature**: `002-docker-dev-testing`
**Dependencies**: Docker Desktop / Engine, .NET 10 SDK

---

## 1. Automated Verification Script

RestoCore includes an automated PowerShell verification script that checks Docker availability, starts all containerized services, waits for health readiness, applies EF Core database migrations, and executes all test suites:

```powershell
./scripts/verify-docker-env.ps1
```

---

## 2. Manual Step-by-Step Execution

If running individual steps manually, follow the sequence below:

### Step 2.1: Start Containerized Infrastructure

Spin up the complete development environment containing PostgreSQL, Redis, SeaweedFS, and OPA:

```bash
docker compose -f docker-compose.dev.yml up -d
```

Verify that all containers reach healthy status:
```bash
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

### Step 2.2: S3 Bucket Initialization

The `restocore-images` bucket is automatically provisioned via the `create-bucket` container defined in `docker-compose.dev.yml`. You can also verify bucket creation directly:
```bash
curl -i http://localhost:8333/restocore-images
```

### Step 2.3: Apply PostgreSQL Database Migrations

Apply EF Core migrations into the live PostgreSQL container:
```bash
dotnet ef database update --project src/RestoCore.Infrastructure/RestoCore.Infrastructure.csproj --startup-project src/RestoCore.Api/RestoCore.Api.csproj
```

### Step 2.4: Execute Automated Test Suites

```bash
# Unit Tests (Domain models, CQRS handlers, Validators)
dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj

# Integration Tests (Container readiness, Full E2E operations flow, SeaweedFS uploads)
dotnet test tests/RestoCore.IntegrationTests/RestoCore.IntegrationTests.csproj

# Security Tests (Live PostgreSQL multi-tenant isolation and query filter boundary tests)
dotnet test tests/RestoCore.SecurityTests/RestoCore.SecurityTests.csproj
```

---

## 3. Deep Health Check Verification

When the API is running locally:
- Liveness Probe: `GET http://localhost:5000/healthz`
- Readiness Probe (verifies PostgreSQL, Redis, SeaweedFS, and OPA): `GET http://localhost:5000/ready`

---

## 4. Teardown Environment

When testing is complete, stop containers and preserve volumes:
```bash
docker compose -f docker-compose.dev.yml down
```

Or purge volumes for a complete clean slate:
```bash
docker compose -f docker-compose.dev.yml down -v
```
