# Phase 1 Data Model: Optimizaciones Arquitectónicas del Backend (`012-backend-architecture-optimizations`)

**Date**: 2026-11-12 | **Branch**: `feature/012-backend-architecture-optimizations`

---

## 1. Modelos de Persistencia y Serialización JSONB (`ADR-0014`)

### 1.1 `OrderItemSnapshot` y `OrderModifierSnapshot` (`RestoCore.Domain.Entities`)
Se estandarizan los nombres de propiedad JSON en camelCase para garantizar coincidencia binaria exacta con el operador `@>` de PostgreSQL:

| Entidad | Propiedad C# | Atributo JSON | Tipo | Regla de Validación |
|---|---|---|---|---|
| `OrderItemSnapshot` | `MenuItemId` | `[JsonPropertyName("menuItemId")]` | `Guid` | Obligatorio, indexado por GIN |
| `OrderItemSnapshot` | `Name` | `[JsonPropertyName("name")]` | `string` | No vacío |
| `OrderItemSnapshot` | `Quantity` | `[JsonPropertyName("quantity")]` | `int` | `> 0` |
| `OrderItemSnapshot` | `UnitPrice` | `[JsonPropertyName("unitPrice")]` | `decimal` | `>= 0` |
| `OrderItemSnapshot` | `Instructions` | `[JsonPropertyName("instructions")]` | `string?` | Opcional |
| `OrderItemSnapshot` | `Modifiers` | `[JsonPropertyName("modifiers")]` | `List<OrderModifierSnapshot>` | Colección inmutable en snapshot |
| `OrderModifierSnapshot` | `OptionId` | `[JsonPropertyName("optionId")]` | `Guid` | Obligatorio |
| `OrderModifierSnapshot` | `Name` | `[JsonPropertyName("name")]` | `string` | No vacío |
| `OrderModifierSnapshot` | `PriceDelta` | `[JsonPropertyName("priceDelta")]` | `decimal` | Puede ser `>= 0` |

### 1.2 Índices en PostgreSQL (`OrderConfiguration` y `UserConfiguration`)
- **`ix_orders_items_snapshot_gin`**: Índice `GIN` sobre `orders.items_snapshot` (`builder.HasIndex(o => o.ItemsSnapshot).HasMethod("gin")`).
- **`ix_orders_active_tenant`**: Índice `B-Tree` parcial sobre `orders (tenant_id)` con predicado `status NOT IN ('delivered', 'cancelled')`.
- **`IX_users_Email`**: Índice `B-Tree` único sobre `users (email)`, almacenando `User.Email` normalizado en minúsculas (`email.Trim().ToLowerInvariant()`).

---

## 2. Modelos de Caché Híbrido L1/L2 (`ADR-0015`)

### 2.1 `PublicMenuCacheEntry` (`RestoCore.Application.Common.Caching`)
Registro inmutable almacenado en memoria local (L1, TTL 60s) y en Redis distribuido (L2, TTL 2h):

| Propiedad | Tipo | Descripción |
|---|---|---|
| `ETag` | `string` | Hash determinista entre comillas (ej. `"a1b2c3d4-t5"`) para evaluación rápida de `If-None-Match` |
| `PayloadJson` | `string` | Payload JSON pre-serializado de `PublicMenuResponse` listo para transmisión directa |

- **Esquema de clave**: `menu:public:{tenantSlug}:{tableToken ?? "default"}`
- **Canal Redis Pub/Sub de invalidación**: `restocore:hybridcache:invalidate` (payload: `{tenantSlug}`)

---

## 3. Modelos de Despacho CQRS (`ADR-0016`)

### 3.1 `IPipelineBehavior<in TRequest, TResponse>` y `IRequestInvoker<TResponse>`
- **`RequestHandlerDelegate<in TRequest, TResponse>`**: `delegate Task<TResponse> RequestHandlerDelegate<in TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)`
- **`IPipelineBehavior<in TRequest, TResponse>`**: Contrato transversal implementado por `ValidationBehavior<TRequest, TResponse>`.
- **`IRequestInvoker<TResponse>` / `RequestInvoker<TRequest, TResponse>`**: Invocador fuertemente tipado almacenado en `ConcurrentDictionary<Type, object> InvokerCache`.

---

## 4. Modelos de Seguridad y Autorización OPA (`ADR-0017`)

### 4.1 `OpaUser` (`RestoCore.Infrastructure.Authorization`)
Sincronizado con `specs/policies/authz.rego`:

| Propiedad C# | Propiedad JSON | Tipo | Descripción |
|---|---|---|---|
| `Id` | `id` | `string` | Identificador del usuario autenticado |
| `TenantId` | `tenant_id` | `string` | UUID del tenant del usuario |
| `Roles` | `roles` | `string[]` | Roles asignados (`owner`, `admin`, `cook`, `waiter`, `superadmin`) |
| `Permissions` | `permissions` | `string[]` | Permisos granulares (`manage_menu`, `manage_categories`, `manage_branding`, `kitchen_stock`, `read_metrics`, `generate_presigned_url`, `manage_tenants`) |
