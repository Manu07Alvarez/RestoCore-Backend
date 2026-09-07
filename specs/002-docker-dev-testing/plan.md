# Implementation Plan: Controlled Containerized Test Environment & End-to-End Verification

**Branch**: `002-docker-dev-testing` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/002-docker-dev-testing/spec.md`

---

## Summary

This plan orchestrates a controlled local containerized environment containing PostgreSQL 16+, Redis 7+, SeaweedFS (Master, Volume, Filer, S3 Gateway), and Open Policy Agent (OPA). It implements decoupled SeaweedFS image pre-signing (ADR-0004) and configures an end-to-end integration and security test harness to verify multi-tenant isolation, dynamic QR generation, kitchen stock toggling, and public menu retrieval against the real infrastructure stack.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10.0 (compatible with .NET 9 LTS runtime)

**Primary Dependencies**:
- ASP.NET Core Minimal APIs
- Entity Framework Core 10 (Npgsql provider)
- AWSSDK.S3 (configured for SeaweedFS S3 gateway v4 signing)
- OpenPolicyAgent client (HTTP / REST)
- StackExchange.Redis
- QRCoder & SkiaSharp
- xUnit, FluentAssertions, Testcontainers

**Storage**:
- PostgreSQL 16+ (Relational tables, JSONB columns, GIN indexes)
- Redis 7+ (Caching & Real-Time Streams)
- SeaweedFS S3 Gateway (Object Storage for menu and tenant imagery)

**Testing**:
- xUnit test runner
- Unit tests (`RestoCore.UnitTests`)
- End-to-End Integration tests (`RestoCore.IntegrationTests`)
- Multi-Tenant Security & Isolation tests (`RestoCore.SecurityTests`)

**Target Platform**: Linux containers (Docker / Docker Compose), local development on Windows host

**Project Type**: Multi-tenant REST Web Service and Integration Test Harness

**Performance Goals**:
- Public menu response < 150ms p95
- Container environment startup and health verification < 45 seconds
- Pre-signed URL generation < 50ms

**Constraints**:
- Zero binary transfers through backend memory/CPU (Inviolable #4 / ADR-0004)
- 100% tenant data isolation across all database operations (Inviolable #5 / ADR-0006)
- Professional documentation standard: strictly zero emojis

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evaluation & Compliance Notes |
| :--- | :--- | :--- |
| **I. Spec-Driven Development** | **PASSED** | Specification created under `specs/002-docker-dev-testing/spec.md`, contract in `contracts/storage-api.yaml`. |
| **II. Multi-Tenancy & OPA Authorization** | **PASSED** | Automated cross-tenant tests assert tenant boundary enforcement in live PostgreSQL with OPA policy checks. |
| **III. Clean Architecture & Cohesion** | **PASSED** | Storage abstraction (`IStorageService`) in Application layer; SeaweedFS S3 implementation in Infrastructure. |
| **IV. Latency Budget & Asynchronous Decoupling** | **PASSED** | Presigned S3 URLs offload image uploads directly to SeaweedFS; API remains 100% stateless and fast. |
| **V. Relational & Flexible Persistence** | **PASSED** | PostgreSQL migrations run automatically with JSONB and GIN indexes. |
| **VI. Test-Driven Quality (NON-NEGOTIABLE)** | **PASSED** | End-to-end integration and security test suites executed against live containers before completion. |

---

## Project Structure

### Documentation (this feature)

```text
specs/002-docker-dev-testing/
+-- plan.md              # This implementation plan
+-- research.md          # SeaweedFS, S3 signing, and container topology research
+-- data-model.md        # Storage bucket structure and infrastructure services
+-- quickstart.md        # Step-by-step startup, migration, and test execution
+-- contracts/           # OpenAPI contracts for storage pre-signing and readiness
¦   +-- storage-api.yaml
+-- checklists/
    +-- requirements.md  # Quality verification checklist
```

### Source Code & Infrastructure Changes

```text
docker-compose.dev.yml                  # Adds SeaweedFS (Master, Volume, S3) and healthchecks
src/RestoCore.Application/
+-- Common/Interfaces/
    +-- IStorageService.cs              # Storage pre-signing interface
src/RestoCore.Infrastructure/
+-- Storage/
    +-- SeaweedStorageService.cs        # AWSSDK.S3 implementation for SeaweedFS
src/RestoCore.Api/
+-- Endpoints/
¦   +-- StorageEndpoints.cs             # POST /api/v1/admin/images/presigned-url
¦   +-- HealthEndpoints.cs              # Updated /ready endpoint checking all 4 services
+-- appsettings.Development.json        # SeaweedFS and Redis connection settings
tests/RestoCore.IntegrationTests/
+-- Endpoints/
¦   +-- SeaweedStorageTests.cs          # Direct S3 upload and retrieval test
¦   +-- FullLifecycleE2ETests.cs        # Comprehensive lifecycle test (Tenant -> Catalog -> Kitchen -> QR)
+-- Fixtures/
    +-- ContainerizedStackFixture.cs    # Test fixture connecting to live compose stack
tests/RestoCore.SecurityTests/
+-- MultiTenancy/
    +-- CrossTenantDatabaseIsolationTests.cs # Live PostgreSQL cross-tenant leak tests
```

---

## Complexity Tracking

*No constitutional violations identified. Design adheres strictly to ratified principles.*
