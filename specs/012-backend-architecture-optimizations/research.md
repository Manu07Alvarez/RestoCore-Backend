# Phase 0 Research: Optimizaciones Arquitectónicas del Backend (`012-backend-architecture-optimizations`)

**Date**: 2026-11-12 | **Branch**: `feature/012-backend-architecture-optimizations`

Este documento consolida la investigación técnica y decisiones de diseño correspondientes a `ADR-0014`, `ADR-0015`, `ADR-0016`, `ADR-0017` y `ADR-0018` (`OPT-01` a `OPT-10`).

---

## 1. Despacho CQRS Precompilado y `IPipelineBehavior` (`ADR-0016` / `OPT-01`)

- **Decision**: Implementar `IRequestInvoker<TResponse>` y `RequestInvoker<TRequest, TResponse>` cacheados en `ConcurrentDictionary<Type, object>` dentro de `Mediator.cs`, junto con `IPipelineBehavior<in TRequest, TResponse>` y `ValidationBehavior<TRequest, TResponse>` utilizando el delegado parametrizado `RequestHandlerDelegate<in TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)`.
- **Rationale**:
  - La implementación previa en `Mediator.cs` invocaba `Type.MakeGenericType`, `Type.GetMethod` y `MethodInfo.Invoke` en cada llamada a `Send<TResponse>`, asignando arreglos `object[]` y haciendo reflexión sobre `Task.Result`.
  - Con `RequestInvoker<TRequest, TResponse>`, `Activator.CreateInstance` se ejecuta una única vez por tipo de comando/consulta durante la vida del proceso, y todas las ejecuciones posteriores despachan de forma fuertemente tipada (`(TRequest)request`) sin reflexión ni boxing.
- **Alternatives considered**:
  - *Migrar al paquete NuGet externo `MediatR`*: Descartado por licenciamiento comercial restrictivo en versiones recientes y por introducir dependencias de terceros innecesarias en `RestoCore.Application`.
  - *Roslyn Source Generators*: Descartado por sobrecarga de mantenimiento en compilación frente al costo prácticamente nulo (`O(1)` lookup en `ConcurrentDictionary`) de `RequestInvoker<TRequest, TResponse>`.

---

## 2. Caché Híbrido L1/L2 y Filtrado de Baja Suave en Carta QR (`ADR-0015` / `OPT-02`, `OPT-03`)

- **Decision**:
  1. Incorporar un servicio/coordinador de caché híbrido multinivel (`IMemoryCache` L1 con TTL de 60s + `IDistributedCache` L2 Redis con TTL de 2h y sincronización por `SemaphoreSlim` por clave contra *cache stampede*) que almacene atómicamente `PublicMenuCacheEntry(string ETag, string PayloadJson)` bajo la clave `menu:public:{tenantSlug}:{tableToken ?? "default"}`, acompañado de invalidación distribuida vía Redis Pub/Sub (`restocore:hybridcache:invalidate`).
  2. Filtrar explícitamente `!i.IsDeleted` en `CatalogRepository.GetTenantWithCatalogAsync` dentro de `.Include(c => c.Items.Where(i => !i.IsDeleted && i.IsAvailable))` y en la proyección en memoria posterior.
- **Rationale**:
  - Elimina los 3 viajes secuenciales a Redis por cada lectura de la carta QR (`version`, `etag`, `payload`), respondiendo desde memoria L1 en `< 0.5 ms` y evaluando `If-None-Match` (`HTTP 304 Not Modified`) de forma inmediata.
  - Garantiza compatibilidad tanto con PostgreSQL/Redis reales como con entornos de pruebas donde Redis no esté conectado.
- **Alternatives considered**:
  - *Solo `IMemoryCache` sin invalidación distribuida*: Descartado por servir datos obsoletos entre réplicas tras publicar cambios de catálogo.
  - *Múltiples claves separadas en Redis*: Descartado por mantener 2–3 round-trips de red en cada request.

---

## 3. Índices GIN, Operador JSONB `@>` con camelCase y Normalización B-Tree (`ADR-0014` / `OPT-04`, `OPT-08`)

- **Decision**:
  1. Anotar explícitamente `OrderItemSnapshot` y `OrderModifierSnapshot` en `Order.cs` con atributos `[JsonPropertyName]` en camelCase (`menuItemId`, `name`, `quantity`, `unitPrice`, `modifiers`, `instructions`, `optionId`, `priceDelta`).
  2. En `OrderRepository.HasActiveOrdersForMenuItemAsync`:
     - Cuando el proveedor de base de datos es relacional Npgsql (`_context.Database.IsNpgsql()`), ejecutar la consulta nativa con `FromSqlInterpolated` y el operador JSONB `@>` (`items_snapshot @> {filterPayload}::jsonb` con `[{"menuItemId":"..."}]`).
     - Cuando se ejecuta sobre el proveedor InMemory en pruebas unitarias, mantener el fallback LINQ en memoria para preservar la portabilidad del conjunto de pruebas.
  3. Configurar en `OrderConfiguration.cs` el índice GIN sobre `ItemsSnapshot` (`HasMethod("gin")`) y el índice parcial filtrado `ix_orders_active_tenant` sobre `TenantId` (`HasFilter("status NOT IN ('delivered', 'cancelled')")`).
  4. Eliminar `.ToLower()` sobre columnas de base de datos en `UserRepository.GetByEmailAsync` y `CatalogRepository.ExistsCategoryByNameAsync` normalizando previamente el valor buscado.
- **Rationale**:
  - Evita descargar todas las órdenes activas a la memoria RAM del servidor (`O(N)`) y previene falsos negativos en `@>` ocasionados por diferencias de capitalización (`MenuItemId` vs `menuItemId`).
- **Alternatives considered**:
  - *Normalizar `items_snapshot` en tablas relacionales `order_items`*: Descartado por violar `ADR-0003` y `ADR-0010`.

---

## 4. Redis Streams Reactivo, Consulta por Lote (`ANY`) y Ciclo de Vida de Sockets/Redis (`ADR-0018` / `OPT-05`, `OPT-06`)

- **Decision**:
  1. Ampliar `IOrderQueueService` con `GetActiveTenantStreamsAsync(CancellationToken ct)` (respaldado por el Redis Set `restocore:active_tenant_streams`, alimentado en `EnqueueOrderAsync` mediante `SetAddAsync`, con hidratación inicial única si está vacío) para que `OrderProcessorWorker` no ejecute `db.Tenants.Select(t => t.Slug).ToList()` cada 500 ms.
  2. Agregar `GetMenuItemsWithModifiersByIdsAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken ct)` en `ICatalogRepository` y `CatalogRepository`, reemplazando el bucle `N+1` en `OrderProcessorWorker.ProcessOrderMessageAsync`.
  3. Reemplazar `ConcurrentBag<WebSocket>` en `KitchenWebSocketManager` por `ConcurrentDictionary<Guid, WebSocket>` por tenant, eliminando e invocando `Dispose()` inmediatamente sobre cualquier socket cerrado o fallido.
  4. Inyectar `IConnectionMultiplexer` singleton en `HealthEndpoints.cs` (`/ready`) en lugar de abrir una nueva conexión con `ConnectionMultiplexer.ConnectAsync` en cada healthcheck.
- **Rationale**:
  - Erradica las 120 consultas SQL por minuto en reposo, reduce de $M$ consultas a 1 consulta SQL por pedido, y elimina las fugas de descriptores WebSocket y puertos efímeros TCP.
- **Alternatives considered**:
  - *Incrementar el `Task.Delay` del worker a 5 segundos*: Descartado por degradar la latencia de entrega de comandas a cocina.

---

## 5. Gobernanza OpenTelemetry, RFC 7807, SeaweedFS SSL y OPA RBAC (`ADR-0017` / `OPT-07`, `OPT-09`, `OPT-10`)

- **Decision**:
  1. Eliminar el middleware inline de `Stopwatch` en `Program.cs` (que emitía `"HTTP {Method} {Path} received"` y `"completed ... in {ElapsedMs}ms"`), delegando la medición de latencia exclusivamente a los spans de OpenTelemetry (Inviolable #5).
  2. Abrir `ILogger.BeginScope` con `tenant_id` y `tenant_slug` en `TenantResolutionMiddleware`.
  3. Actualizar `GlobalExceptionHandler` para emitir respuestas RFC 7807 (`application/problem+json`) con `errorCode` y `traceId` sin registrar mensajes de excepción crudos que puedan contener PII.
  4. Parametrizar `Storage:UseSsl` en `SeaweedStorageService` y ejecutar `_s3Client.GetPreSignedURL(request)` de forma sincrónica (`Task.FromResult`) sin `Task.Run`.
  5. Añadir `[JsonPropertyName("permissions")] public string[] Permissions` a `OpaUser` en `OpaClient.cs`, poblar los permisos en `OpaAuthorizationHandler` (desde claims `permission` e inferencia canónica por rol), agregar caché en memoria de 30s en `OpaClient`, y registrar las políticas granulares (`ManageMenu`, `ManageCategories`, `ManageBranding`, `KitchenStock`, `SuperAdminOnly`).
- **Rationale**:
  - Cumple el 100% de las reglas de gobernanza de `AGENTS.md` y sincroniza el modelo C# con `specs/policies/authz.rego`.
- **Alternatives considered**:
  - *Silenciar logs con `LogLevel.Warning`*: Descartado por mantener código prohibido por la política anti-redundancia.
