# Feature Specification: Multi-Tenant Digital Menu & Operations Platform (QR-Based)

**Feature Branch**: `001-multi-tenant-digital-menu`

**Created**: 2026-09-03

**Status**: Draft

**Input**: User description: "Multi-Tenant Digital Menu & Operations Platform (QR-Based) - SaaS multi-tenant para la gestion y visualizacion de cartas digitales accesibles mediante codigos QR, desacoplando vista publica de comensales respecto al backoffice CMS, integrando vistas operativas (cocina/despacho), personalizacion por comercio y control de acceso RBAC/PBAC."

---

## User Scenarios & Testing

### User Story 1 - Public QR Menu Access & Navigation (Priority: P1)

A dining customer scans a dynamic QR code placed on a table or accesses the tenant public URL (via path slug or custom subdomain) on a mobile browser. The customer views the tenant-branded digital menu loaded rapidly, navigates categories, inspects dish details (description, price, allergens, options/modifiers), and sees real-time availability without requiring an account.

**Why this priority**: Delivering the public menu is the primary commercial deliverable and revenue driver of the platform. Without rapid and reliable public menu rendering, the platform cannot operate.

**Independent Test**: Can be tested independently by querying the public menu endpoint with a valid tenant slug or subdomain and verifying that branding, categories, active dishes, prices, and allergen badges are returned within the performance budget without authentication.

**Acceptance Scenarios**:

1. **Given** an active tenant with published categories and dishes, **When** an anonymous customer navigates to the tenant menu URL via slug or subdomain, **Then** the system returns the complete public catalog with tenant branding (logo, colors, typography) in under 150ms.
2. **Given** a dish marked as paused or out-of-stock, **When** a customer views the menu, **Then** the dish is visibly marked as unavailable and cannot be selected.
3. **Given** an invalid or inactive tenant identifier, **When** a customer requests the menu, **Then** the system returns a 404 Not Found status with a structured error message indicating the restaurant is unavailable.

---

### User Story 2 - Backoffice Menu Catalog Management (Priority: P1)

An authorized restaurant administrator (`TenantAdmin` or `Manager`) logs into the backoffice dashboard to configure categories, dishes, modifiers (e.g. cooking temperature, sides), prices, dietary labels (vegan, celiac), and allergens. Any change is persisted with tenant scoping and immediately reflected on the public catalog.

**Why this priority**: Restaurant managers must have total operational autonomy to build and adjust their offerings in real time without technical support intervention.

**Independent Test**: Can be tested independently by authenticating as `TenantAdmin`, creating a category and dish with modifiers, updating its details, and confirming that the updates appear in both management queries and public catalog queries for that tenant only.

**Acceptance Scenarios**:

1. **Given** an authenticated user with `TenantAdmin` or `Manager` role, **When** they create a new category or dish with associated modifiers, **Then** the system persists the item associated with their `TenantId` and returns a 201 Created status.
2. **Given** an authenticated user attempting to create or edit items for a different `TenantId`, **When** the request is evaluated, **Then** the system rejects the request with a 403 Forbidden status, enforcing tenant isolation.
3. **Given** an existing dish, **When** the manager updates its price or pauses its availability, **Then** the public menu reflects the change instantly on subsequent requests.

---

### User Story 3 - Kitchen & Operations Real-Time Stock Control (Priority: P2)

A kitchen staff member (`KitchenStaff`) accesses a simplified operational view on a tablet or terminal in the kitchen/dispatch area. The staff member can quickly toggle item availability (pause/resume dish or modifier) when supplies run out, and inspect incoming table orders, without having permission to edit pricing, tenant settings, or staff accounts.

**Why this priority**: Operational continuity during service requires fast, error-free stock toggling directly from the kitchen without exposing administrative or financial levers.

**Independent Test**: Can be tested independently by logging in as `KitchenStaff`, toggling a dish availability state, verifying the change propagates to the public menu, and verifying that attempts to modify prices or view financial metrics return 403 Forbidden.

**Acceptance Scenarios**:

1. **Given** an authenticated user with `KitchenStaff` role, **When** they toggle a dish from available to paused, **Then** the system updates the availability status immediately and emits an event to update views.
2. **Given** a user with `KitchenStaff` role, **When** they attempt to access price modification or administrative configuration endpoints, **Then** the authorization policy denies the action with a 403 Forbidden status.

---

### User Story 4 - Multi-Tenant Provisioning & Tenant Resolution (Priority: P2)

A platform administrator (`SuperAdmin`) creates a new tenant organization, assigning an identifier (`TenantId`), a commercial slug, primary contact details, subscription tier, and initial administrative credentials. Once created, the tenant can be resolved via subdomain or URL path slug.

**Why this priority**: Required for onboarding new commercial clients onto the SaaS platform while maintaining total data partitioning.

**Independent Test**: Can be tested independently by issuing a tenant creation request as `SuperAdmin`, verifying database record insertion, and resolving the new tenant context via route slug and host header.

**Acceptance Scenarios**:

1. **Given** an authenticated `SuperAdmin`, **When** they register a new tenant with slug `la-trattoria`, **Then** the tenant is provisioned with default branding configuration and an initial `TenantAdmin` account.
2. **Given** two provisioned tenants (`Tenant A` and `Tenant B`), **When** queries are executed for `Tenant A`, **Then** no data from `Tenant B` is ever included or accessible.

---

### User Story 5 - Dynamic Vector QR Code Generation (Priority: P3)

A restaurant manager requests exportable QR codes for physical tables or for the restaurant general entrance. The system generates high-resolution vector (SVG) and raster (PNG) QR codes containing the restaurant logo in the center and encoding the table-specific or general menu URL.

**Why this priority**: Physical QR codes are the bridge connecting on-premise diners to the digital menu experience.

**Independent Test**: Can be tested independently by calling the QR generation endpoint with a valid table identifier, receiving an SVG/PNG payload, and verifying that decoding the QR yields the expected tenant menu URL with table context parameter.

**Acceptance Scenarios**:

1. **Given** a configured physical table with token/identifier, **When** a manager requests its QR code, **Then** the system returns an SVG/PNG payload embedding the table URL and tenant branding.
2. **Given** an invalid table identifier, **When** a QR code is requested, **Then** the system returns a 404 Not Found status.

---

### Edge Cases

- **Tenant Resolution Fallback**: What happens when an incoming request does not match any recognized subdomain, custom domain, or path slug? The system returns an HTTP 404 response with domain resolution failure context without leaking internal system identifiers.
- **Cross-Tenant Mutation Attempts**: What happens when an authenticated user manipulates the payload or URL parameters to include an entity ID belonging to another tenant? The data access layer and authorization middleware evaluate both the entity ownership and the user claims, rejecting the operation with HTTP 403 or 404 (ownership mismatch) and logging a security audit event with trace context.
- **Concurrent Stock Toggling**: What happens when kitchen staff pauses an item at the exact moment a diner loads the menu? The menu returns the instantaneous database state; if an order is placed on a paused item, the order validation pipeline rejects the item with HTTP 409 Conflict.
- **Malformed or Expired Authentication Tokens**: What happens when a request arrives with an expired JWT or a token missing the `TenantId` claim? The authorization middleware rejects the request with HTTP 401 Unauthorized before reaching application handlers.

---

## Requirements

### Functional Requirements

- **FR-001**: The system MUST isolate all tenant data such that no query, command, or report can access or mutate data across tenant boundaries without explicit `SuperAdmin` privileges.
- **FR-002**: The system MUST resolve tenant context dynamically from incoming requests using: (a) Host header subdomain (`{tenant}.app.com`), (b) Custom domain mapping, or (c) Path prefix (`/tenants/{tenant_slug}/` or `/r/{tenant_slug}/`).
- **FR-003**: The system MUST provide public anonymous endpoints to retrieve the active menu catalog with category hierarchy, dishes, modifier groups, allergen tags, and dietary labels in a single optimized payload.
- **FR-004**: The system MUST support granular hierarchical permissions (PBAC/RBAC) supporting at least `SuperAdmin`, `TenantAdmin`, `Manager`, and `KitchenStaff` roles.
- **FR-005**: Every authenticated request MUST validate that the JWT contains both valid user credentials and a verified `TenantId` claim matching the resolved context.
- **FR-006**: The system MUST allow authorized personnel (`TenantAdmin`, `Manager`) to manage menu categories (ordering, visibility, descriptions).
- **FR-007**: The system MUST allow authorized personnel to manage dishes with pricing, allergen identifiers, dietary tags, and nested modifier groups (mandatory or optional choices with price differentials).
- **FR-008**: The system MUST provide operational endpoints for `KitchenStaff` to toggle dish and modifier availability in real time without allowing access to pricing or administrative settings.
- **FR-009**: The system MUST store and serve tenant visual theming configuration, including primary/secondary color palettes, logo URL, cover banner URL, and chosen display layout (compact list, image grid, catalog style).
- **FR-010**: The system MUST generate vector (SVG) and raster (PNG) QR codes dynamically for both general menu access and table-specific access.
- **FR-011**: The system MUST instrument all incoming HTTP requests, database operations, and domain events with OpenTelemetry, automatically injecting `TenantId` into trace spans, metrics dimensions, and structured log contexts.
- **FR-012**: Any structural or architectural decision regarding data isolation strategy, state synchronization, or authorization pipeline MUST be documented as an Architectural Decision Record (ADR) in compliance with the MADR standard.

### Key Entities

- **Tenant**: Represents a subscriber business/restaurant. Attributes: `Id`, `Name`, `Slug`, `CustomDomain`, `Status`, `CreatedAt`, `BrandingConfig` (JSONB).
- **Category**: Grouping for menu items. Attributes: `Id`, `TenantId`, `Name`, `DisplayOrder`, `IsActive`, `Description`.
- **MenuItem (Dish)**: Individual product offered. Attributes: `Id`, `TenantId`, `CategoryId`, `Name`, `Description`, `BasePrice`, `IsAvailable`, `ImageUrl`, `Allergens` (array), `DietaryLabels` (array).
- **ModifierGroup**: Group of options applicable to a dish (e.g., cooking level, side dishes). Attributes: `Id`, `TenantId`, `MenuItemId`, `Name`, `MinSelection`, `MaxSelection`, `IsRequired`.
- **ModifierOption**: Specific selectable modifier choice. Attributes: `Id`, `ModifierGroupId`, `Name`, `PriceDelta`, `IsAvailable`.
- **Table**: Physical dining table or location inside a restaurant. Attributes: `Id`, `TenantId`, `TableNumber`, `Token`, `QrCodeUrl`.
- **User**: System user assigned to one or more tenants. Attributes: `Id`, `Email`, `TenantId`, `Role`, `IsActive`.

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Public menu endpoint response time MUST remain below 150ms p95 under a baseline load of 500 concurrent requests per tenant.
- **SC-002**: Multi-tenant isolation MUST achieve 100% test pass rate on automated cross-tenant access test suites, with zero data leakage observed.
- **SC-003**: Availability state updates from kitchen operations MUST propagate to subsequent public menu requests in under 500ms.
- **SC-004**: Vector QR code generation for tables MUST render in less than 200ms per request.
- **SC-005**: 100% of telemetry traces and structured logs emitted by the application MUST contain the contextual `TenantId` attribute whenever a tenant context is resolved.
- **SC-006**: A newly registered tenant can be fully provisioned with customized branding and initial menu in under 5 minutes without manual database intervention.

---

## Assumptions

- **Target Runtime & Framework**: The backend application will be implemented in C# 13 and .NET 9+ following Clean Architecture / Vertical Slice principles.
- **Persistence Engine**: PostgreSQL will serve as the primary relational database, using `JSONB` for flexible semi-structured data (theming, modifiers, custom attributes) and relational keys for strong integrity.
- **Authentication Infrastructure**: Authentication tokens are issued as industry-standard JWTs signed by a trusted identity authority, carrying standard claims (`sub`, `tenant_id`, `role`, `permissions`).
- **Telemetry Collector**: An OpenTelemetry-compliant collector or monitoring agent will be available in target environments to receive OTLP traces and metrics.
- **ADR Repository**: Architectural decisions will be recorded in the centralized `RestoCore-Docs` repository under `docs/adr/` in MADR format as specified in ADR-0002.
