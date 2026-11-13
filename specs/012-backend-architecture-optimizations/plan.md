# Implementation Plan: Optimizaciones Arquitectónicas de Persistencia JSONB/GIN, Caché Híbrido L1/L2, Mediador CQRS, Gobernanza OpenTelemetry/OPA y Ciclo de Vida de Streams/WebSockets

**Branch**: `feature/012-backend-architecture-optimizations` | **Date**: 2026-11-12 | **Spec**: [spec.md](file:///r:/Programando/RestoCore/resto-core-back/specs/012-backend-architecture-optimizations/spec.md)

**Input**: Feature specification from `/specs/012-backend-architecture-optimizations/spec.md` (`ADR-0014`, `ADR-0015`, `ADR-0016`, `ADR-0017`, `ADR-0018`; `OPT-01` a `OPT-10`).

---

## Summary

Implementar las 10 optimizaciones arquitectónicas aprobadas en `RestoCore-Docs` (`ADR-0014` a `ADR-0018`):
1. **Despacho CQRS Precompilado (`ADR-0016` / `OPT-01`)**: Sustituir la reflexión repetitiva en `Mediator.cs` por invocadores genéricos fuertemente tipados (`IRequestInvoker<TResponse>` / `RequestInvoker<TRequest, TResponse>`) cacheados en `ConcurrentDictionary` e introducir la canalización `IPipelineBehavior<TRequest, TResponse>` con `ValidationBehavior<TRequest, TResponse>`.
2. **Caché Híbrido Multinivel L1/L2 y Filtrado de Baja Suave (`ADR-0015` / `OPT-02`, `OPT-03`)**: Incorporar caché híbrido L1 (`IMemoryCache`, 60s) + L2 (`IDistributedCache` Redis, 2h) con backplane Redis Pub/Sub (`restocore:hybridcache:invalidate`), clave segmentada por `table_token`, respuesta `304 Not Modified` desde L1, y filtrado estricto de `!IsDeleted` en `CatalogRepository.GetTenantWithCatalogAsync`.
3. **Persistencia JSONB con `@>`, Índices GIN/B-Tree Parciales y Normalización de Login (`ADR-0014` / `OPT-04`, `OPT-08`)**: Estandarizar `[JsonPropertyName]` en camelCase en `OrderItemSnapshot`/`OrderModifierSnapshot`, delegar `HasActiveOrdersForMenuItemAsync` al operador `@>` de PostgreSQL con índice GIN e índice B-Tree parcial `ix_orders_active_tenant`, y suprimir `.ToLower()` sobre columnas indexadas en `UserRepository` y `CatalogRepository`.
4. **Procesamiento Reactivo de Redis Streams, Proyección por Lote y Ciclo de Vida de Sockets/Redis (`ADR-0018` / `OPT-05`, `OPT-06`)**: Reemplazar el sondeo SQL cada 500ms a `db.Tenants` por el Redis Set `restocore:active_tenant_streams`, agregar `GetMenuItemsWithModifiersByIdsAsync` para eliminar consultas `N+1`, gestionar conexiones WebSocket con `ConcurrentDictionary<Guid, WebSocket>` y liberación determinista (`Dispose()`), e inyectar `IConnectionMultiplexer` singleton en `/ready`.
5. **Gobernanza OpenTelemetry, RFC 7807, SeaweedFS SSL y OPA RBAC (`ADR-0017` / `OPT-07`, `OPT-09`, `OPT-10`)**: Eliminar el middleware manual de `Stopwatch` en `Program.cs`, agregar scope estructurado en `TenantResolutionMiddleware`, estandarizar `GlobalExceptionHandler` bajo RFC 7807 sin PII, parametrizar `Storage:UseSsl` con firma sincrónica en `SeaweedStorageService`, e incluir `permissions` en `OpaUser` con caché de evaluación de 30s.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10 (`net10.0`)

**Primary Dependencies**: ASP.NET Core Minimal APIs, Entity Framework Core 10 (`Npgsql.EntityFrameworkCore.PostgreSQL`), `StackExchange.Redis`, `AWSSDK.S3`, `FluentValidation`, OpenTelemetry (`OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`)

**Storage**: PostgreSQL 16+ (con `JSONB`, índices `GIN` e índices `B-Tree` parciales), Redis 7+ (Streams, Pub/Sub y caché L2), SeaweedFS S3

**Testing**: xUnit, Moq, FluentAssertions (`RestoCore.UnitTests`, `RestoCore.SecurityTests`, `RestoCore.IntegrationTests`)

**Target Platform**: Linux / Windows Server (contenedores Docker y ejecución local .NET 10)

**Project Type**: Web Service / REST API Backend bajo Clean Architecture y CQRS

**Performance Goals**:
- Despacho CQRS interno `< 0.05 ms` con 0 llamadas a `MethodInfo.Invoke` tras calentamiento.
- TTFB en aciertos L1 de Carta QR `< 2 ms` (< 2s LCP móvil global).
- Validación de órdenes activas por plato `< 2 ms` mediante operador `@>` e índices GIN/B-Tree parciales.
- 0 consultas SQL por minuto en reposo desde `OrderProcessorWorker` y 1 sola consulta SQL por lote al procesar comandas.

**Constraints**:
- Cumplimiento estricto de los 7 pilares inviolables de `AGENTS.md` y la Constitución de RestoCore.
- Compatibilidad total con la suite existente de pruebas unitarias, de seguridad e integración.

**Scale/Scope**: Multi-tenant SaaS y Self-Hosted con cientos de lecturas QR concurrentes y despacho de comandas en tiempo real.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate Evaluation | Status |
|---|---|---|
| **I. Spec-Driven Development** | Especificado en `specs/012-backend-architecture-optimizations/spec.md` a partir de `ADR-0014` a `ADR-0018` de `RestoCore-Docs`. | PASS |
| **II. Multi-Tenancy & Declarative Auth** | Sincroniza `OpaUser.Permissions` con `specs/policies/authz.rego` y preserva aislamiento estricto por `tenant_id` / `tenant_slug`. | PASS |
| **III. Clean Architecture & Cohesion** | Mantiene contratos en `RestoCore.Application` (`IPipelineBehavior`, `ICatalogRepository`, `IOrderQueueService`) e implementaciones en `RestoCore.Infrastructure`. | PASS |
| **IV. Latency Budget & Async Decoupling** | Reduce TTFB del menú público con caché híbrido L1/L2, elimina `Task.Run` innecesario en URLs pre-firmadas de SeaweedFS y optimiza workers de Redis Streams. | PASS |
| **V. Relational & Flexible Persistence** | Configura índice `GIN` sobre `orders.items_snapshot`, índice B-Tree parcial `ix_orders_active_tenant` y operador nativo `@>`. | PASS |
| **VI. Test-Driven Quality (TDD)** | Incluye pruebas unitarias y de seguridad dedicadas para cada optimización (`OPT-01` a `OPT-10`). | PASS |

---

## Project Structure

### Documentation (this feature)

```text
specs/012-backend-architecture-optimizations/
├── plan.md              # Implementation plan (/speckit-plan)
├── research.md          # Phase 0 technical research & decisions
├── data-model.md        # Phase 1 data models, indexes, and cache schemas
├── quickstart.md        # Phase 1 verification and execution guide
├── contracts/
│   └── contracts.md     # Phase 1 REST, OPA, and internal C# contracts
└── tasks.md             # Phase 2 ordered task breakdown (/speckit-tasks)
```

### Source Code (repository root)

```text
src/
├── RestoCore.Domain/
│   └── Entities/
│       ├── Order.cs                                  # [OPT-04] [JsonPropertyName] camelCase en OrderItemSnapshot/OrderModifierSnapshot
│       └── User.cs                                   # [OPT-08] Normalización de Email en minúsculas
├── RestoCore.Application/
│   └── Common/
│       ├── Behaviors/
│       │   └── ValidationBehavior.cs                 # [OPT-01] Pipeline behavior desacoplado para FluentValidation
│       ├── Caching/
│       │   └── IPublicMenuCacheService.cs            # [OPT-02] Abstracción de caché híbrido L1/L2 para carta QR
│       ├── Interfaces/
│       │   ├── IOrderQueueService.cs                 # [OPT-05] GetActiveTenantStreamsAsync
│       │   └── Repositories/
│       │       └── ICatalogRepository.cs             # [OPT-05] GetMenuItemsWithModifiersByIdsAsync
│       └── Mediator/
│           ├── IPipelineBehavior.cs                  # [OPT-01] Contrato de pipeline behavior y delegado tipado
│           ├── IRequestInvoker.cs                    # [OPT-01] Invocadores genéricos cacheados sin reflexión
│           └── Mediator.cs                           # [OPT-01] Despachador precompilado y registro DI
├── RestoCore.Infrastructure/
│   ├── Authorization/
│   │   ├── OpaAuthorizationHandler.cs                # [OPT-10] Mapeo de permissions desde claims/roles
│   │   └── OpaClient.cs                              # [OPT-10] Propiedad Permissions en OpaUser y caché L1 de 30s
│   ├── BackgroundServices/
│   │   └── OrderProcessorWorker.cs                   # [OPT-05] Descubrimiento reactivo desde Redis y consulta por lote
│   ├── Caching/
│   │   ├── CdnPurgeService.cs                        # [OPT-02] Invalidación coordinada L1/L2 y publicación Redis Pub/Sub
│   │   └── PublicMenuHybridCacheService.cs           # [OPT-02] Caché híbrido L1 (IMemoryCache) + L2 (Redis) + Pub/Sub
│   ├── Messaging/
│   │   └── RedisStreamOrderQueueService.cs           # [OPT-05] SetAdd en restocore:active_tenant_streams y lectura de streams activos
│   ├── MultiTenancy/
│   │   └── TenantResolutionMiddleware.cs             # [OPT-07] Enriquecimiento ILogger.BeginScope con tenant_id y tenant_slug
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs                   # [OPT-02] Invalidación coordinada de caché L1/L2 al guardar cambios
│   │   ├── Configurations/
│   │   │   └── OrderConfiguration.cs                 # [OPT-04] Índice GIN sobre ItemsSnapshot e índice parcial ix_orders_active_tenant
│   │   └── Repositories/
│   │       ├── CatalogRepository.cs                  # [OPT-03, OPT-05, OPT-08] Filtro !IsDeleted, consulta por lote e igualdad normalizada
│   │       ├── OrderRepository.cs                    # [OPT-04] Consulta nativa @> JSONB para órdenes activas
│   │       └── UserRepository.cs                     # [OPT-08] Consulta B-Tree directa sin .ToLower()
│   ├── Storage/
│   │   └── SeaweedStorageService.cs                  # [OPT-09] Parametrización Storage:UseSsl y firma sincrónica sin Task.Run
│   └── WebSockets/
│       └── KitchenWebSocketManager.cs                # [OPT-06] ConcurrentDictionary<Guid, WebSocket> con remoción y Dispose()
└── RestoCore.Api/
    ├── Endpoints/
    │   ├── AdminCatalogEndpoints.cs                  # [OPT-07] Estandarización de errores delegados a RFC 7807
    │   ├── HealthEndpoints.cs                        # [OPT-06] Inyección de IConnectionMultiplexer singleton en /ready
    │   └── PublicMenuEndpoints.cs                    # [OPT-02] Resolución con caché híbrido L1/L2 y 304 Not Modified inmediato
    ├── Middleware/
    │   └── GlobalExceptionHandler.cs                 # [OPT-07] Sanitización anti-PII y formato RFC 7807 con errorCode/traceId
    └── Program.cs                                    # [OPT-07, OPT-10] Eliminación de middleware Stopwatch y registro de políticas OPA

tests/
├── RestoCore.UnitTests/                              # Pruebas unitarias de OPT-01 a OPT-10
└── RestoCore.SecurityTests/                          # Pruebas de autorización OPA y políticas granulares
```

**Structure Decision**: Se preserva la estructura multicapa de Clean Architecture (`RestoCore.Domain`, `RestoCore.Application`, `RestoCore.Infrastructure`, `RestoCore.Api`), ubicando las abstracciones de mediador y caché en `RestoCore.Application` y las implementaciones de persistencia, Redis, WebSockets, SeaweedFS y OPA en `RestoCore.Infrastructure`.

---

## Complexity Tracking

No existen violaciones a la Constitución de RestoCore. Todas las optimizaciones refuerzan directamente los principios constitucionales I a VI.
