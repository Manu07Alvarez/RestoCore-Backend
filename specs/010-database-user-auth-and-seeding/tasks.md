# Tasks: PostgreSQL User Authentication & On-Demand Database Seeding

**Input**: Design documents from `specs/010-database-user-auth-and-seeding/`  
**TDD Discipline**: Unit tests created first in `tests/RestoCore.UnitTests/` per **Constitution Principle VI**.

---

## Phase 1: Setup & Domain Entity

- [X] T001 Create `User` entity in `src/RestoCore.Domain/Entities/User.cs`
- [X] T002 [P] Define `IPasswordHasher` interface in `src/RestoCore.Application/Common/Interfaces/IPasswordHasher.cs`
- [X] T003 [P] Define `IUserRepository` interface in `src/RestoCore.Application/Common/Interfaces/Repositories/IUserRepository.cs`

---

## Phase 2: Foundational Infrastructure

- [X] T004 Implement `PasswordHasher` (PBKDF2-HMAC-SHA256) in `src/RestoCore.Infrastructure/Identity/PasswordHasher.cs`
- [X] T005 Configure `User` table and unique index in `src/RestoCore.Infrastructure/Persistence/Configurations/UserConfiguration.cs` and add `DbSet<User> Users` in `src/RestoCore.Infrastructure/Persistence/ApplicationDbContext.cs`
- [X] T006 Implement `UserRepository` in `src/RestoCore.Infrastructure/Persistence/Repositories/UserRepository.cs`

---

## Phase 3: User Story 1 - PostgreSQL-Backed Authentication & Elimination of Hardcoded Users (Priority: P1) 🎯 MVP

### Tests for User Story 1
- [X] T007 [P] [US1] Create unit tests for `PasswordHasher` in `tests/RestoCore.UnitTests/Features/Auth/PasswordHasherTests.cs`
- [X] T008 [P] [US1] Create unit tests for `LoginCommandHandler` in `tests/RestoCore.UnitTests/Features/Auth/LoginCommandHandlerTests.cs`

### Implementation for User Story 1
- [X] T009 [US1] Implement `LoginCommand`, `LoginResponse`, `UserProfileResponse`, and `LoginCommandHandler` in `src/RestoCore.Application/Features/Auth/Commands/LoginCommand.cs`
- [X] T010 [US1] Refactor `src/RestoCore.Api/Endpoints/AuthEndpoints.cs` to remove all hardcoded accounts (`SeededUser`) and delegate authentication to `ISender.Send(new LoginCommand(...))`

---

## Phase 4: User Story 2 - On-Demand Complete Database Seeding Script (Priority: P2)

### Implementation for User Story 2
- [X] T011 [US2] Implement idempotent `DatabaseSeeder` (seeding schema, `el-gato-negro` tenant, hashed user accounts, categories, sample dishes, and QR tables) in `src/RestoCore.Infrastructure/Persistence/Seed/DatabaseSeeder.cs`
- [X] T012 [US2] Register `IPasswordHasher`, `IUserRepository`, and `--seed` CLI command handler in `src/RestoCore.Api/Program.cs`
- [X] T013 [P] [US2] Create executable PowerShell script `scripts/seed-db.ps1` for manual on-demand seeding

---

## Phase 5: Polish & Verification

- [X] T014 Execute full unit test suite `dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj`
- [X] T015 Verify zero hardcoded credentials remain in `src/` and update knowledge graph with `graphify update .`
