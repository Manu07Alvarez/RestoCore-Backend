# Quickstart & Verification Guide: Category ABMC & Restaurant Profile

**Feature**: `009-category-abmc-restaurant-profile`  
**Date**: 2026-10-15  
**Status**: Ready for Verification

---

## 1. Prerequisites

Ensure PostgreSQL, Redis, and OPA containers are running in Docker or local environment:
```powershell
docker compose up -d
```

---

## 2. Running Unit Tests

Run the full unit test suite including new Category and Profile tests:
```powershell
dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj
```

Targeted test execution for new handlers:
```powershell
dotnet test --filter "FullyQualifiedName~Categories|FullyQualifiedName~Profile"
```

---

## 3. Manual / API Verification with cURL

Set environment variables:
```powershell
$TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." # Valid Owner / TenantAdmin JWT
$BASE_URL = "http://localhost:5000"
$TENANT_SLUG = "parrilla-don-pepe"
```

### 3.1 Category ABMC Verification

1. **Create Category**:
```bash
curl -X POST "$BASE_URL/api/v1/admin/categories" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Bebidas y Tragos", "description": "Vinos y cócteles clásicos", "sort_order": 1, "is_visible": true}'
```

2. **List Categories**:
```bash
curl -X GET "$BASE_URL/api/v1/admin/categories" \
  -H "Authorization: Bearer $TOKEN"
```

3. **Modify Category (PUT)**:
```bash
curl -X PUT "$BASE_URL/api/v1/admin/categories/{CATEGORY_ID}" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Bebidas & Cafetería", "description": "Vinos, cócteles y café", "sort_order": 1, "is_visible": true}'
```

4. **Partial Modification (PATCH)**:
```bash
curl -X PATCH "$BASE_URL/api/v1/admin/categories/{CATEGORY_ID}" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"sort_order": 2}'
```

5. **Guardrail Conflict Verification (DELETE on category with dishes)**:
```bash
# Attempt to delete a category that has dishes assigned:
curl -i -X DELETE "$BASE_URL/api/v1/admin/categories/{CATEGORY_ID_WITH_ITEMS}" \
  -H "Authorization: Bearer $TOKEN"

# Expected: HTTP 409 Conflict with ProblemDetails
```

6. **Delete Category without dishes**:
```bash
curl -i -X DELETE "$BASE_URL/api/v1/admin/categories/{EMPTY_CATEGORY_ID}" \
  -H "Authorization: Bearer $TOKEN"

# Expected: HTTP 204 No Content
```

---

### 3.2 Restaurant Profile Management Verification

1. **Get Profile**:
```bash
curl -X GET "$BASE_URL/api/v1/admin/tenant/profile" \
  -H "Authorization: Bearer $TOKEN"
```

2. **Update Profile (PUT)**:
```bash
curl -X PUT "$BASE_URL/api/v1/admin/tenant/profile" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Parrilla Don Pepe Gourmet",
    "phone_number": "+54 11 4567-8901",
    "address": "Av. Corrientes 1234, CABA",
    "business_hours": "Lunes a Domingo de 12:00 a 00:00 hs"
  }'
```

3. **Partial Update Profile (PATCH)**:
```bash
curl -X PATCH "$BASE_URL/api/v1/admin/tenant/profile" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"phone_number": "+54 11 9999-8888"}'
```

4. **Verify Public Menu Contact Details**:
```bash
curl -i -X GET "$BASE_URL/api/v1/tenants/$TENANT_SLUG/menu"

# Verify response body includes "phone_number", "address", and "business_hours"
```
