# Phase 0 Research: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard

**Feature**: `011-menu-analytics-and-telemetry`  
**Date**: 2026-11-04  
**References**: ADR-0011, ADR-0012, OpenAPI 3.1 (`POST /api/v1/analytics/collect`, `GET /api/v1/admin/analytics/dashboard`)

---

## 1. Asynchronous Telemetry Ingestion Mechanism (`< 50ms p95`)

- **Decision**: Use an asynchronous queue service (`IAnalyticsEventQueue` backed by Redis Streams / `System.Threading.Channels`) coupled with a background processor (`AnalyticsAggregationWorker : BackgroundService`) that persists pre-aggregated daily and hourly roll-ups into PostgreSQL using atomic upserts.
- **Rationale**:
  - Synchronous `INSERT`/`UPDATE` operations into PostgreSQL on every public QR menu open (`menu_view`), dish modal open (`item_view`), or category click (`category_click`) create row-level lock contention on hot catalog items during peak dining hours and risk violating the `< 50ms p95` ingestion budget (`SC-001`) and `< 2s LCP` public menu budget.
  - Enqueuing the validated telemetry event asynchronously allows `POST /api/v1/analytics/collect` to return `HTTP 202 Accepted` in `< 5ms` after lightweight schema and tenant slug validation.
  - Background batch aggregation collapses high-burst event storms (`1000 req/s`) into consolidated roll-up increments per `(TenantId, DateUtc)`.
- **Alternatives Considered**:
  - *Direct Synchronous PostgreSQL Writes*: Rejected due to connection pool exhaustion under burst loads and higher tail latency.
  - *Third-Party Client Trackers (Google Analytics / Mixpanel)*: Rejected per ADR-0011 because client-side scripts degrade mobile LCP, cannot join with PostgreSQL dish margins (`BasePrice`, `EstimatedCost`), and introduce PII compliance concerns.

---

## 2. Pre-Aggregated Roll-Up Schema vs. Raw Event Scans

- **Decision**: Persist daily and hourly roll-up tables (`DailyTenantTraffic`, `DailyItemInteraction`, `DailyCategoryInteraction`, `HourlyTenantTraffic`) indexed by `(TenantId, DateUtc)` rather than executing `COUNT(*)` full-table scans over raw event logs at dashboard query time.
- **Rationale**:
  - `SC-002` requires `GET /api/v1/admin/analytics/dashboard` to execute in `< 300ms p95` for a 30-day window across a 200-item catalog.
  - Querying pre-aggregated daily/hourly summaries bounded by `(TenantId, DateUtc >= from AND DateUtc <= to)` scans at most 30 rows per metric group, guaranteeing single-digit millisecond database execution times.
  - Strictly satisfies `FR-003` and `SC-004` (Zero PII) because roll-up counters store only aggregate counts (`MenuViewsCount`, `MobileCount`, `DesktopCount`, `TabletCount`, `ViewsCount`, `InteractionsCount`) without IP addresses, user agents, or session identifiers.
- **Alternatives Considered**:
  - *Raw Event Table with On-the-Fly `GROUP BY`*: Rejected because millions of raw `item_view` rows degrade read latency over 30-day windows.

---

## 3. Menu Engineering Matrix Computation & Deterministic Edge Cases

- **Decision**: Compute the Menu Engineering Matrix dynamically in the `GetAnalyticsDashboardQueryHandler` by joining active tenant `MenuItem` records (`IsDeleted == false`) with their aggregated view counts for `[from, to]`.
  - Item contribution margin: $\text{Margin}_i = \text{BasePrice}_i - \text{EffectiveCost}_i$, where $\text{EffectiveCost}_i = \text{EstimatedCost}_i ?? (0.30 \times \text{BasePrice}_i)$.
  - Mean popularity: $\bar{V} = \frac{1}{N}\sum_{i=1}^{N} V_i$ across active catalog items $N$.
  - Mean contribution margin: $\bar{M} = \frac{1}{N}\sum_{i=1}^{N} \text{Margin}_i$.
  - Quadrant rules (`FR-007`):
    - **Zero total views guard**: If $\sum V_i == 0$ or $N == 0$, all four quadrant arrays (`stars`, `plowhorses`, `puzzles`, `dogs`) return `[]`.
    - **Classification when $\sum V_i > 0$**:
      - `stars`: $V_i \ge \bar{V}$ and $\text{Margin}_i \ge \bar{M}$ (with positive margin fallback when variance is zero).
      - `plowhorses`: $V_i \ge \bar{V}$ and $\text{Margin}_i < \bar{M}$.
      - `puzzles`: $V_i < \bar{V}$ and $\text{Margin}_i \ge \bar{M}$.
      - `dogs`: $V_i < \bar{V}$ and $\text{Margin}_i < \bar{M}$.
- **Rationale**:
  - Using `>=` for both thresholds naturally handles the uniform-variance edge case (`FR-007`): when all active items have identical non-zero views ($V_i == \bar{V}$) and identical positive margins ($\text{Margin}_i == \bar{M} > 0$), they deterministically fall into `stars` without division by zero.
  - Soft-deleted items (`IsDeleted == true`) remain included in `top_viewed_items` historical rankings if they accrued views in the date range, but are excluded from the active `menu_engineering_matrix` recommendations (`Edge Cases` & `FR-007`).

---

## 4. Declarative OPA Authorization & Multi-Tenant Isolation

- **Decision**: Secure `GET /api/v1/admin/analytics/dashboard` using policy `AnalyticsReadOnly` (mapped to `OpaRequirement("read_metrics")`), which is already granted to `owner` and `tenant_admin` roles in `specs/policies/authz.rego` (and `policies/authz.rego`).
- **Rationale**:
  - Aligns with Constitution Principle II and ADR-0006. Roles `cook` and `waiter` do not have `"read_metrics"` in `authz.rego`, so OPA deterministically denies them with `HTTP 403 Forbidden`.
  - Every dashboard query scopes data strictly by `ITenantContext.TenantId`.

---

## 5. Deferral of QR Table Occupancy & Turnover (ADR-0012 Compliance)

- **Decision**: Omit `table_token`, table dwell time, table turnover, and RevPASH from both `AnalyticsEventRequest` persistence and `AnalyticsDashboardResponse`.
- **Rationale**:
  - Strictly enforces ADR-0012 and `FR-009`, avoiding inaccurate heuristic session tracking and unnecessary TTL sweeping load.
