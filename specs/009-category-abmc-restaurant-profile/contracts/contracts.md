# API Contracts: Category ABMC & Restaurant Contact Profile Management

**Feature**: `009-category-abmc-restaurant-profile`  
**Date**: 2026-10-15  
**Status**: Completed  
**Alignment**: Fully compliant with `RestoCore-Docs/specs/openapi.yaml`

---

## 1. Category ABMC Endpoints (`Admin - Catalog`)

All endpoints in this section are protected with JWT Bearer authentication and the `TenantAdminOnly` policy (`manage_menu`). Multi-tenancy is enforced via `ITenantContext`.

### 1.1 List Categories
- **Route**: `GET /api/v1/admin/categories`
- **Description**: Returns all categories for the authenticated tenant ordered by `sort_order` ascending.
- **Responses**:
  - `200 OK`: Array of `CategoryDetailDto`
  - `401 Unauthorized`
  - `403 Forbidden`

#### Response Body (`200 OK`):
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Entradas",
    "description": "Aperitivos y tapas de autor",
    "sort_order": 1,
    "is_visible": true
  },
  {
    "id": "8bb85f64-5717-4562-b3fc-2c963f66afb2",
    "name": "Platos Principales",
    "description": "Carnes a la parrilla y pastas caseras",
    "sort_order": 2,
    "is_visible": true
  }
]
```

---

### 1.2 Get Category by ID
- **Route**: `GET /api/v1/admin/categories/{id}`
- **Parameters**: `id` (path, UUID, required)
- **Responses**:
  - `200 OK`: `CategoryDetailDto`
  - `401 Unauthorized`
  - `403 Forbidden`
  - `404 Not Found`

#### Response Body (`200 OK`):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Entradas",
  "description": "Aperitivos y tapas de autor",
  "sort_order": 1,
  "is_visible": true
}
```

---

### 1.3 Create Category
- **Route**: `POST /api/v1/admin/categories`
- **Request Headers**: `Content-Type: application/json`
- **Request Body**:
```json
{
  "name": "Postres",
  "description": "Dulces caseros y helados artesanales",
  "sort_order": 3,
  "is_visible": true
}
```
- **Responses**:
  - `201 Created`: Location header `/api/v1/admin/categories/{id}`, body `CategoryDetailDto`
  - `400 Bad Request`: Validation errors
  - `401 Unauthorized`
  - `403 Forbidden`

---

### 1.4 Full Update Category (PUT)
- **Route**: `PUT /api/v1/admin/categories/{id}`
- **Parameters**: `id` (path, UUID, required)
- **Request Body**:
```json
{
  "name": "Postres & Cafetería",
  "description": "Dulces caseros, tortas y café de especialidad",
  "sort_order": 3,
  "is_visible": true
}
```
- **Responses**:
  - `200 OK`: `CategoryDetailDto`
  - `400 Bad Request`
  - `401 Unauthorized`
  - `403 Forbidden`
  - `404 Not Found`

---

### 1.5 Partial Update Category (PATCH)
- **Route**: `PATCH /api/v1/admin/categories/{id}`
- **Parameters**: `id` (path, UUID, required)
- **Request Body**:
```json
{
  "sort_order": 5,
  "is_visible": false
}
```
- **Responses**:
  - `200 OK`: `CategoryDetailDto`
  - `400 Bad Request`
  - `401 Unauthorized`
  - `403 Forbidden`
  - `404 Not Found`

---

### 1.6 Delete Category (DELETE)
- **Route**: `DELETE /api/v1/admin/categories/{id}`
- **Parameters**: `id` (path, UUID, required)
- **Behavior**:
  - Validates if the category contains any dishes (`menu_items`), including soft-deleted ones.
  - If dishes exist: returns `HTTP 409 Conflict`.
  - If no dishes exist: physically removes category and returns `HTTP 204 No Content`.
- **Responses**:
  - `204 No Content`: Category deleted successfully
  - `401 Unauthorized`
  - `403 Forbidden`
  - `404 Not Found`
  - `409 Conflict`: Category has associated dishes
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7807",
  "title": "Conflict",
  "status": 409,
  "detail": "No es posible eliminar la categoría porque contiene platos gastronómicos asociados (activos o archivados). Reasigne o elimine los platos antes de proceder."
}
```

---

## 2. Restaurant Profile Endpoints (`Admin - Restaurant Profile`)

Protected with JWT Bearer authentication and policy `TenantAdminOnly`.

### 2.1 Get Restaurant Profile
- **Route**: `GET /api/v1/admin/tenant/profile`
- **Responses**:
  - `200 OK`: `TenantProfileDto`
  - `401 Unauthorized`
  - `403 Forbidden`

#### Response Body (`200 OK`):
```json
{
  "id": "1fa85f64-5717-4562-b3fc-2c963f66afa1",
  "name": "Parrilla Don Pepe",
  "slug": "don-pepe",
  "phone_number": "+54 11 4567-8901",
  "address": "Av. Corrientes 1234, CABA",
  "business_hours": "Lunes a Domingo de 12:00 a 00:00 hs"
}
```

---

### 2.2 Full Update Restaurant Profile (PUT)
- **Route**: `PUT /api/v1/admin/tenant/profile`
- **Request Body**:
```json
{
  "name": "Parrilla Don Pepe & Asador",
  "phone_number": "+54 11 4567-8901",
  "address": "Av. Corrientes 1234, CABA",
  "business_hours": "Martes a Domingo de 12:00 a 01:00 hs"
}
```
- **Responses**:
  - `200 OK`: `TenantProfileDto`
  - `400 Bad Request`
  - `401 Unauthorized`
  - `403 Forbidden`

---

### 2.3 Partial Update Restaurant Profile (PATCH)
- **Route**: `PATCH /api/v1/admin/tenant/profile`
- **Request Body**:
```json
{
  "phone_number": "+54 11 9999-8888"
}
```
- **Responses**:
  - `200 OK`: `TenantProfileDto`
  - `400 Bad Request`
  - `401 Unauthorized`
  - `403 Forbidden`

---

## 3. Public Menu Contract Extension (`Public Menu`)

- **Route**: `GET /api/v1/tenants/{tenant_slug}/menu`
- **Response Headers**: `ETag: "..."`, `Cache-Control: public, max-age=3600, s-maxage=86400`
- **Extended Fields in Response Body (`200 OK`)**:
```json
{
  "tenant_name": "Parrilla Don Pepe",
  "tenant_slug": "don-pepe",
  "phone_number": "+54 11 4567-8901",
  "address": "Av. Corrientes 1234, CABA",
  "business_hours": "Lunes a Domingo de 12:00 a 00:00 hs",
  "table_number": 5,
  "branding": {
    "primary_color": "#E63946",
    "secondary_color": "#1D3557",
    "background_color": "#F8F9FA",
    "text_color": "#2B2D42",
    "logo_url": "https://storage.restocore.local/don-pepe/logo.png",
    "cover_banner_url": "https://storage.restocore.local/don-pepe/banner.jpg",
    "font_family": "Inter, sans-serif",
    "layout_mode": "GridWithImages"
  },
  "layout_config": null,
  "categories": []
}
```
