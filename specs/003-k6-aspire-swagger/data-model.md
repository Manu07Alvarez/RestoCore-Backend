# Data Model & Configuration Schemas: Stress Testing, Aspire Dashboard, and Swagger UI

**Feature**: `003-k6-aspire-swagger`
**Date**: 2026-09-08

---

## 1. Docker Compose Service Model: `aspire-dashboard`

```yaml
services:
  aspire-dashboard:
    image: mcr.microsoft.com/dotnet/aspire-dashboard:latest
    container_name: restocore-aspire-dashboard-dev
    environment:
      - DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true
      - DASHBOARD__OTLP__AUTHMODE=Unsecured
    ports:
      - "18888:18888" # Dashboard Web UI
      - "4317:4317"   # OTLP gRPC endpoint
      - "4318:4318"   # OTLP HTTP endpoint
    restart: unless-stopped
```

---

## 2. Configuration Settings (`appsettings.Development.json`)

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317",
    "ServiceName": "RestoCore.Backend"
  },
  "Swagger": {
    "Enabled": true,
    "Title": "RestoCore Backend API",
    "Version": "v1"
  }
}
```

---

## 3. k6 Test Profile & Threshold Contract

```javascript
export const options = {
  stages: [
    { duration: '10s', target: 20 }, // Ramp up to 20 virtual users
    { duration: '30s', target: 50 }, // Sustained load at 50 VUs
    { duration: '10s', target: 100 }, // Stress peak at 100 VUs
    { duration: '10s', target: 0 },   // Cool down
  ],
  thresholds: {
    'http_req_duration{status:304}': ['p(95)<150'], // ETag 304 response budget < 150ms
    'http_req_duration{status:200}': ['p(95)<500'], // Dynamic query budget < 500ms
    'http_req_failed': ['rate<0.01'],               // Error rate < 1%
  },
};
```

---

## 4. OpenTelemetry Enriched Trace Schema

Every span emitted during HTTP processing must carry the following attributes:

| Attribute Key | Type | Description | Example |
| :--- | :--- | :--- | :--- |
| `service.name` | string | Originating microservice | `RestoCore.Backend` |
| `tenant.id` | string (UUID) | Active tenant identifier resolved from subdomain/header | `d3b07384-d113-4611-9c60-8454f7623dfa` |
| `tenant.slug` | string | Tenant unique slug | `bistro-central` |
| `http.route` | string | ASP.NET Core matched endpoint pattern | `/api/v1/public/menus/{tenantSlug}` |
| `http.response.status_code` | int | HTTP status code | `200` or `304` |
| `db.system` | string | Relational storage engine | `postgresql` |
