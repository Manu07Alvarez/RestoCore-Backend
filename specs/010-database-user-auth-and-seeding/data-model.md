# Data Model: PostgreSQL User Authentication & On-Demand Database Seeding

**Feature**: `010-database-user-auth-and-seeding`  
**Date**: 2026-10-26  
**Status**: Completed

---

## 1. Domain Entity: `User` (`users` table)

```csharp
namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;          // VARCHAR(150), UNIQUE, NOT NULL
    public string Name { get; set; } = string.Empty;           // VARCHAR(100), NOT NULL
    public string PasswordHash { get; set; } = string.Empty;   // VARCHAR(256), NOT NULL
    public string PrimaryRole { get; set; } = string.Empty;    // VARCHAR(50), NOT NULL
    public string[] Roles { get; set; } = Array.Empty<string>(); // TEXT[], NOT NULL
    public Guid? TenantId { get; set; }                        // UUID, NULL (FK to tenants.id ON DELETE CASCADE)
    public bool IsActive { get; set; } = true;                 // BOOLEAN, DEFAULT true, NOT NULL

    public Tenant? Tenant { get; set; }
}
```

### EF Core Configuration (`UserConfiguration.cs`)
- Table: `users`
- Primary Key: `Id`
- Unique Index: `IX_users_email` on `Email`
- Foreign Key: `HasOne(u => u.Tenant).WithMany().HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Cascade)`

---

## 2. CQRS Command & DTOs (`Features/Auth`)

### `LoginCommand`
```csharp
public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

public record UserProfileResponse(
    string Id,
    string Email,
    string Name,
    string Role
);

public record LoginResponse(
    string Token,
    string? TenantId,
    string? TenantSlug,
    UserProfileResponse User
);
```

---

## 3. Seed Dataset Specification (`DatabaseSeeder`)

When `--seed` or `./scripts/seed-db.ps1` is executed:

1. **Tenant**:
   - `Id`: `11111111-1111-1111-1111-111111111111`
   - `Name`: `"El Gato Negro"`
   - `Slug`: `"el-gato-negro"`
   - `PhoneNumber`: `"+54 11 4567-8901"`
   - `Address`: `"Av. Corrientes 1669, CABA"`
   - `BusinessHours`: `"Lunes a Domingo de 08:00 a 00:00 hs"`
   - `Status`: `"Active"`

2. **Users**:
   | Email | Password (Hashed) | Name | PrimaryRole | Roles | TenantId |
   | :--- | :--- | :--- | :--- | :--- | :--- |
   | `admin@bistrogourmet.com` | `admin123` | `Chef Marco` | `owner` | `["owner", "tenant_admin", "kitchen_staff"]` | `11111111-...` |
   | `admin@restocore.com` | `admin123` | `Administrador Local` | `owner` | `["owner", "tenant_admin", "kitchen_staff"]` | `11111111-...` |
   | `cocina@restocore.com` | `cocina123` | `Cocinero Pablo` | `cook` | `["cook", "kitchen_staff"]` | `11111111-...` |
   | `superadmin@restocore.com` | `admin123` | `Super Administrador` | `super_admin` | `["super_admin"]` | `null` |

3. **Categories & Menu Items**:
   - Category 1: `Entradas y Tapas` (`DisplayOrder = 1`)
     - `Empanadas Salteñas Cortadas a Cuchillo` (`$4500.00`, `DietaryLabels = ["traditional"]`)
     - `Provoleta Ahumada con Tomates Confitados` (`$8900.00`, `DietaryLabels = ["vegetarian", "gluten_free"]`)
   - Category 2: `Platos Principales` (`DisplayOrder = 2`)
     - `Ojo de Bife Madurado 400g` (`$22500.00`, `DietaryLabels = ["gluten_free", "high_protein"]`)
     - `Sorrentinos de Calabaza y Parmesano` (`$14800.00`, `DietaryLabels = ["vegetarian"]`)
   - Category 3: `Postres y Cafetería` (`DisplayOrder = 3`)
     - `Flan Casero Mixto con Dulce de Leche` (`$6200.00`, `DietaryLabels = ["vegetarian", "gluten_free"]`)

4. **Tables**:
   - Table 1 (`Token = "mesa-1-el-gato-negro"`, `TableNumber = 1`)
   - Table 2 (`Token = "mesa-2-el-gato-negro"`, `TableNumber = 2`)
   - Table 3 (`Token = "mesa-3-el-gato-negro"`, `TableNumber = 3`)
