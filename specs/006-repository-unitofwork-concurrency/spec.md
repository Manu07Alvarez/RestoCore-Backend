# Feature Specification: Repositories and Unit of Work for Concurrency and Persistence Decoupling

**Feature Branch**: `006-repository-unitofwork-concurrency`

**Created**: 2026-10-10

**Status**: Draft

**Input**: User decision to adopt Option B: Refactor monolithic `IApplicationDbContext` into domain-scoped Repository and Unit of Work patterns with fine-grained concurrency control, retry policies, and bounded-context persistence isolation.

---

## Background & Architectural Drivers

The codebase analysis performed via Graphify revealed `IApplicationDbContext` and `RestoCore.Application.Common.Interfaces` as global God Nodes (betweenness centrality > 0.030, connecting 30+ components across all application boundaries). Every MediatR command and query directly injects `IApplicationDbContext`, causing:
1. **Violation of the Interface Segregation Principle (ISP)**: Unrelated domains (e.g. Kitchen item availability toggling) have direct visibility over `Tenants`, `Tables`, and `MenuPublishJobs`.
2. **Change Tracker Concurrency Contention**: Concurrent operations sharing the same scoped `DbContext` instance cannot execute parallel subtasks and lack granular optimistic retry policies.
3. **Implicit Transaction Boundaries**: Any mutation in one domain is bundled into a global `SaveChangesAsync` call, creating risks of unexpected side-effects and database deadlocks under high load.

This specification defines the migration to dedicated Repositories and Unit of Work per bounded context, providing explicit transactional boundaries, isolated optimistic concurrency retries, and strict domain decoupling.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Kitchen Availability Updates with Concurrency Resilience (Priority: P1)

As a kitchen staff member or automated kitchen queue, I need to toggle the availability of menu items (`is_available`) concurrently without failing or deadlocking when simultaneous updates occur on the same tenant catalog.

**Why this priority**: Kitchen availability is the most frequent runtime mutation in a running restaurant. High-concurrency contention must not throw unhandled database concurrency errors to end-users or staff.

**Independent Test**: Simulate 10 simultaneous HTTP requests toggling availability on the same item and verify that optimistic concurrency retries succeed idempotently and return HTTP 200 without `DbUpdateConcurrencyException` leaks.

**Acceptance Scenarios**:
1. **Given** an existing menu item with `is_available = true`, **When** two kitchen staff simultaneously toggle availability, **Then** the repository handles the version check, retries optimistically up to 3 times, commits the change, and invalidates the tenant menu cache.
2. **Given** a non-existent item or a cross-tenant item, **When** availability is requested to toggle, **Then** the repository returns null/not found without acquiring transaction locks.

---

### User Story 2 - Catalog Management via Isolated Catalog Repository & Unit of Work (Priority: P2)

As an restaurant owner, I want to create, modify, and manage categories, menu items, and modifier groups through an isolated Catalog Unit of Work so that catalog operations maintain atomic consistency without leaking into tenant or kitchen persistence pipelines.

**Why this priority**: Catalog changes represent core administrative functionality with complex multi-table relationships (items, modifier groups, options).

**Independent Test**: Create a Category, a MenuItem, and a ModifierGroup in a single transaction via `ICatalogUnitOfWork.CommitAsync()`; verify that rolling back on failure leaves no orphaned entities in PostgreSQL.

**Acceptance Scenarios**:
1. **Given** a valid tenant context, **When** creating a category and linking items within `ICatalogUnitOfWork`, **Then** all changes commit atomically and publish cache invalidations.
2. **Given** a database exception mid-transaction (e.g. unique constraint or cancellation), **When** committing `ICatalogUnitOfWork`, **Then** the transaction rolls back cleanly without leaving untracked state in the application layer.

---

### User Story 3 - Canvas Layout Compilation & Job State Management (Priority: P3)

As a tenant administrator publishing a canvas menu layout, I need the compilation and publishing job state machine (`MenuPublishJob`) to run via a dedicated Layout Repository so that long-running compilation and CDN purges do not lock or hold open transactions on the primary catalog tables.

**Why this priority**: Canvas compilation creates background jobs and writes static layouts. Isolating job lifecycle management prevents table-level lock contention on `MenuItems`.

**Independent Test**: Enqueue a `MenuPublishJob`, update its status from `Queued` $\rightarrow$ `Processing` $\rightarrow$ `Completed` using `IMenuLayoutRepository`, and verify that concurrent read queries on the public menu are not blocked.

**Acceptance Scenarios**:
1. **Given** a published menu request, **When** the job transitions states, **Then** the repository updates the job timestamp and status atomically without holding catalog locks.

---

## Edge Cases

- **PostgreSQL Concurrency Conflicts**: When two transactions attempt to update the same row concurrently, the repository must catch concurrency violations and apply an exponential backoff retry policy (maximum 3 retries).
- **Tenant Context Isolation**: Repositories must enforce tenant isolation either through global query filters or explicit repository tenant scoping, ensuring cross-tenant data leakage is structurally impossible.
- **Transaction Rollback on Cancellation**: When a request cancellation token is triggered, any open `UnitOfWork` transaction must issue a database rollback and release connection pool resources immediately.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST define domain repository interfaces under `RestoCore.Application.Common.Interfaces.Repositories` segregated by bounded context:
  - `ICatalogRepository` (Categories, MenuItems, ModifierGroups, ModifierOptions)
  - `IKitchenRepository` (Item availability, kitchen status)
  - `IMenuLayoutRepository` (MenuPublishJobs, layout metadata)
  - `ITenantRepository` (Tenants, Tables, branding)
- **FR-002**: The system MUST define an `IUnitOfWork` contract supporting explicit atomic commits (`CommitAsync`) and rollbacks (`RollbackAsync`).
- **FR-003**: The kitchen repository MUST implement optimistic concurrency retry logic (with maximum 3 retries and jittered exponential backoff) for status and availability updates.
- **FR-004**: Command handlers MUST inject specific repository / unit of work interfaces rather than the monolithic `IApplicationDbContext`.
- **FR-005**: All existing integration tests and unit tests MUST pass without regression following the refactoring.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of MediatR command and query handlers in `RestoCore.Application` are decoupled from `IApplicationDbContext`.
- **SC-002**: Zero `DbUpdateConcurrencyException` errors exposed to HTTP callers during 50 concurrent item availability toggle requests.
- **SC-003**: All existing 43 unit and integration test suites pass successfully.
- **SC-004**: Graphify betweenness centrality of `IApplicationDbContext` decreases to 0 (or is removed/isolated to Infrastructure).

---

## Assumptions

- PostgreSQL remains the underlying storage engine managed by EF Core.
- The existing tenant query filters (`HasQueryFilter(e => e.TenantId == ...)`) continue to be enforced through EF Core's model configuration.
- Redis and SeaweedFS interactions remain decoupled via their respective existing services (`IStorageService`, `ICdnPurgeService`).
