# Tasks: Category ABMC & Restaurant Contact Profile Management

**Input**: Design documents from `specs/009-category-abmc-restaurant-profile/`  
**Prerequisites**: [plan.md](file:///r:/Programando/RestoCore/resto-core-back/specs/009-category-abmc-restaurant-profile/plan.md), [spec.md](file:///r:/Programando/RestoCore/resto-core-back/specs/009-category-abmc-restaurant-profile/spec.md), [research.md](file:///r:/Programando/RestoCore/resto-core-back/specs/009-category-abmc-restaurant-profile/research.md), [data-model.md](file:///r:/Programando/RestoCore/resto-core-back/specs/009-category-abmc-restaurant-profile/data-model.md), [contracts/contracts.md](file:///r:/Programando/RestoCore/resto-core-back/specs/009-category-abmc-restaurant-profile/contracts/contracts.md)  
**TDD Discipline**: Unit tests created first in `tests/RestoCore.UnitTests/` per **Constitution Principle VI**.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verification and preparation of domain models and baseline compilation.

- [X] T001 Verify project compilation and test baseline with `dotnet build`
- [X] T002 Add `PhoneNumber: string?`, `Address: string?`, and `BusinessHours: string?` properties to `Tenant` in `src/RestoCore.Domain/Entities/Tenant.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core persistence mappings and repository interfaces required by user stories.

**CRITICAL**: Must complete before user story handlers and endpoints.

- [X] T003 Configure `PhoneNumber` (max 50), `Address` (max 255), and `BusinessHours` (max 255) column constraints in `src/RestoCore.Infrastructure/Persistence/Configurations/TenantConfiguration.cs`
- [X] T004 [P] Add `GetCategoriesByTenantAsync` and `HasMenuItemsForCategoryAsync` methods to `ICatalogRepository` in `src/RestoCore.Application/Common/Interfaces/Repositories/ICatalogRepository.cs`
- [X] T005 Implement `GetCategoriesByTenantAsync` and `HasMenuItemsForCategoryAsync` (using `IgnoreQueryFilters()`) in `src/RestoCore.Infrastructure/Persistence/Repositories/CatalogRepository.cs`

**Checkpoint**: Foundation ready. User stories can now be implemented.

---

## Phase 3: User Story 1 - Full Category ABMC with Referential Integrity Guardrail (Priority: P1) 🎯 MVP

**Goal**: Provide administrative endpoints for listing, inspecting, creating, updating (`PUT` and `PATCH`), and deleting categories (`DELETE`), enforcing `HTTP 409 Conflict` when a category contains associated dishes.

**Independent Test**: Execute `CategoryQueryHandlerTests`, `UpdatePatchCategoryCommandHandlerTests`, and `DeleteCategoryCommandHandlerTests`, verifying ordered retrieval, full/partial updates, `409 Conflict` when `HasMenuItemsForCategoryAsync` returns true, and physical deletion when empty.

### Tests for User Story 1
- [X] T006 [P] [US1] Create unit tests for `GetCategoriesQueryHandler` and `GetCategoryByIdQueryHandler` in `tests/RestoCore.UnitTests/Features/Categories/CategoryQueryHandlerTests.cs`
- [X] T007 [P] [US1] Create unit tests for `UpdateCategoryCommandHandler` and `PatchCategoryCommandHandler` in `tests/RestoCore.UnitTests/Features/Categories/UpdatePatchCategoryCommandHandlerTests.cs`
- [X] T008 [P] [US1] Create unit tests for `DeleteCategoryCommandHandler` (verifying conflict exception when dishes exist and physical removal when empty) in `tests/RestoCore.UnitTests/Features/Categories/DeleteCategoryCommandHandlerTests.cs`

### Implementation for User Story 1
- [X] T009 [P] [US1] Update `CreateCategoryCommand` and `CategoryResponse` with `JsonPropertyName` attributes (`sort_order`, `is_visible`) and `ICdnPurgeService` invalidation in `src/RestoCore.Application/Features/Categories/Commands/CreateCategoryCommand.cs`
- [X] T010 [P] [US1] Implement `GetCategoriesQuery` and `GetCategoryByIdQuery` with handlers in `src/RestoCore.Application/Features/Categories/Queries/GetCategoriesQueries.cs`
- [X] T011 [P] [US1] Implement `UpdateCategoryCommand` and handler in `src/RestoCore.Application/Features/Categories/Commands/UpdateCategoryCommand.cs`
- [X] T012 [P] [US1] Implement `PatchCategoryCommand` and handler in `src/RestoCore.Application/Features/Categories/Commands/PatchCategoryCommand.cs`
- [X] T013 [P] [US1] Implement `DeleteCategoryCommand` and handler with `HasMenuItemsForCategoryAsync` guardrail in `src/RestoCore.Application/Features/Categories/Commands/DeleteCategoryCommand.cs`
- [X] T014 [US1] Expose `GET /categories`, `GET /categories/{id}`, `PUT /categories/{id}`, `PATCH /categories/{id}`, and `DELETE /categories/{id}` in `src/RestoCore.Api/Endpoints/AdminCatalogEndpoints.cs`

**Checkpoint**: User Story 1 operational and independently testable.

---

## Phase 4: User Story 2 - Restaurant Profile Management (Priority: P2)

**Goal**: Provide administrative endpoints (`GET`, `PUT`, `PATCH /api/v1/admin/tenant/profile`) to consult and modify the establishment's commercial name, phone number, address, and business hours.

**Independent Test**: Execute `TenantProfileHandlerTests`, verifying profile retrieval, full replacement via `PUT`, partial update via `PATCH`, and cache invalidation.

### Tests for User Story 2
- [X] T015 [P] [US2] Create unit tests for `GetTenantProfileQueryHandler`, `UpdateTenantProfileCommandHandler`, and `PatchTenantProfileCommandHandler` in `tests/RestoCore.UnitTests/Features/Tenants/TenantProfileHandlerTests.cs`

### Implementation for User Story 2
- [X] T016 [P] [US2] Create `TenantProfileResponse` DTO in `src/RestoCore.Application/Features/Tenants/DTOs/TenantProfileResponse.cs`
- [X] T017 [P] [US2] Implement `GetTenantProfileQuery` and handler in `src/RestoCore.Application/Features/Tenants/Queries/GetTenantProfileQuery.cs`
- [X] T018 [P] [US2] Implement `UpdateTenantProfileCommand` and handler in `src/RestoCore.Application/Features/Tenants/Commands/UpdateTenantProfileCommand.cs`
- [X] T019 [P] [US2] Implement `PatchTenantProfileCommand` and handler in `src/RestoCore.Application/Features/Tenants/Commands/PatchTenantProfileCommand.cs`
- [X] T020 [US2] Create `AdminProfileEndpoints` mapping `GET`, `PUT`, and `PATCH /api/v1/admin/tenant/profile` in `src/RestoCore.Api/Endpoints/AdminProfileEndpoints.cs` and register in `src/RestoCore.Api/Program.cs`

**Checkpoint**: User Story 2 operational and independently testable.

---

## Phase 5: User Story 3 - Public Menu Exposure of Contact Profile (Priority: P3)

**Goal**: Expose `phone_number`, `address`, and `business_hours` in the public digital QR menu payload (`GET /api/v1/tenants/{tenant_slug}/menu`).

**Independent Test**: Execute `PublicMenuContactProfileTests` verifying that `GetPublicMenuQueryHandler` maps `PhoneNumber`, `Address`, and `BusinessHours` from the `Tenant` entity into `PublicMenuResponse`.

### Tests for User Story 3
- [X] T021 [P] [US3] Create unit tests verifying `PhoneNumber`, `Address`, and `BusinessHours` mapping in `GetPublicMenuQueryHandler` in `tests/RestoCore.UnitTests/Features/PublicMenu/PublicMenuContactProfileTests.cs`

### Implementation for User Story 3
- [X] T022 [P] [US3] Add `PhoneNumber`, `Address`, and `BusinessHours` properties to `PublicMenuResponse` in `src/RestoCore.Application/Features/PublicMenu/DTOs/PublicMenuResponse.cs`
- [X] T023 [US3] Map `PhoneNumber`, `Address`, and `BusinessHours` in `GetPublicMenuQueryHandler` in `src/RestoCore.Application/Features/PublicMenu/Queries/GetPublicMenuQuery.cs`

**Checkpoint**: User Story 3 operational and independently testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: End-to-end test execution, OpenAPI contract synchronization verification, and knowledge graph update.

- [X] T024 Execute full unit test suite `dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj`
- [X] T025 Run `powershell -ExecutionPolicy Bypass -File .agents/skills/sync-docs-specs/scripts/compare-docs.ps1` to confirm 100% OpenAPI contract coverage
- [X] T026 Update knowledge graph with `graphify update .`
