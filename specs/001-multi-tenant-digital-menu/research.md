# Technical Research: Multi-Tenant Digital Menu & Operations Platform (QR-Based)

**Feature**: `001-multi-tenant-digital-menu`
**Date**: 2026-09-03
**Status**: Completed

---

## 1. Multi-Tenant Data Partitioning Strategy

### Context & Problem
The system must guarantee strict data isolation across multiple restaurant tenants while maintaining optimal query performance, straightforward database migrations, and support for flexible menu item attributes (`JSONB`).

### Alternatives Evaluated
1. **Database-per-Tenant:** Completely separate database per tenant.
   - *Pros:* Physical isolation, simple backups per tenant.
   - *Cons:* Extremely high operational overhead for hundreds of small tenants, connection pool exhaustion, complex migration management.
2. **Schema-per-Tenant:** Single database with dedicated PostgreSQL schema per tenant.
   - *Pros:* Logical isolation, clean namespace separation.
   - *Cons:* Migration overhead scales linearly with tenant count, dynamic connection/schema switching in EF Core introduces latency and connection caching issues.
3. **Shared Database, Shared Schema with `TenantId` Discriminator & Global Query Filters (Selected):**
   - *Pros:* Efficient connection pooling, instantaneous onboarding of new tenants, simple database migrations, seamless integration with PostgreSQL `JSONB` and composite `(TenantId, ...)` indexing (`ADR-0003`).
   - *Cons:* Requires disciplined application-level enforcement and automated integration tests to prevent accidental cross-tenant queries.

### Decision
Adopt **Shared Database with `TenantId` Column Discriminator** backed by:
- Entity Framework Core 9 Global Query Filters applied automatically to all multi-tenant entities via `ITenantScopedEntity`.
- Composite database indexes on `(TenantId, IsActive)` and `(TenantId, Slug)`.
- Optional PostgreSQL Row-Level Security (RLS) policies set per transaction connection via `app.current_tenant_id` session variable as a defense-in-depth gate.

---

## 2. Tenant Context Resolution Pipeline

### Context & Problem
Incoming requests originate from multiple channels: QR code scans via path slug (`/api/v1/tenants/{tenant_slug}/menu`), custom tenant subdomains (`{tenant}.restocore.app`), or administrative portal headers.

### Decision
Implement a cascading `TenantResolutionMiddleware` in ASP.NET Core that resolves and establishes an `ITenantContext` in this priority order:
1. **Route Value Strategy:** Inspect route parameters for `{tenant_slug}` or `{tenant_id}` (used by public menu and QR endpoints).
2. **Host Header Strategy:** Inspect `HttpContext.Request.Host` for subdomain patterns (`{tenant}.domain.com`).
3. **JWT Claims Strategy:** For authenticated backoffice requests, validate that the token contains the `tenant_id` claim.
4. **Header Strategy:** Inspect `X-Tenant-ID` (allowed strictly for authorized `SuperAdmin` impersonation).

If a tenant is resolved, the middleware queries the cache/repository, populates `ITenantContext` in the scoped service container, and attaches the `tenant.id` to the current `Activity` (OpenTelemetry span) and Serilog `LogContext`. If resolution fails for a tenant-required route, an HTTP 404 response is immediately returned.

---

## 3. Vector and Raster QR Code Generation

### Context & Problem
Tenants need on-demand QR code generation for physical dining tables and restaurant entrances, supporting high-resolution vector formats (SVG) for professional printing and raster formats (PNG) with embedded logos.

### Decision
Use **QRCoder** in combination with **SkiaSharp**:
- `QRCoder.SvgQRCode` for lightweight, lossless vector QR codes that scale infinitely for print media without rasterization artifacts.
- `QRCoder.PngByteQRCode` or SkiaSharp for rendering 300+ DPI PNG images with centered tenant logo overlay.
- Dynamic caching of generated QR codes using Redis and CDN headers (`Cache-Control: public, max-age=86400, immutable`), ensuring QR generation is performed only once per table token unless branding changes.

---

## 4. OpenTelemetry Distributed Tracing & Metrics

### Context & Problem
Observability must track end-to-end performance across multi-tenant workloads, with immediate capability to filter traces, database execution times, and error rates per restaurant.

### Decision
Integrate the **OpenTelemetry .NET SDK** (`OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.EntityFrameworkCore`):
- Configure a custom `ActivitySource("RestoCore.Backend")` and register an `ActivityProcessor` / `IAsyncDisposable` filter that automatically enriches every span with:
  - `tenant.id`: Unique tenant identifier.
  - `tenant.slug`: Human-readable slug.
  - `user.id` and `user.role` (if authenticated).
- Export traces and metrics via OpenTelemetry Protocol (OTLP) gRPC to an OpenTelemetry Collector.
- Expose Prometheus-compatible metrics endpoint or OTLP metrics stream for p95/p99 request latency and error rate dashboards.

---

## 5. Declarative Authorization with Open Policy Agent (OPA)

### Context & Problem
Authorization rules must adhere to ADR-0006 (`specs/policies/authz.rego`), decoupling business logic from role checks (`SuperAdmin`, `TenantAdmin`, `Manager`, `KitchenStaff`, `Anonymous`).

### Decision
Implement an ASP.NET Core `IAuthorizationPolicyProvider` and `IAuthorizationHandler`:
- The handler constructs an input document:
  ```json
  {
    "user": { "id": "...", "tenant_id": "...", "roles": ["..."] },
    "action": "HTTP_METHOD",
    "resource": { "type": "...", "tenant_id": "..." }
  }
  ```
- Evaluates the policy against OPA via high-performance REST/gRPC or an in-process Rego WebAssembly / compiled evaluator.
- Enforces instant 403 Forbidden rejection if `user.tenant_id != resource.tenant_id` (except for `SuperAdmin`).
