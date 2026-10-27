# Quickstart & Verification Guide: PostgreSQL User Authentication & On-Demand Database Seeding

**Feature**: `010-database-user-auth-and-seeding`  
**Date**: 2026-10-26

---

## 1. Run Unit Tests
```powershell
dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj
```

---

## 2. Seed Database On Demand
Whenever you need to initialize or replenish the development database with the default tenant (`el-gato-negro`), users, categories, dishes, and QR tables:
```powershell
./scripts/seed-db.ps1
```
Or directly via .NET CLI:
```powershell
dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj -- --seed
```

---

## 3. Authenticate via REST API
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@bistrogourmet.com", "password": "admin123"}'
```
