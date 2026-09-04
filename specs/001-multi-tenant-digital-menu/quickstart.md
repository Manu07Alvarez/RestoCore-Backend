# Developer Quickstart: Multi-Tenant Digital Menu & Operations Platform

**Feature**: `001-multi-tenant-digital-menu`
**Runtime**: .NET 9 / C# 13

---

## 1. Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Docker Desktop or Podman (required for Testcontainers and local infrastructure)
- Git

---

## 2. Local Environment Setup

### 2.1. Start Infrastructure Services
The local environment requires PostgreSQL 16+, Redis, and Open Policy Agent (OPA). Start these services via Docker Compose:

```bash
docker compose -f docker-compose.dev.yml up -d
```

Verify service readiness:
- PostgreSQL: `localhost:5432` (database: `restocore_dev`, user: `postgres`, password: `postgres_dev_password`)
- Redis: `localhost:6379`
- OPA Policy Server: `localhost:8181`

### 2.2. Configure Environment Variables
Copy `.env.example` to `.env` or configure `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=restocore_dev;Username=postgres;Password=postgres_dev_password"
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "Opa": {
    "BaseUrl": "http://localhost:8181"
  },
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

---

## 3. Database Migration & Seeding

Apply Entity Framework Core migrations to initialize tables, JSONB columns, and GIN indexes:

```bash
dotnet ef database update --project src/Infrastructure/RestoCore.Infrastructure.csproj --startup-project src/Api/RestoCore.Api.csproj
```

---

## 4. Running the Backend API

Launch the API server locally:

```bash
dotnet run --project src/Api/RestoCore.Api.csproj
```

The application starts on `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS).
- Swagger / OpenAPI UI: `http://localhost:5000/swagger`
- Healthcheck Endpoint: `http://localhost:5000/healthz`

---

## 5. Automated Testing Strategy

Execute tests across the test pyramid:

### Unit Tests
Verify domain calculations, modifier validation, and entity logic:
```bash
dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj
```

### Multi-Tenant Isolation & Security Tests (Testcontainers)
Spins up isolated PostgreSQL containers and verifies that cross-tenant queries and mutations are blocked:
```bash
dotnet test tests/RestoCore.SecurityTests/RestoCore.SecurityTests.csproj
```

### Integration Tests
Validates complete HTTP endpoints, cascading middleware resolution, and OPA authorization policies:
```bash
dotnet test tests/RestoCore.IntegrationTests/RestoCore.IntegrationTests.csproj
```
