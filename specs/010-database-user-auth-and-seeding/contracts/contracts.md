# API & CLI Contracts: PostgreSQL User Authentication & On-Demand Database Seeding

**Feature**: `010-database-user-auth-and-seeding`  
**Date**: 2026-10-26  
**Status**: Completed

---

## 1. REST Endpoint: `POST /api/v1/auth/login`

- **Authentication**: Public (`AllowAnonymous`)
- **Behavior**: Queries `IUserRepository.GetUserByEmailWithTenantAsync(email)` and verifies `password` using `IPasswordHasher.VerifyPassword(password, user.PasswordHash)`. Rejects any invalid email, inactive user, or mismatched password with `HTTP 401 Unauthorized`.

### Request Body
```json
{
  "email": "admin@bistrogourmet.com",
  "password": "admin123"
}
```

### Response `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "tenantSlug": "el-gato-negro",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "admin@bistrogourmet.com",
    "name": "Chef Marco",
    "role": "owner"
  }
}
```

### Response `401 Unauthorized`
```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Credenciales inválidas"
}
```

---

## 2. CLI & Script Contract for On-Demand Seeding

### PowerShell Script
```powershell
./scripts/seed-db.ps1
```

### Direct .NET CLI Command
```powershell
dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj -- --seed
```
- **Exit Code `0`**: Schema verified and seed data applied idempotently.
- **Exit Code `1`**: Database connection error or seed failure.
