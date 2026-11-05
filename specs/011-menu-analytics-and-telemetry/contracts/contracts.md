# Phase 1 API Contracts: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard

**Feature**: `011-menu-analytics-and-telemetry`  
**OpenAPI Version**: 3.1.0  
**Source of Truth**: `RestoCore-Docs/specs/openapi.yaml`

---

## 1. `POST /api/v1/analytics/collect`

### Overview
- **Tag**: `Analytics - Ingestion`
- **OperationId**: `collectAnalyticsEvent`
- **Authorization**: Anonymous / Public (`AllowAnonymous`)
- **Latency Target**: `< 50ms p95` (`HTTP 202 Accepted`)

### Request Body (`application/json` — `AnalyticsEventRequest`)
```json
{
  "event_type": "item_view",
  "tenant_slug": "bistro-gourmet-palermo",
  "item_id": "22222222-2222-2222-2222-222222222201",
  "category_id": null,
  "device_type": "mobile"
}
```

#### Validation Rules
- `event_type` (required): Must be one of `"menu_view"`, `"item_view"`, `"category_click"`.
- `tenant_slug` (required): Non-empty string matching an active tenant.
- `item_id` (optional, nullable UUID): Required when `event_type == "item_view"`.
- `category_id` (optional, nullable UUID): Required when `event_type == "category_click"`.
- `device_type` (optional, nullable): Must be one of `"mobile"`, `"desktop"`, `"tablet"` when provided (defaults to `"mobile"` if omitted).

### Responses
- **`202 Accepted`**: Event validated and enqueued for asynchronous aggregation.
- **`400 Bad Request`**: Invalid `event_type`, missing `tenant_slug`, missing target entity ID for `item_view`/`category_click`, or unknown `tenant_slug` (`application/problem+json`).

---

## 2. `GET /api/v1/admin/analytics/dashboard`

### Overview
- **Tag**: `Admin - Analytics`
- **OperationId**: `getAnalyticsDashboard`
- **Authorization**: Bearer JWT + OPA Policy (`OpaRequirement("read_metrics")` — allows `owner`, `tenant_admin`, `super_admin`; denies `cook`, `waiter`, anonymous).
- **Latency Target**: `< 300ms p95`

### Query Parameters
- `from` (optional, ISO-8601 `date`, e.g. `2026-10-01`): Start date of reporting interval (UTC). Defaults to ` today - 30 days`.
- `to` (optional, ISO-8601 `date`, e.g. `2026-10-31`): End date of reporting interval (UTC). Defaults to `today`.
- Validation: If `from > to`, responds with `400 Bad Request`.

### Response `200 OK` (`application/json` — `AnalyticsDashboardResponse`)

```json
{
  "tenant_id": "11111111-1111-1111-1111-111111111111",
  "period_from": "2026-10-01",
  "period_to": "2026-10-31",
  "priority_high_metrics": {
    "total_qr_visits": 4520,
    "monthly_qr_visits": 1240,
    "top_viewed_items": [
      {
        "item_id": "22222222-2222-2222-2222-222222222201",
        "name": "Ojo de Bife Angus",
        "views_count": 890
      }
    ],
    "top_categories": [
      {
        "category_id": "33333333-3333-3333-3333-333333333301",
        "name": "Carnes a las Brasas",
        "interactions_count": 2150
      }
    ],
    "menu_engineering_matrix": {
      "stars": ["22222222-2222-2222-2222-222222222201"],
      "plowhorses": ["22222222-2222-2222-2222-222222222202"],
      "puzzles": ["22222222-2222-2222-2222-222222222203"],
      "dogs": ["22222222-2222-2222-2222-222222222204"]
    }
  },
  "priority_medium_metrics": {
    "peak_hours": [
      { "hour": 20, "visits_count": 210 },
      { "hour": 21, "visits_count": 340 }
    ],
    "device_breakdown": {
      "mobile_percentage": 92.5,
      "desktop_percentage": 7.5
    }
  }
}
```

> **Note on Standard Tier Tenants (`FR-008`)**: When `Tenant.PlanTier == "Standard"`, `priority_medium_metrics` is `null`.  
> **Note on ADR-0012 Deferral (`FR-009`)**: Table occupancy, dwell time, and turnover fields are strictly omitted across all tiers.

### Error Responses
- **`400 Bad Request`**: Invalid date format or `from > to`.
- **`401 Unauthorized`**: Missing or invalid Bearer token.
- **`403 Forbidden`**: Authenticated user lacks `read_metrics` permission in OPA policy (`cook`, `waiter`) or mismatches tenant scope.
