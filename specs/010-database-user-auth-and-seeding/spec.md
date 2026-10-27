# Feature Specification: PostgreSQL User Authentication & On-Demand Database Seeding

**Feature Branch**: `feature/010-database-user-auth-and-seeding`  
**Created**: 2026-10-26  
**Status**: Approved  
**Source Documents**:
- `RestoCore-Docs/specs/openapi.yaml` (`/api/v1/auth/login`, `/api/v1/auth/me`)
- `RestoCore-Docs/docs/backend-specification.md`
- `AGENTS.md` (Multi-Tenancy, Clean Architecture, PostgreSQL Persistence)

---

## 1. Context & Business Need

Previously, `AuthEndpoints.cs` contained hardcoded in-memory user credentials (`admin@bistrogourmet.com`, `cocina@restocore.com`, `superadmin@restocore.com`) that bypassed PostgreSQL entirely and allowed any non-empty password. Having hardcoded accounts in production/application code is a security and architectural anti-pattern.

To enforce strict database-backed identity and clean separation between runtime code and test/development data:
1. **Eliminate All Hardcoded Credentials**: Remove `SeededUser` and all hardcoded credential checks from `AuthEndpoints.cs`.
2. **Database-Backed User Authentication**: Persist user accounts in PostgreSQL (`users` table) with cryptographic PBKDF2 password hashes (`Rfc2898DeriveBytes` SHA-256), assigned roles, and optional tenant association (`TenantId`).
3. **On-Demand Automated Seeding Script**: Provide an explicit, idempotent seeding utility (`DatabaseSeeder`) invoked exclusively on demand via CLI flag (`--seed`) and a wrapper script (`scripts/seed-db.ps1`) that populates:
   - Base Tenant (`El Gato Negro`, `11111111-1111-1111-1111-111111111111`)
   - Users (`admin@bistrogourmet.com`, `admin@restocore.com`, `cocina@restocore.com`, `superadmin@restocore.com`) with hashed passwords
   - Categories, sample menu items with modifiers/dietary labels, and QR tables.

---

## 2. User Scenarios & Testing *(mandatory)*

### User Story 1 - PostgreSQL-Backed Authentication & Elimination of Hardcoded Users (Priority: P1) 🎯 MVP

As a platform security engineer, I want `POST /api/v1/auth/login` to authenticate users strictly against the PostgreSQL `users` table using cryptographic password verification, with zero hardcoded accounts in the application code.

**Why this priority**: Eliminates hardcoded backdoors and ensures identity state lives exclusively in PostgreSQL.

**Independent Test**:
- Verify `AuthEndpoints.cs` contains zero hardcoded emails or passwords.
- Execute `LoginCommandHandlerTests` verifying valid credentials return a signed JWT with tenant and role claims, and invalid passwords or non-existent users are rejected with `UnauthorizedAccessException` (`HTTP 401 Unauthorized`).

**Acceptance Scenarios**:
1. **Given** an unseeded database, **When** a client sends `POST /api/v1/auth/login` with `admin@bistrogourmet.com`, **Then** the system returns `HTTP 401 Unauthorized`.
2. **Given** a registered active user in PostgreSQL with a PBKDF2 password hash, **When** sending `POST /api/v1/auth/login` with matching email and password, **Then** the system returns `HTTP 200 OK` with a signed JWT and `LoginResponse`.
3. **Given** a registered active user in PostgreSQL, **When** sending `POST /api/v1/auth/login` with an incorrect password, **Then** the system rejects the request with `HTTP 401 Unauthorized`.

---

### User Story 2 - On-Demand Complete Database Seeding Script (Priority: P2)

As a developer or QA engineer, I want to execute a manual seeding script (`./scripts/seed-db.ps1` or `dotnet run --project src/RestoCore.Api -- --seed`) whenever needed to idempotently populate the database with initial users, the base tenant (`el-gato-negro`), categories, sample dishes, and QR tables.

**Why this priority**: Allows deterministic environment provisioning on demand without polluting application startup or hardcoding data in endpoints.

**Independent Test**:
- Run `./scripts/seed-db.ps1` against PostgreSQL.
- Verify tenant, users, categories, dishes, and tables are created idempotently.
- Call `POST /api/v1/auth/login` with `admin@bistrogourmet.com` / `admin123` and verify `HTTP 200 OK`.

**Acceptance Scenarios**:
1. **Given** a PostgreSQL instance, **When** running `./scripts/seed-db.ps1`, **Then** the seeder ensures schema readiness, inserts missing tenant, users, categories, menu items, and tables, and exits with code 0.
2. **Given** an already seeded database, **When** running `./scripts/seed-db.ps1` a second time, **Then** no duplicate records or errors occur (100% idempotent).

---

## 3. Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST define a `User` entity mapped to the PostgreSQL `users` table with `Id`, `Email` (unique index), `Name`, `PasswordHash`, `PrimaryRole`, `Roles` (`string[]`), `TenantId` (nullable FK to `tenants`), and `IsActive`.
- **FR-002**: System MUST hash and verify passwords using PBKDF2-HMAC-SHA256 (`IPasswordHasher`) with cryptographic salt and constant-time comparison (`CryptographicOperations.FixedTimeEquals`).
- **FR-003**: System MUST remove all hardcoded user accounts from `AuthEndpoints.cs` and delegate `POST /api/v1/auth/login` to `LoginCommand` / `LoginCommandHandler`.
- **FR-004**: System MUST provide `DatabaseSeeder` triggered via CLI argument `--seed` and helper script `scripts/seed-db.ps1` (manual execution only; never auto-executed on standard web server startup).
- **FR-005**: The seeder MUST idempotently seed:
  - Tenant `El Gato Negro` (`11111111-1111-1111-1111-111111111111`, slug `el-gato-negro`)
  - Users: `admin@bistrogourmet.com`, `admin@restocore.com`, `cocina@restocore.com`, `superadmin@restocore.com`
  - Menu Categories, sample Menu Items with modifiers and dietary labels, and QR Tables.

---

## 4. Success Criteria *(mandatory)*

- **SC-001**: Zero hardcoded user emails, passwords, or tenant IDs in `AuthEndpoints.cs` or any endpoint/handler.
- **SC-002**: `POST /api/v1/auth/login` validates credentials strictly against PostgreSQL and rejects invalid passwords with `HTTP 401 Unauthorized`.
- **SC-003**: `scripts/seed-db.ps1` completes idempotently and populates users, tenant, categories, dishes, and tables.
- **SC-004**: 100% of unit tests passing (`LoginCommandHandlerTests`, `PasswordHasherTests`, plus existing test suite).
