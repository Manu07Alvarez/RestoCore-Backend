# Research & Architectural Findings: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker

**Feature**: `008-dish-crud-soft-delete-purge`  
**Date**: 2026-10-11  
**Status**: Completed  
**Sources**: [ADR-0010: Baja Suave de Platos y Snapshots Inmutables](file:///R:/Programando/RestoCore/RestoCore-Docs/docs/adr/0010-baja-suave-de-platos-y-snapshots-inmutables.md), [docs/prd-admin-crud.md](file:///R:/Programando/RestoCore/RestoCore-Docs/docs/prd-admin-crud.md), [specs/openapi.yaml](file:///R:/Programando/RestoCore/RestoCore-Docs/specs/openapi.yaml).

---

## 1. Resolved Clarifications & Key Technical Decisions

### D-01: Soft Delete Implementation in EF Core
- **Decision**: Add `IsDeleted: bool` (default: `false`) and `DeletedAt: DateTimeOffset?` directly on `MenuItem` (inheriting from `BaseEntity`).
- **Query Filtering**: Configure a global query filter in `ApplicationDbContext` combined with the existing tenant filter:
  ```csharp
  // For MenuItem specifically:
  // e.TenantId == CurrentTenantId && !e.IsDeleted
  ```
  For administrative queries needing access to soft-deleted items (such as `RestoreMenuItemCommand` or purge checks), use `.IgnoreQueryFilters()` while explicitly filtering by `TenantId`.
- **Rationale**: Keeps customer QR queries transparently isolated from archived dishes without polluting application query logic.
- **Alternatives considered**: Separate `archived_menu_items` table. Rejected because it complicates reactivation/restoration and requires schema duplication.

### D-02: Active Kitchen Orders Verification Before Soft Deletion
- **Decision**: In `IOrderRepository`, add `Task<bool> HasActiveOrdersForMenuItemAsync(Guid tenantId, Guid menuItemId, CancellationToken ct)`.
- **Active States Checked**: Orders in statuses `queued`, `received`, `preparing`, or `ready`. If an active order includes an item snapshot with `MenuItemId == request.Id`, return `HTTP 409 Conflict` with a user-friendly message:
  > *"No es posible retirar este plato porque se encuentra actualmente en preparación en cocina o pedidos abiertos de salón. Espera a que los pedidos finalicen antes de archivarlo."*
- **Rationale**: Satisfies **ADR-0010** safety constraint: dishes currently being prepped cannot vanish from the active kitchen lifecycle.
- **Alternatives considered**: Cancelling ongoing orders. Rejected due to negative diner experience and operational disruption.

### D-03: Human-Friendly Response for Soft Delete (`MenuItemArchiveResponse`)
- **Decision**: Return a dedicated DTO matching OpenAPI schema:
  - `item_id`: `Guid`
  - `status`: `"archived"`
  - `message`: `"El plato ha sido retirado de la carta. Los clientes ya no lo verán en el menú digital ni podrán pedirlo, pero todas tus ventas anteriores y reportes seguirán intactos y seguros. Dispones de 7 días para restaurarlo antes de su eliminación definitiva."`
  - `has_open_orders`: `false`
  - `archived_at`: `DateTimeOffset` (`UtcNow`)
  - `retention_days`: `7`
  - `purge_scheduled_at`: `DateTimeOffset` (`UtcNow.AddDays(7)`)
- **Rationale**: Complies with the non-technical UX mandate defined in ADR-0010 and OpenAPI 3.1.

### D-04: Restoration within 7-Day Window (`POST /api/v1/admin/items/{id}/restore`)
- **Decision**: Retrieve the item with `.IgnoreQueryFilters()` for the current tenant.
  - If not found or `!item.IsDeleted`: Return `HTTP 404 Not Found` or `HTTP 400 Bad Request`.
  - If `item.DeletedAt < DateTimeOffset.UtcNow.AddDays(-7)`: Return `HTTP 404 Not Found` (past retention window).
  - Otherwise, set `IsDeleted = false`, `DeletedAt = null`, invoke `ICdnPurgeService.PurgeMenuCacheAsync(tenantSlug)` (or invalidate cache), commit via `IUnitOfWork`, and return `MenuItemResponse`.
- **Rationale**: Immediate restoration with cache invalidation satisfies both business usability and latency budget (< 500ms).

### D-05: Scheduled Purge Worker (`PurgeArchivedDishesWorker`)
- **Decision**: Implement a `BackgroundService` executing on a periodic schedule (e.g. daily, or configurable interval `PurgeWorker:IntervalHours` with default 24h).
  - Query with `.IgnoreQueryFilters()`: `WHERE is_deleted = true AND deleted_at <= UtcNow - INTERVAL '7 days'`.
  - For each candidate with a non-empty `ImageUrl`, invoke `IStorageService.DeleteObjectAsync(...)` (or log/cleanup in SeaweedFS).
  - Remove entity using `ICatalogRepository.RemoveMenuItem(item)` or raw batch delete, then commit `IUnitOfWork`.
- **Rationale**: Enforces data hygiene and prevents orphaned image storage in SeaweedFS without manual intervention.

---

## 2. Best Practices & Design Constraints

1. **Category 1:N Strict Validation**:
   - Both `CreateMenuItemCommand`, `UpdateMenuItemCommand` (PUT), and `PatchMenuItemCommand` (PATCH) validate that `category_id` exists in `ICatalogRepository.GetCategoryByIdAsync(categoryId)` and belongs to the authenticated `TenantId`.
   - If invalid, throw validation failure mapping to `HTTP 422 Unprocessable Entity` or `HTTP 400 Bad Request`.
2. **N:M DietaryLabels**:
   - Represented as `string[]` in domain model and persisted in PostgreSQL via native text array (`text[]`) or JSON.
   - Allows flexible combinations such as `["celiac_safe", "vegan"]`, `["contains_alcohol"]`, `["alcohol_free"]`.
3. **Optimistic Concurrency**:
   - Handled via `IUnitOfWork` with retry logic as established in Feature 006.
