# Quickstart: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard

**Feature**: `011-menu-analytics-and-telemetry`  
**Date**: 2026-11-04

---

## 1. Prerequisites

Ensure the development Docker stack is running and seeded:

```powershell
docker compose -f docker-compose.dev.yml up -d
./scripts/seed-db.ps1
dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj
```

---

## 2. Ingest Diner Telemetry Events (`POST /api/v1/analytics/collect`)

### 2.1 Record a QR Menu Open (`menu_view`)
```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:5221/api/v1/analytics/collect" `
  -ContentType "application/json" `
  -Body '{"event_type":"menu_view","tenant_slug":"bistro-gourmet-palermo","device_type":"mobile"}'
```
Expected Status: `202 Accepted` in `< 50ms`.

### 2.2 Record a Dish Modal Open (`item_view`)
```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:5221/api/v1/analytics/collect" `
  -ContentType "application/json" `
  -Body '{"event_type":"item_view","tenant_slug":"bistro-gourmet-palermo","item_id":"22222222-2222-2222-2222-222222222201","device_type":"mobile"}'
```

---

## 3. Query Executive Analytics Dashboard (`GET /api/v1/admin/analytics/dashboard`)

### 3.1 Authenticate as Restaurant Owner
```powershell
$login = Invoke-RestMethod -Method Post -Uri "http://localhost:5221/api/v1/auth/login" `
  -ContentType "application/json" `
  -Body '{"email":"admin@bistrogourmet.com","password":"Password123!"}'
$token = $login.token
```

### 3.2 Retrieve Analytics & Menu Engineering Matrix
```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5221/api/v1/admin/analytics/dashboard?from=2026-10-01&to=2026-11-04" `
  -Headers @{ Authorization = "Bearer $token" } | ConvertTo-Json -Depth 6
```

---

## 4. Run Automated Tests

```powershell
dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj
dotnet test tests/RestoCore.IntegrationTests/RestoCore.IntegrationTests.csproj
```
