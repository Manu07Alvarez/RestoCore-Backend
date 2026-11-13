# Phase 1 Contracts: Optimizaciones Arquitectónicas del Backend (`012-backend-architecture-optimizations`)

**Date**: 2026-11-12 | **Branch**: `feature/012-backend-architecture-optimizations`

---

## 1. Contrato de Caché Híbrido para Carta QR (`GET /api/v1/tenants/{tenant_slug}/menu`)

- **Request**:
  - `GET /api/v1/tenants/{tenant_slug}/menu?table_token={optional_token}`
  - Header opcional: `If-None-Match: "{etag}"`
- **Response (`200 OK` / `304 Not Modified`)**:
  - Headers obligatorios:
    - `ETag: "{hash}"`
    - `Cache-Control: public, max-age=60, s-maxage=3600`
  - Garantía de filtrado: Ningún ítem con `IsDeleted == true` o `IsAvailable == false` ni categoría con `IsActive == false` se incluye en `categories[].items[]`.

---

## 2. Contrato de Respuesta de Error Estándar RFC 7807 (`application/problem+json`)

Todas las respuestas de error gestionadas por `GlobalExceptionHandler` cumplen el estándar RFC 7807 sin exponer trazas internas ni PII:

```json
{
  "type": "https://api.restocore.com/errors/validation_error",
  "title": "Error de validacion en la solicitud.",
  "status": 400,
  "errorCode": "VALIDATION_ERROR",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

---

## 3. Contrato de Evaluación OPA (`POST /v1/data/restocore/authz/allow`)

Payload enviado por `OpaClient` hacia Open Policy Agent:

```json
{
  "input": {
    "method": "PUT",
    "path": ["api", "v1", "admin", "branding"],
    "action": "manage_branding",
    "user": {
      "id": "e7b8a9d0-1234-4567-89ab-cdef01234567",
      "tenant_id": "11111111-1111-1111-1111-111111111111",
      "roles": ["owner"],
      "permissions": [
        "manage_menu",
        "manage_categories",
        "manage_branding",
        "kitchen_stock",
        "read_metrics",
        "generate_presigned_url"
      ]
    },
    "resource": {
      "type": "menu",
      "tenant_id": "11111111-1111-1111-1111-111111111111"
    }
  }
}
```

---

## 4. Interfaces Internas de Aplicación e Infraestructura

### 4.1 `IPipelineBehavior<in TRequest, TResponse>` (`RestoCore.Application.Common.Mediator`)
```csharp
public delegate Task<TResponse> RequestHandlerDelegate<in TRequest, TResponse>(
    TRequest request,
    CancellationToken cancellationToken);

public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : notnull
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken);
}
```

### 4.2 `ICatalogRepository.GetMenuItemsWithModifiersByIdsAsync` (`RestoCore.Application.Common.Interfaces.Repositories`)
```csharp
Task<IReadOnlyList<MenuItem>> GetMenuItemsWithModifiersByIdsAsync(
    IReadOnlyCollection<Guid> itemIds,
    CancellationToken cancellationToken = default);
```

### 4.3 `IOrderQueueService.GetActiveTenantStreamsAsync` (`RestoCore.Application.Common.Interfaces`)
```csharp
Task<IReadOnlyCollection<string>> GetActiveTenantStreamsAsync(
    CancellationToken cancellationToken = default);
```
