# Feature Specification: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard

**Feature Branch**: `feature/011-menu-analytics-and-telemetry`

**Created**: 2026-11-04

**Status**: Draft

**Input**: User description: "Execute sync-docs-specs workflow to compare RestoCore-Docs architectural decisions and OpenAPI contracts against resto-core-back, address the highest priority gap (ADR-0011 Menu Analytics & Telemetry, ADR-0012 Table Tracking Deferral), and specify telemetry ingestion and menu engineering matrix."

**Source Architecture Decisions & Contracts**:
- [ADR-0011: Jerarquia de Metricas Analiticas por Fases y Modelo de Telemetria de Menu](../../../RestoCore-Docs/docs/adr/0011-jerarquia-de-metricas-analiticas-y-telemetria-de-menu.md)
- [ADR-0012: Postergacion del Seguimiento de Ocupacion de Mesas por QR y Vision de Modulo de Turnos](../../../RestoCore-Docs/docs/adr/0012-postergacion-seguimiento-ocupacion-mesas-qr.md)
- [OpenAPI 3.1 Canonical Specification](../../../RestoCore-Docs/specs/openapi.yaml) (`POST /api/v1/analytics/collect`, `GET /api/v1/admin/analytics/dashboard`)
- [Security Policies](../../../RestoCore-Docs/specs/policies/) (Declarative OPA / Rego authorization for tenant owners and administrators)

---

## 1. Context & Business Need

To make sound business decisions, restaurant owners and operations managers require actionable intelligence regarding diner engagement with the digital menu, popularity of specific dishes, and category interaction frequency.

Historically, attempting to implement monolithic analytics suites in hospitality platforms introduces severe architectural and operational risks:
1. **Performance Degradation of Public Menu**: Injecting synchronous tracking logic or third-party client trackers (Google Analytics, Mixpanel) directly into the public mobile web client adds bundle overhead and network roundtrips, violating the strict performance budget of < 2s Largest Contentful Paint (LCP) on mobile networks.
2. **Premature Coupling with External Systems**: Advanced financial metrics such as Prime Cost (COGS + Labor) or Food Cost Variance require integrations with third-party Point of Sale (POS), Recipe Management Systems (RMS), and Kitchen Display Systems (KDS) that are absent during MVP rollout.
3. **Flawed Heuristics for Table Occupancy**: Attempting to infer physical table turnover or RevPASH from QR scans produces erroneous data and database lock contention (addressed and formally deferred by ADR-0012).

Per **ADR-0011** and **ADR-0012**, RestoCore implements a phased, decoupled telemetry and analytics architecture:
- **Phase 1 (MVP - High Priority)**: High-throughput, non-blocking ingestion of client interaction events (`POST /api/v1/analytics/collect`) returning `HTTP 202 Accepted` in < 50ms without collecting Personally Identifiable Information (PII); automated computation of the **Menu Engineering Matrix** (Stars, Plowhorses, Puzzles, Dogs) combining item popularity with contribution margin (`base_price - estimated_cost`); aggregated QR visit counts (daily and monthly); and category ranking.
- **Phase 2 (Pro Plan - Medium Priority)**: Temporal demand analysis (hourly peak visit curves) and device category breakdown (mobile vs. desktop vs. tablet) accessible via `GET /api/v1/admin/analytics/dashboard`. In strict adherence to ADR-0012, QR-derived table dwell time, physical occupancy, and turnover metrics remain excluded until the dedicated Waitlist / Turnos module is introduced.
- **Phase 3 (Enterprise - Low Priority / Deferred)**: Prime cost, food cost variance, and kitchen ticket duration dependent on external software integrations.

---

## 2. User Scenarios & Testing *(mandatory)*

### User Story 1 - Asynchronous Ingestion of Diner Interaction Events (Priority: P1) - MVP Foundation

As an anonymous diner scanning a restaurant table QR code or navigating the public digital menu, I want my interactions (menu views, dish detail modal views, category clicks) to be registered asynchronously without any perceptible delay, network overhead, or collection of my personal data.

**Why this priority**: Core ingestion pathway. If analytics event collection slows down menu rendering, it degrades the diner experience and violates the mobile performance budget (< 2s LCP).

**Independent Test**: Send `POST /api/v1/analytics/collect` payloads with `event_type` (`menu_view`, `item_view`, `category_click`) and `tenant_slug`; verify the endpoint responds with `HTTP 202 Accepted` within 50ms, processes the event asynchronously without database deadlocks, rejects malformed payloads with `HTTP 400 Bad Request`, and stores zero PII (no IP address or user-agent fingerprint).

**Acceptance Scenarios**:
1. **Given** an anonymous customer opening the digital menu for an active tenant (`tenant_slug`), **When** the web application sends `POST /api/v1/analytics/collect` with `event_type: "menu_view"`, **Then** the server responds with `HTTP 202 Accepted` and enqueues the event into the asynchronous analytics pipeline.
2. **Given** a diner viewing a specific menu item, **When** the client sends `POST /api/v1/analytics/collect` with `event_type: "item_view"` and a valid `item_id`, **Then** the system records the item interaction count and responds with `HTTP 202 Accepted`.
3. **Given** an incoming request with an invalid `event_type`, missing `tenant_slug`, or malformed UUID for `item_id` or `category_id`, **When** evaluated by the API validation pipeline, **Then** the server responds with `HTTP 400 Bad Request` and an RFC 7807 Problem Details payload without enqueuing corrupted data.

---

### User Story 2 - Executive Analytics Dashboard & Menu Engineering Matrix (Priority: P2)

As a restaurant owner or tenant administrator, I want to access an administrative analytics dashboard that consolidates QR visit volume, top viewed items, top categories, and an automated Menu Engineering Matrix (Stars, Plowhorses, Puzzles, Dogs) over a specified date range, so that I can optimize dish pricing and catalog placement based on empirical demand and profitability.

**Why this priority**: Primary commercial value driver for restaurant management. Transforms raw telemetry into strategic insights regarding item margins and consumer preference.

**Independent Test**: Authenticate as `owner` or `tenant_admin` and query `GET /api/v1/admin/analytics/dashboard?from={start_date}&to={end_date}`; verify `HTTP 200 OK` is returned with populated `priority_high_metrics`, including accurate visit totals, item views, category counts, and classified item UUIDs in each matrix quadrant. Verify that users with role `cook`, `waiter`, or unauthenticated requests are rejected with `HTTP 403 Forbidden` or `HTTP 401 Unauthorized`.

**Acceptance Scenarios**:
1. **Given** an authenticated user with `owner` or `tenant_admin` role for a tenant, **When** invoking `GET /api/v1/admin/analytics/dashboard` with valid date parameters `from` and `to`, **Then** the system returns `HTTP 200 OK` with `priority_high_metrics` containing `total_qr_visits`, `monthly_qr_visits`, `top_viewed_items`, `top_categories`, and the four quadrants of `menu_engineering_matrix`.
2. **Given** an active tenant with dishes having varying popularity and contribution margins, **When** the dashboard computes the Menu Engineering Matrix, **Then** items are classified objectively into:
   - **Stars**: Above-average popularity and above-average contribution margin.
   - **Plowhorses**: Above-average popularity and below-average contribution margin.
   - **Puzzles**: Below-average popularity and above-average contribution margin.
   - **Dogs**: Below-average popularity and below-average contribution margin.
3. **Given** an authenticated user belonging to Tenant A, **When** attempting to query analytics metrics for Tenant B or accessing the endpoint without tenant context, **Then** the authorization engine (OPA/Rego) rejects the request with `HTTP 403 Forbidden`, preserving strict multi-tenant isolation.

---

### User Story 3 - Temporal Demand Curve & Device Distribution for Pro Tenants (Priority: P3)

As a Pro Plan restaurant owner, I want to view hourly peak traffic curves and device breakdown (mobile vs. desktop vs. tablet) over the selected reporting window, while ensuring that inaccurate table turnover and dwell-time heuristics are strictly omitted per ADR-0012.

**Why this priority**: Operational staffing and schedule optimization. Knowing peak rush hours allows managers to schedule waitstaff and kitchen line prep accurately.

**Independent Test**: Query `GET /api/v1/admin/analytics/dashboard` for a Pro-tier tenant; verify `priority_medium_metrics` contains an array of 24 hourly visit aggregates and device percentage ratios. Verify for standard/basic tier tenants that `priority_medium_metrics` is returned as `null`. Verify that no table turnover, dwell time, or RevPASH metrics are exposed in any response.

**Acceptance Scenarios**:
1. **Given** an authenticated administrator of a tenant with an active Pro subscription, **When** retrieving the analytics dashboard, **Then** the response includes `priority_medium_metrics` containing the `peak_hours` distribution (0 to 23 hours) and `device_breakdown` percentages.
2. **Given** an administrator of a tenant on the standard/free tier, **When** retrieving the analytics dashboard, **Then** `priority_medium_metrics` is `null` while `priority_high_metrics` is fully available.
3. **Given** any analytics dashboard response across all subscription tiers, **When** verifying response schema compliance, **Then** table turnover, QR dwell duration, and table occupancy metrics are absent, honoring the ADR-0012 deferral.

---

### Edge Cases

- **Zero Interaction Volume**: When a newly created tenant or a date range with zero events is queried, the dashboard MUST return 200 OK with `total_qr_visits = 0`, `monthly_qr_visits = 0`, and empty lists for top items, top categories, and all four matrix quadrants (`stars`, `plowhorses`, `puzzles`, `dogs`), rather than returning 404, 500, or null references.
- **Soft-Deleted Dishes**: Dishes soft-deleted (`is_deleted == true`) within the requested time frame MUST preserve their historical view counts in analytics totals, but MUST be excluded from the active Menu Engineering Matrix recommendations if marked as archived.
- **Single-Item, Zero-View, or Uniform-Margin Catalogs**: When a catalog has zero total views across all items, the system MUST return empty arrays for all quadrants. If all items have identical non-zero view counts or identical contribution margins, the threshold computation MUST handle division-by-zero and zero-variance edge cases deterministically (classifying active items with positive margin into `stars`).
- **Unconfigured Item Costs**: If `estimated_cost` is null or unconfigured on dishes, the system MUST fallback to the tenant's default margin ratio (defaulting to 30% cost / 70% contribution margin) to ensure immediate and deterministic calculation without runtime failures.
- **High-Burst Event Storms**: In the event of a sudden spike in traffic (e.g., 1000 concurrent QR scans during restaurant opening), the ingestion endpoint MUST persist or buffer events without triggering connection pool exhaustion in PostgreSQL.
- **Cross-Midnight Date Intervals**: Date range filters spanning month or year boundaries MUST be processed using UTC timestamps to prevent off-by-one aggregation errors caused by daylight saving time shifts.

---

## 3. Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST expose a public endpoint `POST /api/v1/analytics/collect` conforming to the OpenAPI 3.1 specification, accepting `AnalyticsEventRequest` payloads containing `event_type` (`menu_view`, `item_view`, `category_click`), `tenant_slug`, optional `item_id`, optional `category_id`, and optional `device_type`.
- **FR-002**: The ingestion endpoint `POST /api/v1/analytics/collect` MUST respond with `HTTP 202 Accepted` within 50ms p95, delegating event persistence and metric aggregation to an asynchronous background worker or message buffer.
- **FR-003**: The telemetry ingestion system MUST comply with platform privacy guidelines: IP addresses, user-agent fingerprints, session cookies, and Personally Identifiable Information (PII) MUST NOT be stored.
- **FR-004**: The system MUST expose an administrative endpoint `GET /api/v1/admin/analytics/dashboard` secured by JWT Bearer authentication and declarative OPA authorization policies permitting access exclusively to `owner` and `tenant_admin` roles.
- **FR-005**: The dashboard endpoint MUST support optional ISO-8601 query parameters `from` and `to` to filter analysis periods. When omitted, defaults MUST span the last 30 calendar days.
- **FR-006**: The dashboard response MUST return `AnalyticsDashboardResponse` containing top-level fields `tenant_id` (UUID), `period_from` (Date), `period_to` (Date), and `priority_high_metrics`:
  - `total_qr_visits`: Total menu visits in the requested period.
  - `monthly_qr_visits`: Visits within the trailing 30 days.
  - `top_viewed_items`: Ordered list of top dishes with `item_id`, `name`, and `views_count`.
  - `top_categories`: Ordered list of top categories with `category_id`, `name`, and `interactions_count`.
  - `menu_engineering_matrix`: Object containing four arrays of item UUIDs (`stars`, `plowhorses`, `puzzles`, `dogs`).
- **FR-007**: The Menu Engineering Matrix calculation MUST compute the average popularity (mean view count across tenant items) and average contribution margin (mean of `base_price - estimated_cost`), classifying each active item accordingly:
  - `stars`: views >= average views AND margin >= average margin.
  - `plowhorses`: views >= average views AND margin < average margin.
  - `puzzles`: views < average views AND margin >= average margin.
  - `dogs`: views < average views AND margin < average margin.
  - *Default Cost Fallback*: If `estimated_cost` is null or unconfigured for an item, the system MUST fallback to a tenant-level configurable default cost ratio (defaulting to 30% of `base_price`, yielding an assumed 70% contribution margin), preventing null arithmetic or unhandled exceptions.
  - *Zero Interaction Handling*: If total catalog view count during the reporting window is 0, the matrix computation MUST NOT classify unviewed items into any quadrant, returning empty arrays `[]` for all four quadrants (`stars`, `plowhorses`, `puzzles`, `dogs`).
  - *Uniform-Variance Handling*: If all items possess identical non-zero view counts and margins greater than 0, items MUST default to the `stars` quadrant if margin is positive, avoiding division-by-zero or non-deterministic behavior.
- **FR-008**: For tenants with the Pro feature tier, the dashboard MUST populate `priority_medium_metrics` containing `peak_hours` (array of objects with `hour` [0 to 23] and `visits_count`) and `device_breakdown` (object with `mobile_percentage` and `desktop_percentage` numerical values). For non-Pro tenants, this field MUST be `null`.
- **FR-009**: In strict compliance with ADR-0012, the backend MUST NOT calculate, estimate, or return table occupancy, table turnover, or RevPASH metrics based on QR code telemetry.
- **FR-010**: All analytics data ingestion, aggregation tables, and queries MUST enforce strict multi-tenant isolation by `tenant_id`.

### Key Entities

- **AnalyticsEvent**: Ephemeral or buffered interaction record capturing tenant association, event type (`menu_view`, `item_view`, `category_click`), target entity reference (`item_id`, `category_id`), device classification (`mobile`, `desktop`, `tablet`), and UTC timestamp.
- **DailyMenuTrafficSummary**: Aggregated roll-up table storing daily totals per tenant for QR menu opens, item impressions, and category clicks, enabling high-performance dashboard reads without full-table scans.
- **MenuEngineeringMatrix**: Computed analytical structure categorizing active dishes into Stars, Plowhorses, Puzzles, and Dogs based on statistical averages of popularity and margin for a given reporting period.

---

## 4. Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Ingestion endpoint `POST /api/v1/analytics/collect` latency p95 is under 50ms under simulated production load (500 req/sec), adding zero overhead to the public menu loading budget (< 2s LCP).
- **SC-002**: Dashboard endpoint `GET /api/v1/admin/analytics/dashboard` executes in under 300ms p95 for a 30-day reporting window across a catalog of 200 items.
- **SC-003**: 100% of tested analytics queries enforce tenant scoping, with zero cross-tenant data leakage verified in automated security integration tests.
- **SC-004**: Zero PII collected: automated compliance audits confirm that IP addresses and user agents are excluded from database schemas and persistence stores.
- **SC-005**: Unit and integration test coverage for the analytics ingestion endpoint, aggregation service, and dashboard query handler exceeds 85%.

---

## 5. Assumptions & Scope Boundaries

- **Unit Cost for Margin Computation**: If explicit dish ingredient/production cost is not yet configured for a dish, the system assumes a configurable default cost ratio (e.g., 30% of base price, or gross margin based on catalog defaults) to enable immediate matrix calculation during MVP.
- **Exclusion of Waitlist & Table Turnover**: Per ADR-0012, tracking physical table turnover and comensal stay duration is explicitly deferred to the future Waitlist / Turnos module and is excluded from this specification.
- **Third-Party POS & RMS Integrations**: Prime Cost, actual food cost variance, and kitchen ticket timing (Level 3 / Phase 3 of ADR-0011) are out of scope for this feature specification.
