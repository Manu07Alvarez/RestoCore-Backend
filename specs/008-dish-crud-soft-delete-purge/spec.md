# Feature Specification: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker

**Feature Branch**: `feature/008-dish-crud-soft-delete-purge`

**Created**: 2026-10-11

**Status**: Draft

**Source Documents in RestoCore-Docs**:
- [docs/prd-admin-crud.md](file:///R:/Programando/RestoCore/RestoCore-Docs/docs/prd-admin-crud.md) (Commits `6dd7095`, `c031c09`)
- [docs/adr/0010-baja-suave-de-platos-y-snapshots-inmutables.md](file:///R:/Programando/RestoCore/RestoCore-Docs/docs/adr/0010-baja-suave-de-platos-y-snapshots-inmutables.md)
- [specs/openapi.yaml](file:///R:/Programando/RestoCore/RestoCore-Docs/specs/openapi.yaml) (`PUT /api/v1/admin/items/{id}`, `PATCH /api/v1/admin/items/{id}`, `DELETE /api/v1/admin/items/{id}`, `POST /api/v1/admin/items/{id}/restore`)

---

## 1. Context & Business Need

The administrative panel of RestoCore requires full CRUD capabilities over menu items (dishes and beverages), enforcing business rules established in canonical documentation:

1. **Relational Consistency (1:N Category Relationship):** Dishes must belong to a single valid category (`category_id`) within the active tenant. Multi-category association (N:M) is forbidden for MVP.
2. **Dietary Classification (N:M DietaryLabels):** Dishes support multiple independent dietary tags (`celiac_safe`, `vegan`, `vegetarian`, `alcohol_free`, etc.).
3. **Modification Operations (`PUT` & `PATCH`):** Tenant administrators must be able to update all dish attributes (`PUT /api/v1/admin/items/{id}`) or partial attributes such as price and availability (`PATCH /api/v1/admin/items/{id}`), invalidating public CDN cache headers upon modification.
4. **Soft Delete with 7-Day Retention Window:** Immediate physical deletion is prohibited. Archiving a dish (`DELETE /api/v1/admin/items/{id}`) must verify that there are no active kitchen orders (`Pending`, `InPreparation`, `Ready`), set `IsDeleted = true` and `DeletedAt = UtcNow`, and return a human-friendly confirmation message explaining that historical sales remain intact and the dish can be restored within 7 days.
5. **Restoration Capability:** Within the 7-day retention period, an administrator can reactivate an archived dish via `POST /api/v1/admin/items/{id}/restore`, resetting deletion flags and restoring public menu visibility.
6. **Automated Physical Purge Worker:** A daily scheduled background service (`PurgeArchivedDishesWorker`) permanently deletes records where `IsDeleted = true` and `DeletedAt <= UtcNow - 7 days`, and requests deletion of associated image binaries in SeaweedFS.

---

## 2. User Scenarios & Testing *(mandatory)*

### User Story 1 - Dish Modification & Relational Validation (Priority: P1) 🎯 MVP

As a restaurant administrator, I want to edit existing dishes (both full update via PUT and partial patch via PATCH) while strictly validating that the assigned `category_id` exists and belongs to my tenant.

**Why this priority**: Without modification capabilities, menu changes require deleting and recreating items, breaking references and operational flows.

**Independent Test**: Send `PUT /api/v1/admin/items/{id}` and `PATCH /api/v1/admin/items/{id}` with valid authentication; verify attributes update in PostgreSQL, relational validation rejects invalid category IDs with HTTP 422/400, and cache headers trigger invalidation.

**Acceptance Scenarios**:
1. **Given** an existing dish and a valid category within the same tenant, **When** sending `PUT /api/v1/admin/items/{id}` with updated price, description, and `dietary_labels: ["celiac_safe", "vegan"]`, **Then** the system updates the record in PostgreSQL and returns `HTTP 200 OK` with the updated dish model.
2. **Given** an update request with a `category_id` that does not exist or belongs to another tenant, **When** sending `PUT` or `PATCH`, **Then** the system rejects the operation with `HTTP 422 Unprocessable Entity` or `HTTP 400 Bad Request` without modifying the record.
3. **Given** an existing dish, **When** sending `PATCH /api/v1/admin/items/{id}` with partial fields (e.g. `base_price` or `is_available`), **Then** only the specified fields are modified and other attributes remain unchanged.

---

### User Story 2 - Soft Delete with 7-Day Retention & Friendly Response (Priority: P2)

As a restaurant owner, I want to remove a dish from my digital menu with peace of mind that past sales and reports remain intact and I have 7 days to restore it if removed by mistake.

**Why this priority**: Protects financial integrity, complies with accounting standards, and prevents accidental loss of dishes while guaranteeing immediate exclusion from the public QR menu.

**Independent Test**: Issue `DELETE /api/v1/admin/items/{id}` for a dish without active orders; verify `IsDeleted = true`, `DeletedAt` is set, the public menu query filter excludes it, and the HTTP response contains the friendly explanation message.

**Acceptance Scenarios**:
1. **Given** a dish without active orders, **When** the administrator sends `DELETE /api/v1/admin/items/{id}`, **Then** the system marks `IsDeleted = true`, sets `DeletedAt = UtcNow`, and responds with `HTTP 200 OK` containing the explanatory business message.
2. **Given** a dish currently included in an active order with status `Pending`, `InPreparation`, or `Ready`, **When** the administrator attempts to delete the dish, **Then** the system rejects the deletion with `HTTP 409 Conflict` and an explanatory message.
3. **Given** a soft-deleted dish, **When** querying public menus (`GET /api/v1/tenants/{tenant_slug}/menu`), **Then** the dish is filtered out transparently via EF Core Global Query Filter.

---

### User Story 3 - Administrative Dish Restoration (Priority: P3)

As an administrator, I want to restore an archived dish within the 7-day retention period so that it becomes visible again in the administrative panel and on the public digital menu without recreating it.

**Why this priority**: Solves human error during menu management and allows seasonal dishes to be brought back easily within grace period.

**Independent Test**: Issue `POST /api/v1/admin/items/{id}/restore` on a soft-deleted dish with `DeletedAt` within the last 7 days; verify `IsDeleted = false`, `DeletedAt = null`, and the item reappears on the public menu.

**Acceptance Scenarios**:
1. **Given** a dish marked as deleted 3 days ago, **When** sending `POST /api/v1/admin/items/{id}/restore`, **Then** the system resets `IsDeleted = false`, `DeletedAt = null`, and returns `HTTP 200 OK`.
2. **Given** a dish that is not deleted or does not exist, **When** sending `POST /api/v1/admin/items/{id}/restore`, **Then** the system returns `HTTP 400 Bad Request` or `HTTP 404 Not Found`.

---

### User Story 4 - Background Purge Worker (Priority: P4)

As a platform operator, I need dishes archived more than 7 days ago to be permanently purged from the database and have their images deleted from SeaweedFS storage to avoid storage bloat.

**Why this priority**: Guarantees database hygiene and storage optimization without manual intervention.

**Independent Test**: Trigger the purge worker with a test record where `DeletedAt = UtcNow - 8 days`; verify the record is permanently removed from `dishes` and the SeaweedFS delete endpoint is invoked for its `ImageUrl`.

**Acceptance Scenarios**:
1. **Given** dishes with `IsDeleted = true` and `DeletedAt <= UtcNow - 7 days`, **When** `PurgeArchivedDishesWorker` executes its scheduled pass, **Then** it hard-deletes the records from PostgreSQL and requests deletion of image URLs in storage.
2. **Given** soft-deleted dishes where `DeletedAt > UtcNow - 7 days`, **When** the worker runs, **Then** these records are ignored and preserved for potential restoration.

---

## 3. Edge Cases

- **Concurrent deletion and order placement**: If an order arrives for a dish right as deletion is requested, optimistic concurrency or active order checks must prevent inconsistent states.
- **Tenant Isolation**: Attempting to update, delete, or restore a dish belonging to another tenant must return `HTTP 403 Forbidden` or `HTTP 404 Not Found`.
- **Image Deletion Failure**: If SeaweedFS is unreachable during the purge pass, the database deletion should either handle retry or log the orphaned asset without crashing the worker.
- **Unique Name Constraints**: If dish names are constrained within a category, unique constraints in PostgreSQL must only apply where `IsDeleted = false`.

---

## 4. Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide `PUT /api/v1/admin/items/{id}` for total updates of dish attributes (`Name`, `Description`, `BasePrice`, `IsAvailable`, `DisplayOrder`, `CategoryId`, `Allergens`, `DietaryLabels`, `ImageUrl`).
- **FR-002**: System MUST provide `PATCH /api/v1/admin/items/{id}` for partial updates of dish attributes.
- **FR-003**: System MUST enforce that `CategoryId` belongs to the current tenant and exists; rejecting invalid IDs with `HTTP 422 Unprocessable Entity` or `HTTP 400 Bad Request`.
- **FR-004**: System MUST provide `DELETE /api/v1/admin/items/{id}` that sets `IsDeleted = true` and `DeletedAt = UtcNow`.
- **FR-005**: System MUST block deletion with `HTTP 409 Conflict` if the dish is associated with orders in `Pending`, `InPreparation`, or `Ready` state.
- **FR-006**: System MUST return the human-friendly explanatory message on successful soft delete as defined in ADR-0010.
- **FR-007**: System MUST provide `POST /api/v1/admin/items/{id}/restore` to restore soft-deleted items whose `DeletedAt` is within 7 days.
- **FR-008**: System MUST apply a Global Query Filter on `MenuItem` in EF Core to exclude deleted records from standard queries unless explicitly ignored (`IgnoreQueryFilters()`).
- **FR-009**: System MUST execute a background service (`PurgeArchivedDishesWorker`) to permanently delete dishes older than 7 days from `DeletedAt` and cleanup storage assets.

### Key Entities

- **`MenuItem` (Domain)**:
  - `IsDeleted: bool` (default: false)
  - `DeletedAt: DateTimeOffset?`
  - `DietaryLabels: string[]` (array/N:M classification)
  - `Allergens: string[]`
  - `CategoryId: Guid` (mandatory foreign key 1:N)

---

## 5. Success Criteria *(mandatory)*

- **SC-001**: 100% compliance with OpenAPI 3.1 endpoints defined in `RestoCore-Docs/specs/openapi.yaml` for administrative item operations.
- **SC-002**: Dish modifications update in PostgreSQL and reflect in the public menu within latency budget (< 500ms).
- **SC-003**: Soft-deleted dishes disappear immediately from public menu queries without affecting existing historical orders.
- **SC-004**: Purge worker safely deletes records older than 7 days with full test coverage.
- **SC-005**: Unit and integration test suite passes with 0 regressions.
