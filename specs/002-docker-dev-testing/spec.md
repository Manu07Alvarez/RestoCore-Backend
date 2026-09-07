# Feature Specification: Controlled Containerized Test Environment & End-to-End Verification

**Feature Branch**: `002-docker-dev-testing`

**Created**: 2026-09-06

**Status**: Draft

**Input**: User description: "/speckit-specify crea y realiza test usando un entorno de desarrollo en docker el cual contenga PostgreSQL, Redis, SeaweedFS y cualquier otra dependencia necesaria, esto con el objetivo de testear el producto en un espacio controlado y ver si realmente cumple los objetivos."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Orchestrated Infrastructure Readiness & Connectivity (Priority: P1)

An engineer or automated CI/CD pipeline triggers the local development environment containing PostgreSQL 16+, Redis 7+, SeaweedFS (S3-compatible Object Storage), and Open Policy Agent (OPA). The system validates that all services reach healthy operational status and can accept network connections before any test execution begins.

**Why this priority**: Without a deterministic, isolated container environment with all infrastructure dependencies up and verified, integration tests cannot run reliably or reflect real production topology.

**Independent Test**: Execute the container orchestration environment startup, invoke health checks against all service ports (PostgreSQL 5432, Redis 6379, SeaweedFS S3 8333/9000, OPA 8181), and verify that each returns an active, responsive status.

**Acceptance Scenarios**:

1. **Given** a clean development host with Docker running, **When** the environment orchestration is started, **Then** PostgreSQL, Redis, SeaweedFS, and OPA containers spin up and report healthy status within 45 seconds.
2. **Given** running infrastructure containers, **When** the backend health and readiness endpoints (`/healthz` and `/ready`) are queried, **Then** the system confirms active database and external dependency connectivity with HTTP 200 OK.

---

### User Story 2 - End-to-End Multi-Tenant Lifecycle Verification (Priority: P1)

An automated verification test suite runs against the containerized stack to validate the core business lifecycle: provisioning a new restaurant tenant, populating categories and menu dishes, toggling real-time availability from kitchen operations, and requesting dynamic table QR codes.

**Why this priority**: Validates that the software truly meets its business objectives end-to-end against actual database, cache, and policy engines, rather than in-memory mocks.

**Independent Test**: Run the end-to-end verification suite against the containerized environment, executing tenant creation, catalog addition, kitchen stock pausing, and public menu query, asserting that data mutations reflect accurately across PostgreSQL and Redis.

**Acceptance Scenarios**:

1. **Given** the running containerized environment, **When** an automated test provisions a new tenant (`el-bodegon`), **Then** the database correctly registers the tenant schema and default branding.
2. **Given** a provisioned tenant with categories and items, **When** kitchen operations pause an item via API, **Then** immediate public menu requests reflect the item as unavailable while keeping the active categories intact.
3. **Given** a configured table, **When** its QR code is requested, **Then** a valid vector SVG payload encoding the table menu URL with table token is returned.

---

### User Story 3 - SeaweedFS Decoupled Storage & Pre-Signed Upload Validation (Priority: P2)

An administrator uploads a dish or restaurant logo image. In accordance with architectural principle ADR-0004, the backend generates an S3-compatible pre-signed upload URL against SeaweedFS. The test suite verifies that the binary payload can be uploaded directly to SeaweedFS and retrieved publicly without backend server CPU/memory bottlenecks.

**Why this priority**: Guarantees compliance with ADR-0004 and Inviolable #4 (the API server never buffers or transfers large image binaries), verifying SeaweedFS S3 integration in a controlled container.

**Independent Test**: Request a pre-signed image upload URL via the API, upload a test image file directly to SeaweedFS using HTTP PUT, and verify that the resulting public URL successfully returns the image binary.

**Acceptance Scenarios**:

1. **Given** an authenticated administrator, **When** requesting a pre-signed URL for `plato-especial.webp`, **Then** the API returns a cryptographically signed SeaweedFS S3 PUT URL with an active expiration window.
2. **Given** a pre-signed upload URL, **When** the client performs an HTTP PUT directly to SeaweedFS with the image binary, **Then** SeaweedFS accepts the upload with HTTP 200/201 and makes the file accessible via its public read endpoint.

---

### User Story 4 - Strict Multi-Tenant Isolation & Boundary Enforcement (Priority: P2)

An automated security test suite attempts cross-tenant data leakage by querying and mutating resources of `Tenant B` while authenticated in `Tenant A` context within the real PostgreSQL database.

**Why this priority**: Ensures that EF Core Global Query Filters and OPA policy boundaries operate correctly in a real database engine, achieving 100% data boundary isolation with zero leaks.

**Independent Test**: Execute automated cross-tenant assertions against the live PostgreSQL container, confirming that queries filtered by `Tenant A` never return rows belonging to `Tenant B`, and cross-tenant mutations return 403 Forbidden or 404 Not Found.

**Acceptance Scenarios**:

1. **Given** two distinct tenants with disjoint catalogs in PostgreSQL, **When** requesting the catalog for Tenant A, **Then** zero items or categories from Tenant B are returned.
2. **Given** an administrator token for Tenant A, **When** attempting to update an item ID belonging to Tenant B, **Then** the system rejects the operation and does not mutate Tenant B data.

---

### Edge Cases

- **Cold Start Dependency Race**: What happens when the backend attempts to connect before PostgreSQL or Redis is accepting connections? The orchestration configuration defines health checks and dependency orders, and the application includes resilient retry policies during startup.
- **SeaweedFS Bucket Initialization**: What happens if the public images bucket does not exist upon container initialization? An automated initialization script or container entrypoint ensures the required S3 bucket is created automatically upon first run.
- **Port Conflicts on Host Machine**: What happens if ports (e.g., 5432 or 6379) are already occupied on the developer machine? The docker-compose configuration permits environment variable overrides for external port mapping while keeping internal container networking consistent.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST define an orchestrated container environment including PostgreSQL 16+, Redis 7+, SeaweedFS (Master, Volume, S3 gateway), and Open Policy Agent (OPA).
- **FR-002**: The environment configuration MUST include explicit health checks for each service to guarantee readiness prior to test execution.
- **FR-003**: The system MUST provide an automated database migration and schema seeding mechanism that applies the latest EF Core migrations into the containerized PostgreSQL instance.
- **FR-004**: The system MUST configure SeaweedFS with an initialized S3 bucket (`restocore-images`) configured for public read access and authenticated pre-signed PUT uploads.
- **FR-005**: The system MUST provide a pre-signed URL generation service conforming to ADR-0004, enabling decoupled image uploads directly to SeaweedFS.
- **FR-006**: The system MUST provide an end-to-end integration test suite that executes against the live containerized environment, testing all primary user journeys (US1 through US5 of feature 001).
- **FR-007**: The integration test suite MUST execute automated multi-tenant isolation tests verifying that EF Core query filters strictly partition data in PostgreSQL.
- **FR-008**: The system MUST support single-command provisioning, test execution, and teardown of the controlled containerized test environment.

### Key Entities

- **ContainerizedEnvironment**: Configuration set defining service images, network bridges, volume mounts, environment variables, and health check intervals.
- **StorageBucket**: S3-compatible storage namespace hosted in SeaweedFS for tenant assets and menu imagery.
- **PresignedUploadTicket**: Cryptographically signed upload descriptor containing the target URL, HTTP method, required headers, and expiration timestamp.
- **EndToEndTestSuite**: Automated test fixtures and test cases executing HTTP calls against the live running stack.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of required infrastructure services (PostgreSQL, Redis, SeaweedFS, OPA) initialize and reach healthy status in under 60 seconds from cold start.
- **SC-002**: Automated end-to-end verification test suite achieves 100% pass rate across public menu retrieval, kitchen stock control, table QR generation, and tenant onboarding.
- **SC-003**: Direct binary upload to SeaweedFS via pre-signed URL completes successfully with zero binary data streamed through the backend API process.
- **SC-004**: Automated multi-tenant security verification achieves 100% pass rate with zero instances of cross-tenant data leakage observed in PostgreSQL.
- **SC-005**: The entire test lifecycle (spin-up, database migration, test run, assertion verification) can be executed autonomously with a single command.

---

## Assumptions

- The host environment runs Docker Desktop or a compatible container runtime with Docker Compose v2 support.
- SeaweedFS S3 gateway uses standard local development credentials (e.g. key `any`, secret `any` or predefined development access credentials).
- The existing codebase from feature `001-multi-tenant-digital-menu` (entities, endpoints, migrations, and OPA policies) is reused and validated against this live stack.
