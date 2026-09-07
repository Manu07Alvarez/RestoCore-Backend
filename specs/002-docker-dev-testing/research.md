# Phase 0 Research: Containerized Test Environment & Controlled Verification

**Feature**: `002-docker-dev-testing`
**Status**: Completed

---

## 1. Research Questions & Decisions

### Decision 1: SeaweedFS Container Topology and Configuration
- **Context**: ADR-0004 defines SeaweedFS as the object storage engine. For local development and end-to-end testing, we need Master, Volume, and an S3-compatible API gateway.
- **Decision**: Use `chrislusf/seaweedfs:latest` in Docker Compose configured with `server -s3 -filer` mode.
- **Rationale**:
  - `weed server -s3` boots Master, Volume, Filer, and the S3 gateway in a single lightweight container, exposing:
    - Port `8888`: Filer UI / HTTP gateway.
    - Port `8333`: S3 API gateway.
    - Port `9333`: Master server.
  - Minimizes container memory overhead in development while providing fully compliant AWS S3 v4 pre-signed PUT and GET semantics.
- **Alternatives Considered**:
  - MinIO: Heavyweight, AGPL license friction, and deviates from architectural standard ADR-0004.
  - Multi-container SeaweedFS cluster: Unnecessary operational overhead for developer local testing.

---

### Decision 2: Pre-Signed URL Generation in .NET 10
- **Context**: The backend must issue temporary pre-signed URLs directly to SeaweedFS (ADR-0004) so image uploads never traverse the API process.
- **Decision**: Implement `IStorageService` using `AWSSDK.S3` configured with SeaweedFS S3 endpoint (`http://localhost:8333` or container alias `http://seaweedfs:8333`), enabling `ForcePathStyle = true`.
- **Rationale**:
  - SeaweedFS S3 API natively supports AWS S3 SDK standard signatures (v4).
  - Pre-signed PUT URLs with configurable TTL (default: 15 minutes) allow direct binary upload from frontend/curl without buffering binary streams in the API.
- **Alternatives Considered**:
  - Direct HTTP multipart upload to ASP.NET Core: Explicitly rejected by ADR-0004 and Inviolable #4 of `AGENTS.md`.

---

### Decision 3: Automated Database Migration & Seeding for Testing
- **Context**: The containerized PostgreSQL instance must receive the database schema and indexes automatically before test execution.
- **Decision**: Implement an automated migration execution step during test startup (or via `dotnet ef database update` / `context.Database.MigrateAsync()`) in the test harness or CLI script.
- **Rationale**:
  - Ensures integration and security tests run against the exact production schema, JSONB types, and composite GIN indexes defined in feature `001-multi-tenant-digital-menu`.
- **Alternatives Considered**:
  - Raw SQL init scripts mounted into `/docker-entrypoint-initdb.d`: Fragile and risks drift from EF Core migrations.

---

### Decision 4: Integration Test Suite Orchestration (Testcontainers vs. Docker Compose)
- **Context**: Tests must execute in a controlled containerized environment. Docker Compose provides a persistent stack (`docker-compose.dev.yml`), whereas Testcontainers enables isolated, ephemeral runs inside xUnit.
- **Decision**: Provide dual support:
  1. **Persistent Local Stack (`docker-compose.dev.yml`)**: PostgreSQL, Redis, SeaweedFS, OPA with health checks and an init container for the S3 bucket.
  2. **Automated Integration & Security Test Suite (`RestoCore.IntegrationTests` and `RestoCore.SecurityTests`)**: Configured to run against the live docker-compose stack or spin up Testcontainers dynamically.
- **Rationale**: Allows developers to run `docker compose up -d` for interactive exploration via Swagger, while CI/test scripts can verify full end-to-end user journeys deterministically.
