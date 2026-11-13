# Quickstart: Verificación de Optimizaciones Arquitectónicas (`012-backend-architecture-optimizations`)

**Date**: 2026-11-12 | **Branch**: `feature/012-backend-architecture-optimizations`

---

## 1. Compilación de la Solución

```powershell
dotnet build RestoCore.slnx --nologo
```

## 2. Ejecución de Pruebas Unitarias y de Seguridad (`OPT-01` a `OPT-10`)

```powershell
dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj --nologo
dotnet test tests/RestoCore.SecurityTests/RestoCore.SecurityTests.csproj --nologo
```

## 3. Verificaciones Específicas por ADR

1. **`ADR-0016` (`OPT-01` - Mediador CQRS Precompilado y `ValidationBehavior`)**:
   - Ejecutar `dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj --filter "FullyQualifiedName~Mediator"` para verificar el despacho tipado sin reflexión y la interceptación de `ValidationBehavior`.
2. **`ADR-0015` (`OPT-02`, `OPT-03` - Caché Híbrido L1/L2 y Filtrado de Baja Suave)**:
   - Consultar `GET /api/v1/tenants/{tenantSlug}/menu?table_token=mesa-1` dos veces consecutivas y luego enviar `If-None-Match` con el `ETag` recibido para comprobar la respuesta `HTTP 304 Not Modified` inmediata desde L1.
3. **`ADR-0014` (`OPT-04`, `OPT-08` - JSONB `@>` camelCase e Índices GIN/B-Tree)**:
   - Ejecutar las pruebas de `OrderRepository` y `DeleteMenuItemCommandHandler` verificando que `OrderItemSnapshot` serializa `"menuItemId"` en camelCase y bloquea el archivado de platos con comandas activas.
4. **`ADR-0018` (`OPT-05`, `OPT-06` - Streams Reactivos, Carga por Lote y Ciclo de Vida de Sockets)**:
   - Ejecutar las pruebas de `OrderProcessorWorker` y `KitchenWebSocketManager` verificando que un pedido con múltiples platos ejecuta 1 sola llamada a `GetMenuItemsWithModifiersByIdsAsync` y que los sockets cerrados se eliminan del diccionario y liberan con `Dispose()`.
5. **`ADR-0017` (`OPT-07`, `OPT-09`, `OPT-10` - Gobernanza OTel, RFC 7807, SeaweedFS SSL y OPA RBAC)**:
   - Verificar que `SeaweedStorageService` con `"Storage:UseSsl": "true"` emite URLs `https://` de forma sincrónica y que `OpaClient` envía el arreglo `permissions` en `OpaUser`.
