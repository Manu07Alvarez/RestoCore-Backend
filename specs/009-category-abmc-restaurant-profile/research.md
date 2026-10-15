# Architectural Research: Category ABMC & Restaurant Contact Profile Management

**Feature**: `009-category-abmc-restaurant-profile`  
**Date**: 2026-10-15  
**Status**: Completed  
**Sources**: `RestoCore-Docs` (`docs/prd-admin-crud.md` @ commit `42f8e6c`, `docs/ears-menu.md`, `specs/openapi.yaml`)

---

## 1. Category Referential Integrity Guardrail on Deletion

### Context & Problem
In `RestoCore-Docs/docs/prd-admin-crud.md` and `specs/openapi.yaml`, when an administrator issues `DELETE /api/v1/admin/categories/{id}`, deleting a category that currently has associated menu items (whether active or soft-deleted) would leave those menu items orphaned. In relational terms, `menu_items.category_id` has a foreign key constraint configured with `OnDelete(DeleteBehavior.Restrict)`. If the application fails to validate this before attempting deletion, either an unhandled database exception (PostgreSQL 23503 foreign_key_violation) is thrown (yielding HTTP 500) or dishes become untracked.

### Decision
Implement a deterministic business validation in `DeleteCategoryCommandHandler` that queries `ICatalogRepository.HasMenuItemsForCategoryAsync(categoryId)`.
- The repository query must execute `_context.MenuItems.IgnoreQueryFilters().AnyAsync(m => m.CategoryId == categoryId, cancellationToken)`.
- If true, the handler returns/throws a conflict error that maps to `HTTP 409 Conflict` with ProblemDetails:
  - `Status`: 409
  - `Title`: "Conflict"
  - `Detail`: "No es posible eliminar la categoría porque contiene platos gastronómicos asociados (activos o archivados). Reasigne o elimine los platos antes de proceder."
- If false (zero associated items), the category is physically removed from the database, the public menu version hash is recomputed, perimeter CDN cache is purged via `ICdnPurgeService`, and the endpoint responds with `HTTP 204 No Content`.

### Alternatives Considered
1. *Cascade Delete Dishes on Category Deletion*: Rejected. This would cause catastrophic unintended data loss and violate PRD safety requirements (dishes can have order history and financial records).
2. *Set Dish Category to NULL*: Rejected. `category_id` is a required non-nullable foreign key representing the mandatory 1:N structural grouping in the digital menu.

---

## 2. Restaurant Profile Extension & Multi-Tenant Isolation

### Context & Problem
`RestoCore-Docs` requires administrative control over public restaurant contact details (`phone_number`, `address`, `business_hours`) and their publication to the QR digital menu (`GET /api/v1/tenants/{tenant_slug}/menu`). Currently, the `tenants` entity and table only store `Name`, `Slug`, `CustomDomain`, `Status`, `BrandingConfig`, `LayoutConfig`, and `CurrentVersionHash`.

### Decision
1. **Domain Model**: Extend `Tenant.cs` with three nullable string properties:
   - `PhoneNumber`: nullable string, max length 50. Supports international format (e.g. `+54 11 4567-8901`).
   - `Address`: nullable string, max length 255. Physical restaurant address.
   - `BusinessHours`: nullable string, max length 255. Public schedule text (e.g. `Lunes a Domingo de 12:00 a 00:00 hs`).
2. **EF Core Mapping**: Update `TenantConfiguration.cs` with column lengths and nullability rules.
3. **Multi-Tenancy & Authorization**:
   - `GET /api/v1/admin/tenant/profile`: Resolves tenant from `ITenantContext.TenantId` and requires `TenantAdminOnly` (OPA `manage_menu` or `manage_tenants` policy).
   - `PUT /api/v1/admin/tenant/profile`: Complete replacement of commercial details (`Name`, `PhoneNumber`, `Address`, `BusinessHours`).
   - `PATCH /api/v1/admin/tenant/profile`: Partial update. Only non-null supplied fields are mutated.
   - Any modification invalidates CDN cache via `ICdnPurgeService` and updates `CurrentVersionHash`.

### Alternatives Considered
1. *Store Contact Details inside `BrandingConfig` JSONB*: Rejected. Contact information (phone, address, opening hours) represents primary commercial identity and search metadata, not visual layout/theming. Storing them as first-class columns ensures standard SQL indexing, cleaner validation, and exact alignment with `TenantProfileDto` in OpenAPI.

---

## 3. Public Menu Exposure & Latency Budget Compliance

### Context & Problem
When diners access `GET /api/v1/tenants/{tenant_slug}/menu`, the response must include `phone_number`, `address`, and `business_hours` at root level, without degrading the strict server processing budget (< 500ms) or QR LCP (< 2s).

### Decision
1. Update `PublicMenuResponse.cs` to include:
   ```csharp
   public string? PhoneNumber { get; set; }
   public string? Address { get; set; }
   public string? BusinessHours { get; set; }
   ```
2. Update `GetPublicMenuQueryHandler.cs` to map `tenant.PhoneNumber`, `tenant.Address`, and `tenant.BusinessHours` into `PublicMenuResponse`.
3. Because `GetTenantWithCatalogAsync` already fetches the `Tenant` entity, no additional database query or join is introduced. Latency overhead is 0ms.
4. Changes to restaurant profile trigger `tenant.CurrentVersionHash` recalculation and `ICdnPurgeService.PurgeTenantCacheAsync`, ensuring edge CDN nodes refresh immediately while returning `304 Not Modified` when ETags match.

---

## 4. Category ABMC Partial Update (PATCH) Semantics

### Context & Problem
In `PATCH /api/v1/admin/categories/{id}`, administrators may change only `sort_order` (reordering in backoffice) or toggle `is_visible` without sending the complete category object.

### Decision
1. Define `PatchCategoryCommand(Guid Id, string? Name, string? Description, int? SortOrder, bool? IsVisible)`.
2. In `PatchCategoryCommandHandler`:
   - Fetch category by `Id` scoped to `TenantId`. If not found, return 404.
   - If `request.Name` is not null: validate not empty, trim, update.
   - If `request.Description` is provided: update description.
   - If `request.SortOrder.HasValue`: update `DisplayOrder`.
   - If `request.IsVisible.HasValue`: update `IsActive`.
   - Persist via `IUnitOfWork.CommitAsync`.
   - Invalidate cache via `ICdnPurgeService`.
   - Return updated `CategoryResponse`.

---

## 5. DTO Naming & Serialization Strategy

### Context & Problem
OpenAPI 3.1 defines property names `sort_order` and `is_visible` for category requests/responses, and `phone_number`, `business_hours` for tenant profile. In C#, domain entity and standard property names use `DisplayOrder`, `IsActive`, `PhoneNumber`, `BusinessHours`.

### Decision
DTO records will declare explicit System.Text.Json property name attributes:
- `[property: JsonPropertyName("sort_order")] int SortOrder` (with internal mapping to `DisplayOrder`)
- `[property: JsonPropertyName("is_visible")] bool IsVisible` (with internal mapping to `IsActive`)
- `[property: JsonPropertyName("phone_number")] string? PhoneNumber`
- `[property: JsonPropertyName("business_hours")] string? BusinessHours`
This ensures 100% adherence to OpenAPI schema contracts while maintaining idiomatic C# domain code.
