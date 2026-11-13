---
description: "Task list for 012-backend-architecture-optimizations implementation"
---

# Tasks: Optimizaciones Arquitectónicas de Persistencia JSONB/GIN, Caché Híbrido L1/L2, Mediador CQRS, Gobernanza OpenTelemetry/OPA y Ciclo de Vida de Streams/WebSockets

**Input**: Design documents from `/specs/012-backend-architecture-optimizations/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/contracts.md`, `quickstart.md`

**Tests**: Required per Principle VI of the RestoCore Constitution (`AGENTS.md` Inviolable #7: Test-First / TDD discipline).

**Organization**: Tasks are grouped by user story (`US1` to `US5`) to enable independent implementation and verification of each architectural optimization (`ADR-0014` to `ADR-0018`; `OPT-01` to `OPT-10`).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (`[US1]`, `[US2]`, `[US3]`, `[US4]`, `[US5]`)
- Exact file paths are included in all descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare shared interfaces and directory structure in `RestoCore.Application` and `RestoCore.Infrastructure`.

- [X] T001 Create pipeline behavior and caching directories `src/RestoCore.Application/Common/Behaviors/` and `src/RestoCore.Application/Common/Caching/`
- [X] T002 [P] Define `PublicMenuCacheEntry` and `IPublicMenuCacheService` contract in `src/RestoCore.Application/Common/Caching/IPublicMenuCacheService.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core interfaces in `RestoCore.Application` required across multiple user stories.

**CRITICAL**: Must complete before User Story implementation begins.

- [X] T003 [P] Define `RequestHandlerDelegate<in TRequest, TResponse>` and `IPipelineBehavior<TRequest, TResponse>` in `src/RestoCore.Application/Common/Mediator/IPipelineBehavior.cs`
- [X] T004 [P] Define `IRequestInvoker<TResponse>` and `RequestInvoker<TRequest, TResponse>` in `src/RestoCore.Application/Common/Mediator/IRequestInvoker.cs`
- [X] T005 [P] Add `GetMenuItemsWithModifiersByIdsAsync` to `ICatalogRepository` in `src/RestoCore.Application/Common/Interfaces/Repositories/ICatalogRepository.cs`
- [X] T006 [P] Add `GetActiveTenantStreamsAsync` to `IOrderQueueService` in `src/RestoCore.Application/Common/Interfaces/IOrderQueueService.cs`

**Checkpoint**: Foundational interfaces ready — user story implementation can proceed.

---

## Phase 3: User Story 1 - Caché Híbrido Multinivel L1/L2 y Filtrado Estricto de Baja Suave en Carta QR (Priority: P1) 🎯 MVP

**Goal**: Servir el menú público QR desde un caché híbrido L1 (`IMemoryCache`, 60s) + L2 (`IDistributedCache` Redis, 2h) con clave segmentada `menu:public:{tenantSlug}:{tableToken}`, invalidación distribuida vía Redis Pub/Sub (`restocore:hybridcache:invalidate`), evaluación `HTTP 304 Not Modified` inmediata desde L1, y exclusión estricta de platos eliminados (`!IsDeleted`) (`ADR-0015` / `OPT-02`, `OPT-03`).

**Independent Test**: Ejecutar pruebas unitarias de `PublicMenuHybridCacheService` y `CatalogRepository` verificando que un acierto L1 evita llamadas a Redis, que `RemoveByTenantAsync` limpia L1/L2 y publica en Redis Pub/Sub, y que `GetTenantWithCatalogAsync` excluye ítems con `IsDeleted = true`.

### Tests for User Story 1

- [X] T007 [P] [US1] Write unit tests for L1/L2 hybrid caching, stampede protection, and tenant invalidation in `tests/RestoCore.UnitTests/Infrastructure/PublicMenuHybridCacheServiceTests.cs`
- [X] T008 [P] [US1] Write unit tests verifying soft-deleted dishes (`IsDeleted = true`) are excluded from public catalog queries in `tests/RestoCore.UnitTests/Features/PublicMenu/PublicMenuSoftDeleteFilterTests.cs`

### Implementation for User Story 1

- [X] T009 [US1] Implement `PublicMenuHybridCacheService` (L1 `IMemoryCache` 60s + L2 `IDistributedCache` 2h + Redis Pub/Sub `restocore:hybridcache:invalidate`) in `src/RestoCore.Infrastructure/Caching/PublicMenuHybridCacheService.cs`
- [X] T010 [US1] Update `CdnPurgeService` and `ApplicationDbContext` to invoke `IPublicMenuCacheService.InvalidateTenantMenuAsync` on catalog/profile mutations in `src/RestoCore.Infrastructure/Cdn/LocalDevelopmentCdnPurgeService.cs` and `src/RestoCore.Infrastructure/Persistence/ApplicationDbContext.cs`
- [X] T011 [US1] Update `PublicMenuEndpoints` to resolve `PublicMenuCacheEntry` via `IPublicMenuCacheService` with `table_token` segmentation and immediate `304 Not Modified` response in `src/RestoCore.Api/Endpoints/PublicMenuEndpoints.cs`
- [X] T012 [US1] Enforce explicit `!i.IsDeleted && i.IsAvailable` filtering in `CatalogRepository.GetTenantWithCatalogAsync` and `GetPublicMenuQueryHandler` in `src/RestoCore.Infrastructure/Persistence/Repositories/CatalogRepository.cs` and `src/RestoCore.Application/Features/PublicMenu/Queries/GetPublicMenuQuery.cs`

**Checkpoint**: User Story 1 (`ADR-0015` / `OPT-02`, `OPT-03`) complete and independently verified.

---

## Phase 4: User Story 2 - Persistencia JSONB con Operador Nativo `@>`, Índices GIN/B-Tree Parciales y Normalización de Login (Priority: P1)

**Goal**: Estandarizar `[JsonPropertyName]` en camelCase en `OrderItemSnapshot` y `OrderModifierSnapshot`, delegar `HasActiveOrdersForMenuItemAsync` al operador `@>` de PostgreSQL con índice GIN e índice B-Tree parcial `ix_orders_active_tenant`, y eliminar `.ToLower()` sobre columnas indexadas en `UserRepository` y `CatalogRepository` (`ADR-0014` / `OPT-04`, `OPT-08`).

**Independent Test**: Ejecutar pruebas unitarias comprobando que `OrderItemSnapshot` serializa `"menuItemId"` en camelCase, que `OrderRepository.HasActiveOrdersForMenuItemAsync` detecta órdenes activas sin descargar colecciones completas en PostgreSQL, y que `UserRepository.GetByEmailAsync` normaliza el email en minúsculas sin invocar `.ToLower()` en la columna.

### Tests for User Story 2

- [X] T013 [P] [US2] Write unit tests for camelCase JSON serialization of `OrderItemSnapshot`, `OrderRepository.HasActiveOrdersForMenuItemAsync`, and `UserRepository.GetByEmailAsync` in `tests/RestoCore.UnitTests/Infrastructure/PersistenceOptimizationTests.cs`

### Implementation for User Story 2

- [X] T014 [P] [US2] Annotate `OrderItemSnapshot` and `OrderModifierSnapshot` properties with explicit camelCase `[JsonPropertyName]` attributes in `src/RestoCore.Domain/Entities/Order.cs`
- [X] T015 [P] [US2] Configure GIN index on `ItemsSnapshot` and partial B-Tree index `ix_orders_active_tenant` on `TenantId` in `src/RestoCore.Infrastructure/Persistence/Configurations/OrderConfiguration.cs`
- [X] T016 [US2] Update `OrderRepository.HasActiveOrdersForMenuItemAsync` to use PostgreSQL JSONB containment `@>` with `[{"menuItemId":"..."}]` (with InMemory fallback for unit tests) in `src/RestoCore.Infrastructure/Persistence/Repositories/OrderRepository.cs`
- [X] T017 [US2] Normalize email/name comparison without `.ToLower()` on database columns in `src/RestoCore.Infrastructure/Persistence/Repositories/UserRepository.cs` and `src/RestoCore.Infrastructure/Persistence/Repositories/CatalogRepository.cs`

**Checkpoint**: User Story 2 (`ADR-0014` / `OPT-04`, `OPT-08`) complete and independently verified.

---

## Phase 5: User Story 3 - Mediador CQRS Precompilado sin Reflexión y Canalización `IPipelineBehavior` (Priority: P1)

**Goal**: Eliminar `MethodInfo.Invoke` y `Type.GetMethod` en `Mediator.Send` mediante `RequestInvoker<TRequest, TResponse>` cacheado en `ConcurrentDictionary` y desacoplar FluentValidation en `ValidationBehavior<TRequest, TResponse>` (`ADR-0016` / `OPT-01`).

**Independent Test**: Ejecutar pruebas unitarias de `Mediator` y `ValidationBehavior` verificando el encadenamiento de múltiples `IPipelineBehavior`, la ejecución paralela de validadores y el despacho tipado repetido sin reflexión.

### Tests for User Story 3

- [X] T018 [P] [US3] Write unit tests for `Mediator`, `RequestInvoker`, and `ValidationBehavior` pipeline execution in `tests/RestoCore.UnitTests/Application/MediatorPipelineTests.cs`

### Implementation for User Story 3

- [X] T019 [US3] Implement `ValidationBehavior<TRequest, TResponse>` using `FluentValidation.IValidator<TRequest>` in `src/RestoCore.Application/Common/Behaviors/ValidationBehavior.cs`
- [X] T020 [US3] Refactor `Mediator` and `MediatorServiceCollectionExtensions` to use `InvokerCache` (`IRequestInvoker<TResponse>`) and register `IPipelineBehavior<,>` (`ValidationBehavior<,>`) in `src/RestoCore.Application/Common/Mediator/Mediator.cs`

**Checkpoint**: User Story 3 (`ADR-0016` / `OPT-01`) complete and independently verified.

---

## Phase 6: User Story 4 - Procesamiento Reactivo de Redis Streams, Carga SQL por Lote (`ANY`) y Ciclo de Vida Determinista de WebSockets y Redis (Priority: P2)

**Goal**: Eliminar las 120 consultas SQL por minuto en reposo en `OrderProcessorWorker` usando el Redis Set `restocore:active_tenant_streams`, reemplazar el bucle `N+1` de platos por `GetMenuItemsWithModifiersByIdsAsync`, gestionar sockets KDS en `KitchenWebSocketManager` con `ConcurrentDictionary<Guid, WebSocket>` y `Dispose()`, y reutilizar `IConnectionMultiplexer` singleton en `/ready` (`ADR-0018` / `OPT-05`, `OPT-06`).

**Independent Test**: Ejecutar pruebas unitarias de `OrderProcessorWorker`, `CatalogRepository.GetMenuItemsWithModifiersByIdsAsync`, y `KitchenWebSocketManager` verificando que se ejecuta 1 sola consulta por lote y que los sockets cerrados se remueven y liberan inmediatamente.

### Tests for User Story 4

- [X] T021 [P] [US4] Write unit tests for `KitchenWebSocketManager` deterministic socket removal/disposal and batch catalog loading in `tests/RestoCore.UnitTests/Infrastructure/WebSocketAndStreamWorkerOptimizationTests.cs`

### Implementation for User Story 4

- [X] T022 [P] [US4] Implement `GetMenuItemsWithModifiersByIdsAsync` in `src/RestoCore.Infrastructure/Persistence/Repositories/CatalogRepository.cs`
- [X] T023 [P] [US4] Update `RedisOrderQueueService` to register active tenant slugs via `SetAddAsync("restocore:active_tenant_streams", ...)` and implement `GetActiveTenantStreamsAsync` in `src/RestoCore.Infrastructure/Messaging/RedisOrderQueueService.cs`
- [X] T024 [US4] Refactor `OrderProcessorWorker` to read active tenant streams from `IOrderQueueService.GetActiveTenantStreamsAsync` and load items in batch via `GetMenuItemsWithModifiersByIdsAsync` in `src/RestoCore.Infrastructure/BackgroundServices/OrderProcessorWorker.cs`
- [X] T025 [P] [US4] Refactor `KitchenWebSocketManager` to store connections in `ConcurrentDictionary<Guid, WebSocket>` per tenant and remove + `Dispose()` non-open or faulted sockets in `src/RestoCore.Infrastructure/WebSockets/KitchenWebSocketManager.cs`
- [X] T026 [P] [US4] Update `GET /ready` in `HealthEndpoints` to inject and reuse singleton `IConnectionMultiplexer` instead of calling `ConnectionMultiplexer.ConnectAsync` per request in `src/RestoCore.Api/Endpoints/HealthEndpoints.cs`

**Checkpoint**: User Story 4 (`ADR-0018` / `OPT-05`, `OPT-06`) complete and independently verified.

---

## Phase 7: User Story 5 - Gobernanza OpenTelemetry Anti-Redundancia, RFC 7807, Parametrización SSL en SeaweedFS y Sincronización RBAC en OPA (Priority: P2)

**Goal**: Eliminar el middleware manual de `Stopwatch` en `Program.cs`, agregar scope estructurado (`tenant_id`, `tenant_slug`) en `TenantResolutionMiddleware`, sanitizar `GlobalExceptionHandler` bajo RFC 7807 sin PII, parametrizar `Storage:UseSsl` con firma sincrónica en `SeaweedStorageService`, e incluir `permissions` en `OpaUser` con caché de 30s (`ADR-0017` / `OPT-07`, `OPT-09`, `OPT-10`).

**Independent Test**: Ejecutar pruebas unitarias y de seguridad verificando que `SeaweedStorageService` respeta `Storage:UseSsl` sin `Task.Run`, que `GlobalExceptionHandler` emite `ProblemDetails` RFC 7807 con `errorCode` y `traceId` sin exponer PII, y que `OpaClient` envía `permissions` y cachea evaluaciones por 30 segundos.

### Tests for User Story 5

- [X] T027 [P] [US5] Write unit and security tests for `SeaweedStorageService` SSL configuration, `GlobalExceptionHandler` RFC 7807 sanitization, and `OpaClient` `permissions` payload + L1 cache in `tests/RestoCore.UnitTests/Infrastructure/GovernanceAndSecurityOptimizationTests.cs`

### Implementation for User Story 5

- [X] T028 [P] [US5] Update `SeaweedStorageService` to read `Storage:UseSsl` from `IConfiguration` and compute pre-signed URLs synchronously via `Task.FromResult` in `src/RestoCore.Infrastructure/Storage/SeaweedStorageService.cs`
- [X] T029 [P] [US5] Update `OpaUser` to include `[JsonPropertyName("permissions")] public string[] Permissions`, add 30-second `IMemoryCache` evaluation caching in `OpaClient`, and populate permissions in `OpaAuthorizationHandler` in `src/RestoCore.Infrastructure/Authorization/OpaClient.cs` and `src/RestoCore.Infrastructure/Authorization/OpaAuthorizationHandler.cs`
- [X] T030 [P] [US5] Add structured logger scope (`tenant_id`, `tenant_slug`) in `TenantResolutionMiddleware` in `src/RestoCore.Infrastructure/MultiTenancy/TenantResolutionMiddleware.cs`
- [X] T031 [P] [US5] Update `GlobalExceptionHandler` to sanitize error logs (preventing raw `exception.Message` PII leakage) and emit RFC 7807 `ProblemDetails` with `errorCode`, `traceId`, and `type` (aligned with `contracts.md` section 2.2) in `src/RestoCore.Api/Middlewares/GlobalExceptionHandler.cs`
- [X] T032 [US5] Remove redundant inline `Stopwatch` HTTP logging middleware, register `IPublicMenuCacheService`, and register granular OPA policies (`ManageMenu`, `ManageCategories`, `ManageBranding`, `KitchenStock`, `SuperAdminOnly`) in `src/RestoCore.Api/Program.cs`

**Checkpoint**: User Story 5 (`ADR-0017` / `OPT-07`, `OPT-09`, `OPT-10`) complete and independently verified.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: End-to-end validation across all test suites, knowledge graph synchronization, and `compare-docs.ps1` verification.

- [X] T033 Run full test suite (`RestoCore.UnitTests`, `RestoCore.SecurityTests`, `RestoCore.IntegrationTests`) and verify 0 regressions in `RestoCore.slnx`
- [X] T034 Update knowledge graph via `graphify update .` and verify delta synchronization with `.agents/skills/sync-docs-specs/scripts/compare-docs.ps1`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies (`T001`–`T002`).
- **Phase 2 (Foundational)**: Depends on Phase 1 (`T003`–`T006`) — blocks User Stories.
- **Phase 3 (`US1` - P1)**: Depends on Phase 2 (`T007`–`T012`).
- **Phase 4 (`US2` - P1)**: Independent of `US1`, depends on Phase 2 (`T013`–`T017`).
- **Phase 5 (`US3` - P1)**: Independent of `US1`/`US2`, depends on Phase 2 (`T018`–`T020`).
- **Phase 6 (`US4` - P2)**: Depends on Phase 2 (`T021`–`T026`).
- **Phase 7 (`US5` - P2)**: Depends on Phase 2 and `T009` (`T027`–`T032`).
- **Phase 8 (Polish)**: Depends on Phases 3–7 (`T033`–`T034`).

### Parallel Opportunities

- Foundational tasks `T003`, `T004`, `T005`, `T006` touch independent interface files and can run in parallel.
- User Stories `US1`, `US2`, `US3`, and `US4` can be developed in parallel after Phase 2.
- Within `US5`, tasks `T028`, `T029`, `T030`, and `T031` modify separate files and can run in parallel before `T032`.

---

## Parallel Example: User Story 4 (`ADR-0018`)

```bash
Task: "T022 [P] [US4] Implement GetMenuItemsWithModifiersByIdsAsync in src/RestoCore.Infrastructure/Persistence/Repositories/CatalogRepository.cs"
Task: "T023 [P] [US4] Update RedisStreamOrderQueueService in src/RestoCore.Infrastructure/Messaging/RedisStreamOrderQueueService.cs"
Task: "T025 [P] [US4] Refactor KitchenWebSocketManager in src/RestoCore.Infrastructure/WebSockets/KitchenWebSocketManager.cs"
Task: "T026 [P] [US4] Update GET /ready in src/RestoCore.Api/Endpoints/HealthEndpoints.cs"
```

---

## Implementation Strategy

### MVP First (`US1` + `US2` + `US3` - Priority P1)

1. Complete Phase 1 (Setup) and Phase 2 (Foundational).
2. Implement `US1` (`ADR-0015` Hybrid Cache L1/L2 + Soft-Delete Filter), `US2` (`ADR-0014` JSONB `@>` + GIN/B-Tree Indexes), and `US3` (`ADR-0016` Precompiled CQRS Mediator + `ValidationBehavior`).
3. Validate P1 optimizations with unit tests.

### Incremental Delivery (`US4` + `US5` - Priority P2)

4. Implement `US4` (`ADR-0018` Reactive Redis Streams + Batch Catalog Lookup + WebSocket/Redis Lifecycle).
5. Implement `US5` (`ADR-0017` OTel Anti-Redundancy + RFC 7807 + SeaweedFS SSL + OPA `permissions`).
6. Run full verification (`T033`–`T034`).
