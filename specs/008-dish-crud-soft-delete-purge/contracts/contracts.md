# API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore

**Feature**: `008-dish-crud-soft-delete-purge`  
**Base Route**: `/api/v1/admin/items`  
**Authorization**: `Bearer <token>` (`TenantAdminOnly` / Owner)

---

## 1. Full Modification of Menu Item (`PUT /api/v1/admin/items/{id}`)

### Request
```http
PUT /api/v1/admin/items/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Content-Type: application/json
Authorization: Bearer <jwt>

{
  "category_id": "4a73a384-934c-47ea-bd56-c73468df7298",
  "name": "Hamburguesa Doble Smash",
  "description": "Doble medallón de 100g, queso cheddar, panceta crocante y salsa secreta",
  "base_price": 14500.00,
  "is_available": true,
  "image_url": "http://seaweedfs:8888/dishes/smash-burger.jpg",
  "display_order": 1,
  "allergens": ["gluten", "dairy"],
  "dietary_labels": ["contains_dairy"]
}
```

### Responses
- `200 OK`: Updated `MenuItemResponse`
- `400 Bad Request`: Validation failure (empty name, negative price)
- `422 Unprocessable Entity`: `category_id` does not exist or does not belong to the active tenant
- `404 Not Found`: Item not found or belongs to another tenant

---

## 2. Partial Modification of Menu Item (`PATCH /api/v1/admin/items/{id}`)

### Request
```http
PATCH /api/v1/admin/items/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Content-Type: application/json
Authorization: Bearer <jwt>

{
  "base_price": 15000.00,
  "dietary_labels": ["contains_dairy", "popular"]
}
```

### Responses
- `200 OK`: Updated `MenuItemResponse`
- `400 Bad Request`: Validation error
- `422 Unprocessable Entity`: Invalid `category_id` if provided
- `404 Not Found`: Item not found

---

## 3. Soft Delete Menu Item (`DELETE /api/v1/admin/items/{id}`)

### Request
```http
DELETE /api/v1/admin/items/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Authorization: Bearer <jwt>
```

### Responses
- `200 OK`:
```json
{
  "item_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "archived",
  "message": "El plato ha sido retirado de la carta. Los clientes ya no lo verán en el menú digital ni podrán pedirlo, pero todas tus ventas anteriores y reportes seguirán intactos y seguros. Dispones de 7 días para restaurarlo antes de su eliminación definitiva.",
  "has_open_orders": false,
  "archived_at": "2026-10-11T02:45:00Z",
  "retention_days": 7,
  "purge_scheduled_at": "2026-10-18T02:45:00Z"
}
```
- `409 Conflict`:
```json
{
  "type": "https://restocore.io/errors/active-orders-conflict",
  "title": "Cannot archive dish with active orders",
  "status": 409,
  "detail": "No es posible retirar este plato porque se encuentra actualmente en preparación en cocina o pedidos abiertos de salón. Espera a que los pedidos finalicen antes de archivarlo."
}
```
- `404 Not Found`: Item not found or already purged

---

## 4. Restore Menu Item (`POST /api/v1/admin/items/{id}/restore`)

### Request
```http
POST /api/v1/admin/items/3fa85f64-5717-4562-b3fc-2c963f66afa6/restore HTTP/1.1
Authorization: Bearer <jwt>
```

### Responses
- `200 OK`: Restored `MenuItemResponse` with `is_available` restored and excluded from soft-delete
- `400 Bad Request`: Item is not deleted
- `404 Not Found`: Item does not exist or 7-day retention period has expired
