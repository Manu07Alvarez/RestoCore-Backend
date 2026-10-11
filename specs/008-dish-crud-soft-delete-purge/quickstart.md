# Quickstart: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker

**Feature**: `008-dish-crud-soft-delete-purge`

---

## 1. Running Automated Tests

Run unit tests covering `UpdateMenuItemCommand`, `PatchMenuItemCommand`, `DeleteMenuItemCommand`, `RestoreMenuItemCommand`, and `PurgeArchivedDishesWorker`:

```powershell
dotnet test tests/RestoCore.UnitTests --filter "FullyQualifiedName~MenuItem"
```

---

## 2. Testing Endpoints via cURL or REST Client

### A. Modifying a Dish (PUT)
```bash
curl -X PUT "http://localhost:5000/api/v1/admin/items/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "category_id": "4a73a384-934c-47ea-bd56-c73468df7298",
    "name": "Hamburguesa Doble Smash Especial",
    "description": "Edición especial con alioli trufado",
    "base_price": 16000.00,
    "is_available": true,
    "display_order": 1,
    "allergens": ["gluten", "dairy"],
    "dietary_labels": ["contains_dairy"]
  }'
```

### B. Soft Deleting a Dish (DELETE)
```bash
curl -X DELETE "http://localhost:5000/api/v1/admin/items/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer <token>"
```

### C. Restoring an Archived Dish (POST)
```bash
curl -X POST "http://localhost:5000/api/v1/admin/items/3fa85f64-5717-4562-b3fc-2c963f66afa6/restore" \
  -H "Authorization: Bearer <token>"
```
