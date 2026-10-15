# Data Model: Category ABMC & Restaurant Contact Profile Management

**Feature**: `009-category-abmc-restaurant-profile`  
**Date**: 2026-10-15  
**Status**: Completed  
**Sources**: `RestoCore-Docs/specs/openapi.yaml`, `RestoCore.Domain/Entities/`

---

## 1. Domain Entities & Database Schema Changes

### 1.1 Category Entity (`categories` table)
Existing entity and table. No schema migration required; existing fields cover all operations:

```csharp
namespace RestoCore.Domain.Entities;

public class Category : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;       // VARCHAR(80), NOT NULL
    public string? Description { get; set; }              // VARCHAR(250), NULL
    public int DisplayOrder { get; set; }                 // INT, DEFAULT 0, NOT NULL (mapped as sort_order)
    public bool IsActive { get; set; } = true;            // BOOLEAN, DEFAULT true, NOT NULL (mapped as is_visible)

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
```

#### Relational Constraints:
- `TenantId` Foreign Key references `tenants(id)` ON DELETE CASCADE.
- Index: `IX_categories_tenant_id_display_order` ON `(tenant_id, display_order)`.
- Foreign Key from `menu_items(category_id)` ON DELETE RESTRICT prevents accidental cascading deletions at database engine level.

---

### 1.2 Tenant Entity Extension (`tenants` table)
Add contact profile columns to `Tenant.cs` and `TenantConfiguration.cs`:

```csharp
namespace RestoCore.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public string Status { get; set; } = "Active";

    // New profile contact columns
    public string? PhoneNumber { get; set; }              // VARCHAR(50), NULL
    public string? Address { get; set; }                  // VARCHAR(255), NULL
    public string? BusinessHours { get; set; }            // VARCHAR(255), NULL

    public BrandingConfig BrandingConfig { get; set; } = new();
    public LayoutConfig? LayoutConfig { get; set; }
    public string? CurrentVersionHash { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public ICollection<Table> Tables { get; set; } = new List<Table>();
    public ICollection<MenuPublishJob> MenuPublishJobs { get; set; } = new List<MenuPublishJob>();
}
```

#### EF Core Configuration (`TenantConfiguration.cs`):
```csharp
builder.Property(t => t.PhoneNumber).HasMaxLength(50);
builder.Property(t => t.Address).HasMaxLength(255);
builder.Property(t => t.BusinessHours).HasMaxLength(255);
```

---

## 2. CQRS Commands, Queries & DTOs

### 2.1 Category ABMC Operations

#### Queries:
1. **`GetCategoriesQuery()`** -> Returns `List<CategoryResponse>`
   - Scoped to `TenantId`. Ordered by `DisplayOrder ASC, Name ASC`.
2. **`GetCategoryByIdQuery(Guid Id)`** -> Returns `CategoryResponse`
   - Scoped to `TenantId`. If not found, throws `KeyNotFoundException` (404).

#### Commands:
1. **`CreateCategoryCommand`**:
   ```csharp
   public record CreateCategoryCommand(
       string Name,
       string? Description,
       [property: JsonPropertyName("sort_order")] int DisplayOrder = 0,
       [property: JsonPropertyName("is_visible")] bool IsActive = true
   ) : IRequest<CategoryResponse>;
   ```
2. **`UpdateCategoryCommand`** (Full update - PUT):
   ```csharp
   public record UpdateCategoryCommand(
       Guid Id,
       string Name,
       string? Description,
       [property: JsonPropertyName("sort_order")] int DisplayOrder,
       [property: JsonPropertyName("is_visible")] bool IsActive
   ) : IRequest<CategoryResponse>;
   ```
3. **`PatchCategoryCommand`** (Partial update - PATCH):
   ```csharp
   public record PatchCategoryCommand(
       Guid Id,
       string? Name,
       string? Description,
       [property: JsonPropertyName("sort_order")] int? DisplayOrder,
       [property: JsonPropertyName("is_visible")] bool? IsActive
   ) : IRequest<CategoryResponse>;
   ```
4. **`DeleteCategoryCommand`**:
   ```csharp
   public record DeleteCategoryCommand(Guid Id) : IRequest<Unit>;
   ```

#### Category Response DTO:
```csharp
public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    [property: JsonPropertyName("sort_order")] int DisplayOrder,
    [property: JsonPropertyName("is_visible")] bool IsActive
);
```

---

### 2.2 Restaurant Profile Operations

#### Queries:
1. **`GetTenantProfileQuery()`** -> Returns `TenantProfileResponse`:
   ```csharp
   public record TenantProfileResponse(
       Guid Id,
       string Name,
       string Slug,
       [property: JsonPropertyName("phone_number")] string? PhoneNumber,
       string? Address,
       [property: JsonPropertyName("business_hours")] string? BusinessHours
   );
   ```

#### Commands:
1. **`UpdateTenantProfileCommand`** (Full update - PUT):
   ```csharp
   public record UpdateTenantProfileCommand(
       string Name,
       [property: JsonPropertyName("phone_number")] string? PhoneNumber,
       string? Address,
       [property: JsonPropertyName("business_hours")] string? BusinessHours
   ) : IRequest<TenantProfileResponse>;
   ```
2. **`PatchTenantProfileCommand`** (Partial update - PATCH):
   ```csharp
   public record PatchTenantProfileCommand(
       string? Name,
       [property: JsonPropertyName("phone_number")] string? PhoneNumber,
       string? Address,
       [property: JsonPropertyName("business_hours")] string? BusinessHours
   ) : IRequest<TenantProfileResponse>;
   ```

---

### 2.3 Public Menu Response Model Extension

In `PublicMenuResponse.cs`:
```csharp
public class PublicMenuResponse
{
    public string TenantName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? BusinessHours { get; set; }
    public int? TableNumber { get; set; }
    public PublicBrandingDto Branding { get; set; } = new();
    public LayoutConfigDto? LayoutConfig { get; set; }
    public List<PublicCategoryDto> Categories { get; set; } = new();
}
```

---

## 3. Referential Integrity & Validation Rules

### 3.1 Referential Integrity Guardrail on Category Deletion
```text
[DELETE /api/v1/admin/categories/{id}]
                 │
                 ▼
   Check Category exists in Tenant?
   ├─ NO  ──> Return HTTP 404 Not Found
   └─ YES
         │
         ▼
   Has associated MenuItems (active or soft-deleted)?
   ├─ YES ──> Throw InvalidOperationException / Conflict
   │          Return HTTP 409 Conflict
   │          (Detail: "La categoría tiene platos asociados...")
   └─ NO
         │
         ▼
   Physical deletion from DbContext
   Update Tenant.CurrentVersionHash
   Trigger ICdnPurgeService.PurgeTenantCacheAsync
   Return HTTP 204 No Content
```

### 3.2 Category Field Validation Rules
- `Name`: Required, non-empty, max 80 characters.
- `Description`: Optional, max 250 characters.
- `DisplayOrder` (`sort_order`): Integer >= 0.
- `IsActive` (`is_visible`): Boolean.

### 3.3 Tenant Profile Field Validation Rules
- `Name`: Required, non-empty, max 100 characters.
- `PhoneNumber`: Optional, max 50 characters.
- `Address`: Optional, max 255 characters.
- `BusinessHours`: Optional, max 255 characters.
