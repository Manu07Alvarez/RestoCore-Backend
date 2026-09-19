# Feature Specification: Canvas Menu Layout & AOT Perimeter Compilation

**Feature Branch**: `005-canvas-layout-aot-compilation`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "analiza el repositorio de RestoCore-Docs @[conversation:Reading Project Documentation] se necesita aplicar las ultimas especificaciones al proyecto - Capa Visual Canvas (Modo Lienzo), Desacoplamiento de Catálogo, Compilación Ahead-Of-Time (AOT) y Caché Perimetral CDN (ADR-0005, ADR-0006, EARS-MENU, OpenAPI 3.1)"

---

## User Scenarios & Testing

### User Story 1 - Canvas Layout Authoring & Architectural Decoupling (Priority: P1)

An authorized restaurant owner or administrator logs into the backoffice CMS visual editor to customize the digital menu's visual layout beyond the standard sequential list. The administrator arranges dishes freely on a two-dimensional visual plane by assigning coordinates (`x`, `y`), dimensions (`width`, `height`), and layer order (`z_index`). The administrator optionally configures a canvas background color (`background_color`) or selects an uploaded background image (`background_url` hosted on SeaweedFS), and toggles canvas mode on (`canvas_enabled: true`). The system persists this geometry as an additive, semi-structured configuration object (`layout_config`), completely decoupled from the transactional catalog entities (dishes, prices, modifiers, dietary tags).

**Why this priority**: Brand differentiation and custom menu presentation are core business drivers for restaurant clients. Decoupling the visual canvas layout from catalog entities protects data integrity, ensures rapid iteration without database schema migrations, and allows instant reversion to default layouts.

**Independent Test**: Can be tested independently by authenticating with `owner` or `tenant_admin` role, sending a layout update payload with valid canvas coordinates and elements, verifying that the layout configuration is persisted with tenant scoping, and confirming that dish prices, modifiers, and catalog data remain unmodified.

**Acceptance Scenarios**:

1. **Given** an authenticated tenant administrator and an existing catalog with dishes, **When** they submit a valid layout configuration with `canvas_enabled: true`, background settings, and an array of canvas elements referencing existing dish identifiers, **Then** the system persists the configuration, associates it with the tenant, and returns HTTP 200 OK with the updated `layout_config` within 200ms.
2. **Given** an active canvas configuration, **When** the administrator updates `canvas_enabled` to `false` or resets the layout, **Then** the system updates the flag without deleting underlying dish catalog entities, immediately reverting the public presentation to the standard sequential category layout.
3. **Given** a layout submission containing negative dimensions, non-numeric coordinates, or missing required fields, **When** the payload is evaluated, **Then** the system rejects the submission with HTTP 400 Bad Request and structured problem details.
4. **Given** an authenticated user belonging to Tenant A, **When** they attempt to update or reset the layout for Tenant B, **Then** the declarative authorization policy (OPA) denies the request with HTTP 403 Forbidden.

---

### User Story 2 - Public Menu Consumption with Canvas Mode & Resilient Fallback (Priority: P1)

A dining customer scans a table QR code or navigates to the public menu URL. When canvas mode is active (`canvas_enabled: true`), the public menu endpoint delivers the pre-structured `layout_config` alongside the catalog data, allowing the mobile web client to render the interactive custom visual canvas. If canvas mode is disabled, absent, or if referenced visual assets fail to load, the system guarantees zero service disruption by transparently falling back to the standard sequential category-based menu.

**Why this priority**: The public QR menu is the primary guest touchpoint. Visual customization must never jeopardize mobile load speed (< 2s LCP budget) or service availability.

**Independent Test**: Can be tested independently by requesting the public menu endpoint (`GET /api/v1/tenants/{tenant_slug}/menu`) under both canvas-enabled and canvas-disabled states, verifying that the response structure reflects the layout state and satisfies the latency budget (< 150ms server-side processing).

**Acceptance Scenarios**:

1. **Given** a tenant with `canvas_enabled: true` and a published layout, **When** a customer requests the public menu, **Then** the response includes the complete `layout_config` (background, coordinates, dimensions, layer depths) alongside categories and dishes in under 150ms.
2. **Given** a tenant with `canvas_enabled: false`, **When** a customer requests the public menu, **Then** the response includes `layout_config` with `canvas_enabled: false` and the complete sequential category catalog.
3. **Given** a canvas configuration referencing a dish identifier that was subsequently deleted from the catalog, **When** the public menu is generated, **Then** the system automatically omits the orphaned canvas element without failing the rest of the canvas layout.
4. **Given** a configured background image URL that is unreachable or returns HTTP 404, **When** rendered by the client, **Then** the system utilizes the fallback `background_color` without blocking catalog display.

---

### User Story 3 - Ahead-Of-Time (AOT) Compilation Pipeline & Perimeter CDN Purging (Priority: P2)

When the restaurant administrator finalizes menu edits and triggers publication (`POST /api/v1/admin/menu/publish`), the backend asynchronously executes an Ahead-Of-Time (AOT) compilation pipeline. The pipeline validates catalog integrity, pre-flattens the 2D layout geometry (eliminating browser-side positioning calculations), generates responsive media variant directives (`srcset` WebP/AVIF), computes a deterministic `version_hash`, updates the public menu ETag, and dispatches a selective cache purge request to the CDN edge.

**Why this priority**: AOT compilation shifts computational load from mobile devices to the backend, pre-calculates geometries, and guarantees that 100% of guest traffic can be served from CDN cache edges, achieving the < 2s LCP budget and shielding PostgreSQL from database query spikes.

**Independent Test**: Can be tested independently by invoking the publication endpoint with an authenticated administrator token, verifying the immediate HTTP 202 Accepted response with job metadata, and asserting that the compilation job produces a deterministic `version_hash` and triggers CDN cache invalidation.

**Acceptance Scenarios**:

1. **Given** an authenticated administrator with valid catalog and layout data, **When** they request menu publication (`POST /api/v1/admin/menu/publish`), **Then** the system immediately returns HTTP 202 Accepted with a unique `job_id`, status `queued`/`processing`, and an estimated SLA duration of < 3 seconds.
2. **Given** an initiated publication job, **When** the AOT compilation pipeline completes, **Then** the system marks the job status as `completed`, records the deterministic `version_hash`, and issues a selective CDN purge request for the tenant's cache tags.
3. **Given** a newly published menu version, **When** subsequent public requests supply the prior ETag in `If-None-Match`, **Then** the server or CDN edge evaluates the hash change and serves the updated payload with HTTP 200 and the new ETag header.
4. **Given** an AOT compilation failure (e.g., severe data inconsistency), **When** the pipeline encounters the error, **Then** the system logs a structured error event, maintains the prior stable compiled version active on the CDN edge, and marks the job status as `failed` without impacting live public traffic.

---

### Edge Cases

- **Orphaned Canvas Dishes:** If an administrator deletes a dish from the catalog that remains positioned on the canvas, the system omits the orphaned element during public retrieval and AOT compilation without crashing or distorting valid sibling elements.
- **Unreachable Media Assets:** If SeaweedFS background images are temporarily unavailable or timeout, the client falls back directly to the configured `background_color` or base CSS style.
- **Malformed Geometric Inputs:** If coordinates contain non-finite numbers, `NaN`, negative dimensions, or missing mandatory keys (`dish_id`, `x`, `y`, `width`, `height`, `z_index`), the validation layer rejects the payload with HTTP 400 ProblemDetails before persistence.
- **Concurrent Publication Requests:** If multiple publication requests are triggered simultaneously for the same tenant, the system deduplicates or sequences them so only the latest catalog state is compiled, avoiding race conditions.
- **CDN Purge Failure:** If the external CDN invalidation API experiences a timeout or network glitch, the system records a structured log with `error.code="CDN_PURGE_FAILURE"` and schedules an asynchronous retry while preserving the database version update.

---

## Requirements

### Functional Requirements

- **FR-001 (Ubiquitous):** The system MUST maintain complete architectural decoupling between business catalog entities (`MenuItem`, `Category`, `ModifierGroup`) and the visual presentation layer (`LayoutConfig`), persisting geometry as a semi-structured configuration.
- **FR-002 (Event-Driven):** WHEN an authorized administrator submits an updated layout (`PUT /api/v1/admin/menu/layout`), the system MUST validate geometric coordinates, ensure referenced dish IDs belong to the tenant, persist the configuration, and return HTTP 200 OK.
- **FR-003 (Event-Driven):** WHEN an authorized administrator invokes menu publication (`POST /api/v1/admin/menu/publish`), the system MUST initiate an asynchronous Ahead-Of-Time (AOT) compilation job, return HTTP 202 Accepted with a `job_id`, and complete the compilation pipeline within a 3-second SLA budget.
- **FR-004 (Ubiquitous):** The AOT compilation pipeline MUST pre-flatten layout geometries, validate active dish references, compute a deterministic `version_hash`, and issue a selective cache invalidation request to the CDN edge.
- **FR-005 (State-Driven):** WHILE `canvas_enabled` is `false` or no layout configuration exists, the system MUST deliver the public menu in the default sequential category order.
- **FR-006 (State-Driven):** WHILE a valid compiled version exists, the public menu endpoint MUST provide HTTP caching headers (`ETag`, `Cache-Control: public, max-age=3600, s-maxage=86400`) and support conditional requests (`If-None-Match` yielding HTTP 304 Not Modified).
- **FR-007 (Ubiquitous):** The system MUST strictly enforce tenant isolation across all layout mutations, public reads, and publication jobs, requiring `owner` or `tenant_admin` roles evaluated via declarative OPA policies for administrative endpoints.
- **FR-008 (Unwanted Behavior):** IF a canvas element references a dish that has been deleted or deactivated, THEN the system MUST filter out the orphaned element during menu assembly without failing the overall layout.
- **FR-009 (Unwanted Behavior):** IF an AOT compilation job encounters an unrecoverable failure, THEN the system MUST retain the last known valid compiled menu on the CDN edge and log structured telemetry with error code `AOT_COMPILATION_ERROR`.
- **FR-010 (Unwanted Behavior):** IF a CDN cache purge request fails, THEN the system MUST record a structured error event with `error.code="CDN_PURGE_FAILURE"` and execute an asynchronous retry without blocking public menu availability.

---

### Key Entities

- **Layout Configuration (`LayoutConfig`):** Additive visual configuration persisted per tenant/menu. Key attributes: `canvas_enabled` (boolean), `background_url` (URI or null), `background_color` (hex string or null), `elements` (collection of canvas elements).
- **Canvas Element (`CanvasElement`):** Individual positioned item within the layout. Key attributes: `dish_id` (UUID referencing MenuItem), `x` (number, horizontal coordinate), `y` (number, vertical coordinate), `z_index` (integer, layer depth), `width` (number, element width), `height` (number, element height).
- **Menu Publication Job (`MenuPublishJob`):** Record of an asynchronous AOT compilation run. Key attributes: `job_id` (UUID), `tenant_slug` (string), `status` (`queued`, `processing`, `completed`, `failed`), `version_hash` (string), `cdn_purge_requested` (boolean), `assets_queued` (integer), `estimated_duration_seconds` (integer), `triggered_at` (UTC timestamp), `completed_at` (UTC timestamp).

---

## Success Criteria

### Measurable Outcomes

- **SC-001 (Latency Budget Compliance):** Public menu retrieval with canvas layout delivers within a server-side processing budget of < 150ms, supporting a client-side LCP of < 2.0 seconds over standard mobile connections.
- **SC-002 (AOT Compilation SLA):** 95% of AOT menu compilation jobs complete within 3.0 seconds from trigger to CDN purge dispatch.
- **SC-003 (Zero Catalog Mutation):** 100% decoupling between canvas styling and transactional catalog entities; modifying layout geometries never modifies dish prices, descriptions, or modifier options.
- **SC-004 (Seamless Fallback):** 100% of requests successfully fall back to standard sequential layout when canvas mode is disabled or upon missing media assets without throwing 5xx errors.
- **SC-005 (Multi-Tenant Isolation):** 100% isolation of canvas configurations and publication jobs between tenants, with zero cross-tenant data leakage verified by automated tests.
- **SC-006 (CDN Cache Efficiency):** Repeat requests with matching `If-None-Match` header return HTTP 304 Not Modified within 20ms, bypassing database queries.

---

## Assumptions

- Product catalog management (categories, menu items, modifier groups) and public menu endpoint are already operational in `resto-core-back`.
- Background images for the canvas use the existing SeaweedFS presigned upload infrastructure (`POST /api/v1/storage/presigned-upload` as per `ADR-0004`).
- Administrative endpoints (`PUT /api/v1/admin/menu/layout` and `POST /api/v1/admin/menu/publish`) are protected by JWT Bearer authentication and evaluated against OPA policies (`owner` / `tenant_admin` roles).
- The client-side application is responsible for viewport-proportional coordinate scaling from desktop authoring to mobile screen sizes, as established in `ADR-0005`.
- CDN cache invalidation uses an adapter interface (e.g. `ICdnPurgeService`) that supports local mock/development environments and production CDN providers.