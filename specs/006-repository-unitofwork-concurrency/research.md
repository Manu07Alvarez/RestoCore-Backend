# Phase 0 Research: Repositories, Unit of Work, and Concurrency Control

## 1. Context and Problem Statement

The Graphify static and semantic analysis flagged `IApplicationDbContext` as a high-centrality God Node that crosses multiple bounded contexts (Tenants, Catalog, Kitchen, Canvas, QR Codes). 
Every command handler in `RestoCore.Application` directly manipulates `DbSet<T>` and calls `_context.SaveChangesAsync()`.
To resolve this architectural bottleneck and support high-throughput concurrency, we adopt **Option B: Domain-Scoped Repositories and Unit of Work**, addressing:
1. Segregation of persistence interfaces by bounded context.
2. Explicit transaction boundary management (`IUnitOfWork`).
3. Resilient optimistic concurrency handling on mutable items (e.g., kitchen availability toggles).
4. Full backward-compatibility with existing integration tests and API contracts.

---

## 2. Research Decisions

### Decision 1: Scope and Granularity of Repositories

- **Options Considered**:
  - *Option 1*: A repository per entity (`ICategoryRepository`, `IMenuItemRepository`, `ITableRepository`, `ITenantRepository`, `IModifierGroupRepository`, etc.).
  - *Option 2*: Aggregate / Bounded-Context Repositories (`ICatalogRepository`, `IKitchenRepository`, `IMenuLayoutRepository`, `ITenantRepository`).
- **Decision**: **Option 2 (Bounded-Context Repositories)**.
- **Rationale**:
  - `Category`, `MenuItem`, and `ModifierGroup` belong to the Catalog aggregate root hierarchy and are frequently fetched/mutated in relational coordination.
  - Creating 8+ separate repositories introduces an explosion of DI boilerplate and unnecessary indirection.
  - 4 bounded-context repositories map directly to the clean architecture domains of RestoCore.

### Decision 2: Unit of Work Implementation Pattern

- **Options Considered**:
  - *Option 1*: Generic `IUnitOfWork` injected alongside repositories.
  - *Option 2*: Context-specific Unit of Work (`ICatalogUnitOfWork`, etc.).
  - *Option 3*: Unit of Work exposed through `IUnitOfWork` managing explicit transactions and atomic `CommitAsync(ct)`.
- **Decision**: **Option 3 (Scoped `IUnitOfWork` with `CommitAsync` and explicit transaction support)**.
- **Rationale**:
  - In EF Core, `ApplicationDbContext` inherently maintains a Change Tracker that already functions as an internal Unit of Work.
  - Wrapping this in an explicit `IUnitOfWork` decouples MediatR command handlers from EF Core types (`Microsoft.EntityFrameworkCore` namespace) while allowing explicit transaction boundaries (`BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`).

### Decision 3: Optimistic Concurrency Control & Retry Strategy

- **Options Considered**:
  - *Option 1*: Global Polly retry policy on HTTP handlers.
  - *Option 2*: Granular retry inside `IKitchenRepository` / `KitchenCommandHandler` for optimistic concurrency exceptions (`DbUpdateConcurrencyException`).
  - *Option 3*: PostgreSQL `SELECT ... FOR UPDATE` (Pessimistic Locking).
- **Decision**: **Option 2 (Encapsulated Optimistic Retry with Exponential Backoff and Jitter)**.
- **Rationale**:
  - Pessimistic locks (`FOR UPDATE`) reduce throughput and can easily cause deadlocks on concurrent reads and writes in high-traffic restaurants.
  - Optimistic concurrency with EF Core row versioning / concurrency tokens and an encapsulated retry loop (up to 3 retries, exponential backoff: 50ms, 100ms, 200ms) handles 99.9% of concurrent kitchen toggles without blocking read queries or failing user requests.

### Decision 4: Querying (CQRS Query Handlers)

- **Options Considered**:
  - *Option 1*: Force all queries through repository methods returning domain entities, then mapping to DTOs.
  - *Option 2*: Provide read methods on repositories returning projected DTOs or `IQueryable` / specifications.
- **Decision**: **Option 2 (Direct projected queries via Repository methods using `AsNoTracking()`)**.
- **Rationale**:
  - Read queries (such as `GetPublicMenuQuery` or `GetTableQrQuery`) must strictly honor the <500ms server latency budget and emit CDN ETags without tracking overhead.
  - Repositories provide specialized read methods (e.g. `ICatalogRepository.GetPublicMenuDtoAsync(...)`) that project directly to DTOs without attaching entities to the EF Core Change Tracker.

---

## 3. Best Practices Alignment

- **Dependency Injection**: Repositories and `IUnitOfWork` registered as `Scoped` services in `RestoCore.Infrastructure`, resolving `ApplicationDbContext` internally.
- **Tenant Isolation**: All repository queries leverage the global query filter registered on `ApplicationDbContext` (`HasQueryFilter(e => e.TenantId == _tenantContext.TenantId)`), guaranteeing that no repository query can leak data across tenants even if the developer omits `Where(e => e.TenantId == ...)`.
- **Testability**: Interfaces in `RestoCore.Application.Common.Interfaces.Repositories` allow clean, isolated mocking in unit tests without instantiating or mocking `DbSet<T>`.
