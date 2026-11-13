# Feature Specification: Optimizaciones Arquitectónicas de Persistencia JSONB/GIN, Caché Híbrido L1/L2, Mediador CQRS, Gobernanza OpenTelemetry/OPA y Ciclo de Vida de Streams/WebSockets

**Feature Branch**: `feature/012-backend-architecture-optimizations`

**Created**: 2026-11-12

**Status**: Approved from Canonical Docs (`RestoCore-Docs`)

**Source ADRs & Audit References (`RestoCore-Docs`)**:
- `docs/adr/0014-optimizacion-persistencia-jsonb-gin-postgresql.md` (`OPT-04`, `OPT-08`)
- `docs/adr/0015-cache-hibrido-l1-l2-latencia-menu-qr.md` (`OPT-02`, `OPT-03`)
- `docs/adr/0016-optimizacion-mediador-cqrs-pipeline-behaviors.md` (`OPT-01`)
- `docs/adr/0017-gobernanza-telemetria-opentelemetry-y-privacidad.md` (`OPT-07`, `OPT-09`, `OPT-10`)
- `docs/adr/0018-procesamiento-reactivo-streams-y-ciclo-vida-sockets.md` (`OPT-05`, `OPT-06`)
- `docs/audit/diagnostico-optimizaciones-backend.md` (`OPT-01` a `OPT-10`)

**Input**: Sincronización documental mediante `/sync-docs-specs` consolidando las decisiones arquitectónicas `ADR-0014` a `ADR-0018` y el diagnóstico integral de optimizaciones (`OPT-01` a `OPT-10`).

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Caché Híbrido Multinivel L1/L2 y Filtrado Estricto de Baja Suave en Carta QR (Priority: P1)

Como comensal que escanea el código QR de una mesa desde un dispositivo móvil, necesito que la carta pública se sirva desde memoria local L1/L2 con evaluación inmediata de `ETag` (`HTTP 304 Not Modified`), segmentada por `table_token`, e invalidada instantáneamente en todas las réplicas vía Redis Pub/Sub, excluyendo cualquier plato dado de baja lógica (`IsDeleted = true`).

**Why this priority**: Crítico para el Inviolable #2 de `AGENTS.md` (< 2s LCP móvil y procesamiento dinámico < 500ms) y la prevención de exposición de platos eliminados (`ADR-0015`, `OPT-02`, `OPT-03`).

**Independent Test**: Se verifica consultando `GET /api/v1/tenants/{tenantSlug}/menu?table_token=mesa-01`, comprobando almacenamiento en `HybridCache` (L1 60s, L2 Redis 2h) bajo clave `menu:public:{tenantSlug}:{tableToken}`, respuesta `304 Not Modified` ante `If-None-Match` coincidente sin serializar payload, ausencia de platos con `IsDeleted = true`, e invalidación distribuida al modificar el catálogo.

**Acceptance Scenarios**:

1. **Given** un menú público compilado para un tenant y un `table_token` específico, **When** se realizan lecturas concurrentes a `GET /api/v1/tenants/{tenantSlug}/menu`, **Then** el sistema resuelve la respuesta desde el nivel L1/L2 de `HybridCache` en un único acceso atómico (`PublicMenuCacheEntry`) sin múltiples viajes secuenciales a Redis.
2. **Given** un cliente móvil que envía `If-None-Match` coincidente con el `ETag` vigente en caché, **When** consulta el menú público, **Then** el endpoint responde `HTTP 304 Not Modified` en menos de 2 ms sin transferir el cuerpo JSON.
3. **Given** un plato archivado mediante baja lógica (`IsDeleted = true`), **When** `CatalogRepository.GetTenantWithCatalogAsync` construye el grafo del catálogo, **Then** el plato eliminado queda excluido del payload público aunque se utilice `.IgnoreQueryFilters()` para la resolución anónima del tenant.
4. **Given** una actualización de catálogo, perfil o publicación de menú, **When** se confirma la transacción o se invoca `ICdnPurgeService`, **Then** se desaloja la entrada en `HybridCache` y se publica el evento en el canal Redis Pub/Sub `restocore:hybridcache:invalidate` para purgar el nivel L1 de todas las réplicas.

---

### User Story 2 - Persistencia JSONB con Operador Nativo `@>`, Índices GIN/B-Tree Parciales y Normalización de Login (Priority: P1)

Como administrador del restaurante y operador de plataforma, necesito que la verificación de comandas activas antes de archivar un plato se ejecute directamente en PostgreSQL mediante el operador JSONB `@>` con nomenclatura camelCase estandarizada (`menuItemId`), respaldada por un índice GIN sobre `orders.items_snapshot` y un índice B-Tree parcial `ix_orders_active_tenant`, y que el inicio de sesión consulte el índice B-Tree `IX_users_Email` sin invocar `LOWER()` sobre la columna.

**Why this priority**: Erradica la descarga a memoria `O(N)` de todas las órdenes activas y evita falsos negativos por discrepancia de capitalización en JSONB al dar de baja platos con comandas vivas en cocina (`ADR-0014`, `OPT-04`, `OPT-08`).

**Independent Test**: Se verifica serializando `OrderItemSnapshot` con `[JsonPropertyName("menuItemId")]`, comprobando que `OrderRepository.HasActiveOrdersForMenuItemAsync` delega el filtro `@> '[{"menuItemId":"..."}]'::jsonb` a PostgreSQL, y que `UserRepository.GetByEmailAsync` compara directamente contra el email normalizado en minúsculas sin `.ToLower()` en el árbol de expresión LINQ.

**Acceptance Scenarios**:

1. **Given** una orden en estado `pending` o `preparing` cuyo `items_snapshot` contiene un plato específico, **When** se invoca `HasActiveOrdersForMenuItemAsync(tenantId, menuItemId)`, **Then** la consulta evalúa `items_snapshot @> '[{"menuItemId":"..."}]'::jsonb` en base de datos sin materializar la colección de órdenes en memoria de la aplicación.
2. **Given** el esquema de persistencia en `OrderConfiguration` y migraciones de EF Core, **When** se inspecciona el modelo relacional, **Then** existen la extensión `btree_gin`, el índice GIN sobre `orders.items_snapshot` y el índice B-Tree parcial `ix_orders_active_tenant` (`WHERE status NOT IN ('delivered', 'cancelled')`).
3. **Given** un usuario registrado con correo electrónico normalizado en minúsculas, **When** se autentica mediante `UserRepository.GetByEmailAsync(email)` o se busca una categoría por nombre en `CatalogRepository.ExistsCategoryByNameAsync`, **Then** la consulta evita aplicar `.ToLower()` sobre las columnas de base de datos, preservando el uso de índices B-Tree.

---

### User Story 3 - Mediador CQRS Precompilado sin Reflexión y Canalización `IPipelineBehavior` (Priority: P1)

Como arquitecto del sistema, necesito que el mediador CQRS (`IMediator` / `ISender`) despache comandos y consultas mediante invocadores fuertemente tipados cacheados (`IRequestInvoker<TResponse>`) y delegados parametrizados (`RequestHandlerDelegate<in TRequest, TResponse>`), ejecutando FluentValidation a través de un comportamiento transversal `ValidationBehavior<TRequest, TResponse>` sin reflexión dinámica en la ruta crítica.

**Why this priority**: Elimina las llamadas repetitivas a `Type.MakeGenericType`, `Type.GetMethod`, `MethodInfo.Invoke` y el boxing en `Task<object>` en cada petición HTTP (`ADR-0016`, `OPT-01`).

**Independent Test**: Se verifica ejecutando pruebas unitarias sobre `Mediator` con y sin `IPipelineBehavior<TRequest, TResponse>` registrados, confirmando que `ValidationBehavior` ejecuta todos los `IValidator<TRequest>` en paralelo y lanza `ValidationException` ante fallos, y que los handlers se invocan mediante `RequestInvoker<TRequest, TResponse>` cacheado en `ConcurrentDictionary`.

**Acceptance Scenarios**:

1. **Given** un comando o consulta que implementa `IRequest<TResponse>`, **When** se envía a través de `ISender.Send`, **Then** el mediador resuelve y cachea una instancia de `IRequestInvoker<TResponse>` evitando `MethodInfo.Invoke` en ejecuciones subsecuentes.
2. **Given** uno o más `IPipelineBehavior<TRequest, TResponse>` registrados en DI (incluyendo `ValidationBehavior<TRequest, TResponse>`), **When** se envía un comando con datos inválidos, **Then** el pipeline intercepta la solicitud antes del handler y arroja `FluentValidation.ValidationException`.

---

### User Story 4 - Procesamiento Reactivo de Redis Streams, Carga SQL por Lote (`ANY`) y Ciclo de Vida Determinista de WebSockets y Redis (Priority: P2)

Como operador de cocina (KDS) y administrador de infraestructura, necesito que `OrderProcessorWorker` descubra los streams activos desde un conjunto en Redis (`restocore:active_tenant_streams`) en lugar de sondear `db.Tenants` cada 500 ms, cargue todos los platos de una comanda en una sola consulta SQL por lote (`GetMenuItemsWithModifiersByIdsAsync`), libere inmediatamente los `WebSocket` cerrados usando `ConcurrentDictionary<Guid, WebSocket>`, y reutilice `IConnectionMultiplexer` singleton en `/ready`.

**Why this priority**: Elimina las 120 consultas SQL ociosas por minuto a PostgreSQL, erradica el problema `N+1` al consolidar pedidos de múltiples platos, y corrige las fugas de memoria y puertos TCP efímeros (`ADR-0018`, `OPT-05`, `OPT-06`).

**Independent Test**: Se verifica comprobando que `OrderSubmissionService` registra el `tenantSlug` en el Redis Set `restocore:active_tenant_streams`, que `OrderProcessorWorker` invoca `catalogRepo.GetMenuItemsWithModifiersByIdsAsync` una única vez por pedido, que `KitchenWebSocketManager` remueve y hace `Dispose()` de sockets cerrados en `O(1)`, y que `/ready` utiliza `IConnectionMultiplexer` inyectado.

**Acceptance Scenarios**:

1. **Given** el sistema en reposo sin órdenes nuevas, **When** `OrderProcessorWorker` ejecuta su ciclo de lectura, **Then** consulta los tenants activos desde Redis (`restocore:active_tenant_streams`) generando 0 consultas SQL contra `db.Tenants`.
2. **Given** una comanda entrante con 10 ítems distintos, **When** `OrderProcessorWorker.ProcessOrderMessageAsync` valida los platos y modificadores, **Then** ejecuta una única consulta por lote mediante `ICatalogRepository.GetMenuItemsWithModifiersByIdsAsync`.
3. **Given** una terminal KDS cuya conexión WebSocket se cierra o falla durante `BroadcastAsync`, **When** `KitchenWebSocketManager` detecta `State != WebSocketState.Open` o una excepción de envío, **Then** remueve atómicamente el socket del `ConcurrentDictionary<Guid, WebSocket>` e invoca `Dispose()`.
4. **Given** sondas periódicas de salud sobre `GET /ready`, **When** el endpoint verifica la conectividad con Redis, **Then** reutiliza la instancia singleton `IConnectionMultiplexer` sin invocar `ConnectionMultiplexer.ConnectAsync`.

---

### User Story 5 - Gobernanza OpenTelemetry Anti-Redundancia, RFC 7807, Parametrización SSL en SeaweedFS y Sincronización RBAC en OPA (Priority: P2)

Como responsable de seguridad y observabilidad, necesito que se elimine el middleware manual de `Stopwatch` en `Program.cs`, se enriquezca el alcance de logs con `tenant_id` y `tenant_slug` en `TenantResolutionMiddleware`, se saniticen las excepciones bajo formato estándar RFC 7807 (`ProblemDetails`) sin exponer PII, se parametrice `Storage:UseSsl` con firma sincrónica en `SeaweedStorageService`, y se incluya la propiedad `permissions` en `OpaUser` con políticas granulares acordes a `authz.rego`.

**Why this priority**: Garantiza el cumplimiento estricto de los Inviolables #4 y #5 de `AGENTS.md` y sincroniza el contrato C# con la política declarativa Rego (`ADR-0017`, `OPT-07`, `OPT-09`, `OPT-10`).

**Independent Test**: Se verifica comprobando la ausencia de logs de inicio/fin de duración en `Program.cs`, respuestas `application/problem+json` con `errorCode` y `traceId` desde `GlobalExceptionHandler`, generación de URLs `https://` cuando `Storage:UseSsl = true` sin `Task.Run`, y serialización de `permissions` en `OpaUser`.

**Acceptance Scenarios**:

1. **Given** cualquier solicitud HTTP procesada por la API, **When** finaliza su ejecución, **Then** la duración se registra exclusivamente en el span de OpenTelemetry sin emitir logs manuales redundantes con `Stopwatch`.
2. **Given** una excepción no controlada o de validación de negocio en un endpoint, **When** `GlobalExceptionHandler` procesa el fallo, **Then** emite un log estructurado con `ErrorCode`, `StatusCode` y `TenantId` (sin volcar PII de `exception.Message`) y retorna `ProblemDetails` RFC 7807 con `errorCode` y `traceId`.
3. **Given** la configuración `Storage:UseSsl = true`, **When** `SeaweedStorageService.GeneratePresignedUploadUrlAsync` firma una URL de carga, **Then** ejecuta `_s3Client.GetPreSignedURL` de forma sincrónica sin `Task.Run` y retorna una URL con esquema `https://`.
4. **Given** un usuario autenticado con claims de rol y permisos (`permission`), **When** `OpaAuthorizationHandler` consulta a OPA a través de `OpaClient`, **Then** el payload `OpaUser` incluye `permissions` sincronizado con `specs/policies/authz.rego`.

---

### Edge Cases

- **Redis no disponible en entorno de pruebas o contingencia**: `HybridCache` continúa operando sobre el nivel L1 en memoria local (`IMemoryCache`) y el backplane Redis Pub/Sub degrada de forma segura sin interrumpir las lecturas de la carta QR.
- **Formato de `table_token` nulo o con espacios**: Se normaliza a `"default"` en la clave `menu:public:{tenantSlug}:{token}` para evitar fragmentación de caché.
- **Conjunto `restocore:active_tenant_streams` vacío tras reinicio de Redis**: Si el conjunto en Redis está vacío al iniciar `OrderProcessorWorker`, realiza una única hidratación inicial de slugs activos y, en adelante, `OrderSubmissionService` mantiene el conjunto actualizado mediante `SADD` en cada nueva comanda.
- **Permisos implícitos por rol en tokens JWT legados**: Si el JWT contiene el rol (`owner`, `admin`, `cook`, `waiter`) pero no claims explícitos `permission`, `OpaAuthorizationHandler` proyecta los permisos canónicos del rol además de los claims `permission` presentes, manteniendo compatibilidad total con tokens emitidos previamente.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001 (`ADR-0016` / `OPT-01`)**: El sistema DEBE reemplazar la resolución por reflexión en `Mediator.cs` por invocadores genéricos fuertemente tipados (`IRequestInvoker<TResponse>` / `RequestInvoker<TRequest, TResponse>`) cacheados en un `ConcurrentDictionary<Type, object>`.
- **FR-002 (`ADR-0016` / `OPT-01`)**: El sistema DEBE definir `IPipelineBehavior<in TRequest, TResponse>` y `RequestHandlerDelegate<in TRequest, TResponse>`, extrayendo la validación de FluentValidation hacia `ValidationBehavior<TRequest, TResponse>`.
- **FR-003 (`ADR-0015` / `OPT-02`)**: El endpoint `GET /api/v1/tenants/{tenantSlug}/menu` DEBE utilizar una estrategia de caché híbrido multinivel L1/L2 (`IPublicMenuCacheService` / `PublicMenuHybridCacheService` respaldado por `IMemoryCache` con TTL L1 de 60 segundos e `IDistributedCache` en Redis con TTL L2 de 2 horas), empaquetando `ETag` y `PayloadJson` en `PublicMenuCacheEntry` bajo la clave segmentada `menu:public:{tenantSlug}:{tableToken ?? "default"}` y respondiendo `HTTP 304 Not Modified` cuando `If-None-Match` coincida.
- **FR-004 (`ADR-0015` / `OPT-02`)**: La invalidación de caché de catálogo (`ICdnPurgeService` y `ApplicationDbContext`) DEBE desalojar las entradas locales y distribuidas mediante `IPublicMenuCacheService.InvalidateTenantMenuAsync` y difundir el mensaje de invalidación mediante Redis Pub/Sub (`restocore:hybridcache:invalidate`) para sincronizar el nivel L1 entre múltiples réplicas.
- **FR-005 (`ADR-0015` / `OPT-03`)**: `CatalogRepository.GetTenantWithCatalogAsync` DEBE filtrar explícitamente las categorías activas (`c.IsActive`) y los platos no eliminados (`!i.IsDeleted`) aun cuando emplee `.IgnoreQueryFilters()`.
- **FR-006 (`ADR-0014` / `OPT-04`)**: `OrderItemSnapshot` y `OrderModifierSnapshot` en `Order.cs` DEBEN anotar todas sus propiedades con `[JsonPropertyName]` en formato camelCase explícito (incluyendo `[JsonPropertyName("menuItemId")]`).
- **FR-007 (`ADR-0014` / `OPT-04`)**: `OrderRepository.HasActiveOrdersForMenuItemAsync` DEBE evaluar la existencia de comandas activas directamente en PostgreSQL mediante el operador JSONB `@>` (`items_snapshot @> '[{"menuItemId":"..."}]'::jsonb`), respaldado por un índice GIN sobre `orders.items_snapshot` y el índice B-Tree parcial `ix_orders_active_tenant` en `OrderConfiguration` / migraciones.
- **FR-008 (`ADR-0014` / `OPT-08`)**: `UserRepository.GetByEmailAsync` y `CatalogRepository` DEBEN normalizar las cadenas de entrada en memoria (`Trim().ToLowerInvariant()`) y evitar invocar `.ToLower()` sobre columnas indexadas en consultas LINQ a PostgreSQL.
- **FR-009 (`ADR-0018` / `OPT-05`)**: `OrderSubmissionService` DEBE registrar el `tenantSlug` en el conjunto de Redis `restocore:active_tenant_streams` al encolar un pedido, y `OrderProcessorWorker` DEBE consultar dicho conjunto en lugar de ejecutar `db.Tenants.Select(t => t.Slug).ToList()` cada 500 ms.
- **FR-010 (`ADR-0018` / `OPT-05`)**: `ICatalogRepository` DEBE exponer `GetMenuItemsWithModifiersByIdsAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken ct)` y `OrderProcessorWorker` DEBE utilizarlo para validar y enriquecer todos los ítems de un pedido en una única consulta SQL por lote.
- **FR-011 (`ADR-0018` / `OPT-06`)**: `KitchenWebSocketManager` DEBE almacenar las conexiones en un `ConcurrentDictionary` indexado por identificador de conexión, removiendo atómicamente (`TryRemove`) e invocando `Dispose()` sobre cualquier socket cuyo estado no sea `WebSocketState.Open` o falle al transmitir.
- **FR-012 (`ADR-0018` / `OPT-06`)**: El endpoint `GET /ready` en `HealthEndpoints.cs` DEBE inyectar y reutilizar `IConnectionMultiplexer` singleton desde el contenedor DI sin abrir nuevas conexiones TCP con `ConnectionMultiplexer.ConnectAsync`.
- **FR-013 (`ADR-0017` / `OPT-07`)**: Se DEBE eliminar el middleware manual de `Stopwatch` en `Program.cs`, agregar un `ILogger.BeginScope` con `tenant_id` y `tenant_slug` en `TenantResolutionMiddleware`, sanitizar `GlobalExceptionHandler` para no volcar PII en los logs, y estandarizar las respuestas de error bajo RFC 7807 (`ProblemDetails`).
- **FR-014 (`ADR-0017` / `OPT-09`)**: `SeaweedStorageService` DEBE parametrizar el uso de HTTPS mediante `Storage:UseSsl` en `IConfiguration` y calcular la URL pre-firmada de forma sincrónica (`Task.FromResult`) sin envolver `_s3Client.GetPreSignedURL` en `Task.Run`.
- **FR-015 (`ADR-0017` / `OPT-10`)**: `OpaUser` en `OpaClient.cs` DEBE incluir `[JsonPropertyName("permissions")] public string[] Permissions`, `OpaAuthorizationHandler` DEBE poblar los permisos del usuario, y `OpaClient` DEBE cachear temporalmente en memoria (30s) las decisiones de autorización por clave compuesta `(userId, tenantId, action, resource)`.

### Key Entities

- **`PublicMenuCacheEntry`**: Registro inmutable (`ETag`, `PayloadJson`) almacenado atómicamente en `HybridCache` (L1/L2) bajo la clave `menu:public:{tenantSlug}:{tableToken}`.
- **`IRequestInvoker<TResponse>` / `IPipelineBehavior<TRequest, TResponse>`**: Contratos de despacho CQRS fuertemente tipados y canalización transversal de comportamientos en `RestoCore.Application`.
- **`OrderItemSnapshot`**: Modelo de ítem dentro del snapshot JSONB de una comanda con atributos `[JsonPropertyName]` en camelCase estricto (`menuItemId`, `name`, `quantity`, `unitPrice`, `modifiers`, `instructions`).
- **`OpaUser`**: Contrato de identidad enviado al motor OPA con `id`, `tenant_id`, `roles` y `permissions`.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El despacho interno de comandos y consultas en `Mediator.Send` ejecuta 0 llamadas a `MethodInfo.Invoke` o `Type.GetMethod` tras el primer envío de cada tipo de solicitud.
- **SC-002**: El endpoint `GET /api/v1/tenants/{tenantSlug}/menu` resuelve aciertos de caché L1 y validaciones `If-None-Match` (`HTTP 304`) en menos de 2 ms sin viajes de red a Redis ni serialización repetitiva.
- **SC-003**: `OrderRepository.HasActiveOrdersForMenuItemAsync` transfiere un único valor booleano escalar desde PostgreSQL utilizando el operador `@>`, sin cargar entidades `Order` en la memoria de la API.
- **SC-004**: En estado de reposo sin pedidos nuevos, `OrderProcessorWorker` realiza 0 consultas SQL por minuto contra la tabla `tenants`, y al procesar una comanda con $M$ platos ejecuta exactamente 1 consulta SQL de lectura de catálogo.
- **SC-005**: Las desconexiones de clientes WebSocket en `KitchenWebSocketManager` reducen inmediatamente el conteo de entradas activas en el diccionario y liberan la instancia `WebSocket`, y las llamadas a `/ready` mantienen 1 única conexión TCP multiplexada hacia Redis.
- **SC-006**: El 100% de las pruebas unitarias, de seguridad e integración existentes y nuevas para `OPT-01` a `OPT-10` compilan y pasan sin regresiones.

---

## Assumptions

- En entornos donde no exista una instancia de Redis configurada (o durante pruebas unitarias aisladas), `HybridCache` utiliza el almacenamiento L1 en memoria local y el suscriptor Pub/Sub opera como no-op seguro.
- El proveedor de EF Core (`Npgsql`) soporta la traducción de `.Where(m => itemIds.Contains(m.Id))` al operador nativo `= ANY(@itemIds)` de PostgreSQL, mientras que en proveedores en memoria (para pruebas unitarias) se evalúa de manera transparente con `Contains`.
