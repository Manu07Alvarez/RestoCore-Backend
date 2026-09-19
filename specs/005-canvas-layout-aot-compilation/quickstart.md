# Quickstart & Verification Guide: Canvas Menu Layout & AOT Perimeter Compilation

**Feature**: `005-canvas-layout-aot-compilation`
**Status**: Ready
**Date**: 2026-09-17

This guide describes how to verify the Canvas Menu Layout and AOT Perimeter Compilation feature locally.

---

## 1. Prerequisites

Ensure local development containers are running and healthy:
```powershell
docker ps
```
The following containers must be active:
- `restocore-postgres-dev`
- `restocore-redis-dev`
- `restocore-seaweedfs-dev`
- `restocore-opa-dev`
- `restocore-aspire-dashboard-dev`

---

## 2. Testing Endpoints Locally

### A. Update Canvas Layout Configuration
Save custom dish positions and background settings for the restaurant:
```powershell
$token = "YOUR_ADMIN_JWT_TOKEN"
$tenantId = "TENANT_GUID"
$dishId = "DISH_GUID"

$body = @{
    canvas_enabled = $true
    background_url = "http://localhost:8333/restocore-media/canvas-bg.webp"
    background_color = "#121212"
    elements = @(
        @{
            dish_id = $dishId
            x = 100.0
            y = 250.0
            z_index = 1
            width = 300.0
            height = 200.0
        }
    )
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Method Put -Uri "http://localhost:5000/api/v1/admin/menu/layout" `
  -Headers @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" } `
  -Body $body
```

### B. Query Public Menu with Canvas
Verify that the public menu includes the `layout_config`:
```powershell
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/v1/tenants/demo-restaurant/menu" -Method Get
$response.StatusCode # Should be 200
$etag = $response.Headers["ETag"]
Write-Host "ETag: $etag"
Write-Host "Content: $($response.Content)"
```

### C. Trigger Ahead-Of-Time (AOT) Menu Publication
Publish changes to trigger AOT pre-flattening, version hash calculation, and CDN cache purging:
```powershell
$publishBody = @{ force_recompile = $false } | ConvertTo-Json

$job = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/v1/admin/menu/publish" `
  -Headers @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" } `
  -Body $publishBody

Write-Host "Job ID: $($job.job_id)"
Write-Host "Status: $($job.status)"
Write-Host "Version Hash: $($job.version_hash)"
```

### D. Verify Conditional HTTP 304 Caching
Repeat the public menu query passing the `If-None-Match` header with the `version_hash`:
```powershell
$cachedResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/v1/tenants/demo-restaurant/menu" `
  -Headers @{ "If-None-Match" = $etag } `
  -Method Get

$cachedResponse.StatusCode # Should be 304 Not Modified
```

---

## 3. Automated Test Execution

Execute unit and integration tests covering canvas layout and AOT compilation:
```powershell
dotnet test --filter "FullyQualifiedName~Canvas|FullyQualifiedName~Publish"
```
Or run the full test suite:
```powershell
dotnet test
```