# Technical Research: Canvas Menu Layout & AOT Perimeter Compilation

**Feature**: `005-canvas-layout-aot-compilation`
**Status**: Completed
**Date**: 2026-09-17

---

## 1. Storage of Visual Canvas Geometry (`layout_config`)

### Decision
Store `LayoutConfig` as an owned semi-structured JSONB entity on `Tenant` in PostgreSQL using EF Core's `OwnsOne(t => t.LayoutConfig, b => { b.ToJson(); })`.

### Rationale
- **Decoupling (ADR-0005):** Visual coordinates (`x`, `y`), dimensions (`width`, `height`), layer depths (`z_index`), and styling (`background_url`, `background_color`) are presentation-only metadata. They do not alter transactional catalog rules, prices, or modifiers.
- **PostgreSQL JSONB Discipline (Constitution Principle V):** EF Core 9/10 native `ToJson()` maps nested object graphs directly to PostgreSQL `jsonb` columns, enabling serialization/deserialization without manual JSON converters and allowing GIN indexing if needed.
- **Additive & Non-destructive:** If canvas is disabled or unset, `LayoutConfig` is null or has `canvas_enabled = false`, and EF Core handles nullability cleanly without altering `categories` or `menu_items` tables.

### Alternatives Considered
- *Option A: Columns on `MenuItem` (`pos_x`, `pos_y`, etc.):* Rejected because it pollutes catalog tables with display geometry, complicates multi-device layouts, and couples business products to canvas design.
- *Option B: Separate `canvas_layouts` table:* Overkill for single-tenant active layouts. An owned JSON document on `Tenant` aligns with `BrandingConfig` and requires zero join overhead during public menu retrieval.

---

## 2. Ahead-Of-Time (AOT) Pre-Flattening & Version Hashing

### Decision
Implement `IMenuCompilationService` that pre-flattens canvas geometry, prunes orphaned elements (elements referencing non-existent or inactive `dish_id`), calculates a deterministic SHA-256 `version_hash` combining the tenant catalog snapshot and layout config, and updates the tenant menu ETag.

### Rationale
- **Latency Budget (< 2s LCP) & CPU Savings (ADR-0006):** Mobile browsers should not execute layout calculations or resolve orphan dishes. Pre-flattening in backend shifts work away from low-end mobile devices.
- **Deterministic ETag:** The `version_hash` format `v{timestamp}-{sha256:8}` ensures deterministic conditional caching. Repeat requests with `If-None-Match` can be answered with HTTP 304 Not Modified in < 20ms without regenerating JSON responses.
- **Asynchronous Execution:** Invoked via `POST /api/v1/admin/menu/publish`, the endpoint returns HTTP 202 Accepted with `job_id`, `version_hash`, and status `processing`/`completed` within the < 3-second SLA budget.

### Alternatives Considered
- *Client-Side Dynamic Calculation (JIT):* Rejected per ADR-0006 due to main-thread mobile CPU bottlenecks and high battery consumption.
- *Server-Side Rendering (SSR) per Request:* Rejected because it increases TTFB and overloads the backend during concurrent peak lunch/dinner QR scanning surges.

---

## 3. CDN Cache Invalidation & Resilient Purging

### Decision
Introduce `ICdnPurgeService` interface with a pluggable design:
1. `LocalDevelopmentCdnPurgeService`: Logs invalidation tags and simulates zero-latency purging for dev/test environments.
2. `CloudflareCdnPurgeService` (or HTTP adapter): Dispatches cache tag / URL purges to perimeter CDNs in staging/production.
3. If the external CDN API times out or fails, OpenTelemetry records a structured span with `error.code="CDN_PURGE_FAILURE"`, while keeping the database version intact and allowing background retries without failing the user response.

### Rationale
- **Resilience (Constitution Principle IV, EARS-AOT-005):** External CDN outages or rate limits must never compromise backend data consistency or block the admin CMS.
- **Multi-Tenant Tag Purging:** Purging by cache tag (`tenant:{tenant_slug}`) ensures surgical invalidation of that restaurant's assets without flushing other tenants.

### Alternatives Considered
- *Direct synchronous HTTP call to CDN without fallback:* Rejected because CDN API network latency or downtime would cause HTTP 500 errors on admin publish actions.
- *Time-based TTL expiration only (no purge):* Rejected because restaurant price/layout edits must take effect immediately.

---

## 4. Declarative Authorization & OPA Rego Policies

### Decision
Enforce authorization for `/api/v1/admin/menu/layout` and `/api/v1/admin/menu/publish` through the existing `IOpaService` policy evaluation:
- Action: `menu:update_layout` and `menu:publish`.
- Roles: `owner`, `tenant_admin` (denying `cook`, `waiter`, and anonymous).
- Resource: `tenant_id` match.

### Rationale
- **Constitution Principle II (Declarative Auth):** Zero hardcoded role checks in Minimal API endpoints. OPA Rego handles deterministic role verification and tenant boundary isolation.

### Alternatives Considered
- *Hardcoded role checks in endpoint:* Violates Constitution Principle II and ADR-0006.

---

## 5. Public Menu Endpoint Integration & Cache Invalidation

### Decision
Extend `PublicMenuResponse` with `LayoutConfigDto? LayoutConfig`. When querying `GET /api/v1/tenants/{tenant_slug}/menu`, the query handler loads `Tenant.LayoutConfig` alongside `Categories` and `MenuItems`.
If `canvas_enabled` is true, it filters out elements referencing deleted or inactive dishes.
The response emits:
- `ETag`: `"{version_hash}"`
- `Cache-Control`: `"public, max-age=3600, s-maxage=86400"`
If incoming `If-None-Match` equals the current ETag, the endpoint immediately returns HTTP 304 Not Modified.
Cache invalidation in Redis/memory is updated on layout mutations and publication.

### Rationale
- Conforms directly to OpenAPI 3.1 contract in `RestoCore-Docs/specs/openapi.yaml`.
- Guarantees backward compatibility: clients that do not parse `layout_config` still receive full `categories` and `branding`.