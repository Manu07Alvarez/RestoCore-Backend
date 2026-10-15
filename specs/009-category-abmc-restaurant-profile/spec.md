# Feature Specification: Category ABMC & Restaurant Contact Profile Management

**Feature Branch**: `feature/009-category-abmc-restaurant-profile`

**Created**: 2026-10-12

**Status**: Draft

**Source Documents in RestoCore-Docs**:
- [docs/prd-admin-crud.md](file:///R:/Programando/RestoCore/RestoCore-Docs/docs/prd-admin-crud.md) (Commit `42f8e6c`)
- [docs/ears-menu.md](file:///R:/Programando/RestoCore/RestoCore-Docs/docs/ears-menu.md)
- [specs/openapi.yaml](file:///R:/Programando/RestoCore/RestoCore-Docs/specs/openapi.yaml) (Endpoints: `/api/v1/admin/categories`, `/api/v1/admin/categories/{id}`, `/api/v1/admin/tenant/profile`, `/api/v1/tenants/{tenant_slug}/menu`)

---

## 1. Context & Business Need

The management of restaurant menus and establishment identity requires administrative capabilities over two foundational areas updated in `RestoCore-Docs`:

1. **Full Lifecycle ABMC of Menu Categories (`1:N` Grouping)**:
   - Administrators must be able to list (`GET /api/v1/admin/categories`), retrieve by ID (`GET /api/v1/admin/categories/{id}`), create (`POST`), update completely (`PUT /api/v1/admin/categories/{id}`), patch attributes (`PATCH /api/v1/admin/categories/{id}`) such as `display_order` or `is_active`, and delete categories (`DELETE /api/v1/admin/categories/{id}`).
   - **Referential Integrity Enforcement (Non-Destructive Guardrail)**: Deleting a category that contains active dishes (`menu_items`) MUST be rejected with `HTTP 409 Conflict`, preventing orphaned menu items.
   - Any category change must trigger perimeter cache invalidation (`ICdnPurgeService`) within the 500ms latency budget.

2. **Restaurant Contact & Identity Profile Management**:
   - The platform must allow tenant administrators to consult (`GET /api/v1/admin/tenant/profile`), update completely (`PUT /api/v1/admin/tenant/profile`), and partially update (`PATCH /api/v1/admin/tenant/profile`) public commercial data:
     - Commercial Name (`name`)
     - Phone / WhatsApp contact number (`phone_number`)
     - Physical address (`address`)
     - Public business hours (`business_hours`)
   - The public digital QR menu (`GET /api/v1/tenants/{tenant_slug}/menu`) must expose these contact fields (`phone_number`, `address`, `business_hours`) so diners can view contact and location details.

---

## 2. User Scenarios & Testing *(mandatory)*

### User Story 1 - Full Category ABMC with Referential Integrity Guardrail (Priority: P1) 🎯 MVP

As a restaurant owner, I want to manage menu categories (create, list, inspect, edit full or partially, and delete), with the safety guarantee that I cannot accidentally delete a category that currently has dishes assigned to it.

**Why this priority**: Categories organize the entire public catalog and are the prerequisite 1:N foreign key parent for dishes.

**Independent Test**:
- Call `GET /api/v1/admin/categories` to verify ordered listing.
- Call `PUT` and `PATCH /api/v1/admin/categories/{id}` and verify updates in PostgreSQL.
- Call `DELETE /api/v1/admin/categories/{id}` on a category with dishes; verify `HTTP 409 Conflict` is returned.
- Call `DELETE /api/v1/admin/categories/{id}` on an empty category; verify `HTTP 204 No Content` or `200 OK` is returned and the category is deleted.

**Acceptance Scenarios**:
1. **Given** an authenticated tenant administrator, **When** sending `GET /api/v1/admin/categories`, **Then** the system returns `HTTP 200 OK` with an array of categories ordered by `display_order`.
2. **Given** an existing category with 3 assigned menu items, **When** the administrator sends `DELETE /api/v1/admin/categories/{id}`, **Then** the system rejects the deletion with `HTTP 409 Conflict` and an explanatory error message.
3. **Given** an existing category with 0 assigned menu items, **When** the administrator sends `DELETE /api/v1/admin/categories/{id}`, **Then** the category is removed and `HTTP 204 No Content` is returned.
4. **Given** an existing category, **When** sending `PUT` or `PATCH /api/v1/admin/categories/{id}`, **Then** attributes are updated and public menu cache is invalidated.

---

### User Story 2 - Restaurant Profile Management (Priority: P2)

As a restaurant owner, I want to manage my establishment's commercial name, phone number, address, and business hours via `/api/v1/admin/tenant/profile`.

**Why this priority**: Diners need verified contact info and operating hours displayed directly in the digital QR experience.

**Independent Test**:
- Call `GET /api/v1/admin/tenant/profile` to retrieve current profile.
- Call `PUT /api/v1/admin/tenant/profile` and `PATCH /api/v1/admin/tenant/profile` with updated `phone_number`, `address`, and `business_hours`. Verify database persistence.

**Acceptance Scenarios**:
1. **Given** an authenticated administrator, **When** querying `GET /api/v1/admin/tenant/profile`, **Then** the system returns `TenantProfileDto` with `id`, `name`, `slug`, `phone_number`, `address`, and `business_hours`.
2. **When** sending `PUT /api/v1/admin/tenant/profile` with updated contact details, **Then** the tenant record is updated in PostgreSQL and `HTTP 200 OK` is returned.
3. **When** sending `PATCH /api/v1/admin/tenant/profile` with partial fields (e.g. only `phone_number`), **Then** only specified fields are updated and remaining fields remain untouched.

---

### User Story 3 - Public Menu Exposure of Contact Profile (Priority: P3)

As a diner browsing the digital QR menu, I want to see the restaurant's phone number, address, and business hours at the top of the menu.

**Why this priority**: Diners frequently need to call the restaurant or confirm business hours without leaving the digital menu page.

**Independent Test**:
- Call `GET /api/v1/tenants/{tenant_slug}/menu` and verify `phone_number`, `address`, and `business_hours` are present in the response payload.

**Acceptance Scenarios**:
1. **Given** an active tenant with configured contact profile, **When** requesting `GET /api/v1/tenants/{tenant_slug}/menu`, **Then** the response includes `phone_number`, `address`, and `business_hours`.

---

## 3. Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide `GET /api/v1/admin/categories` returning all categories for the authenticated tenant ordered by `display_order`.
- **FR-002**: System MUST provide `GET /api/v1/admin/categories/{id}` returning detail of a single category.
- **FR-003**: System MUST provide `PUT /api/v1/admin/categories/{id}` for total category update.
- **FR-004**: System MUST provide `PATCH /api/v1/admin/categories/{id}` for partial category update.
- **FR-005**: System MUST provide `DELETE /api/v1/admin/categories/{id}`, rejecting with `HTTP 409 Conflict` if the category has associated `menu_items`.
- **FR-006**: System MUST provide `GET /api/v1/admin/tenant/profile` returning `TenantProfileDto`.
- **FR-007**: System MUST provide `PUT /api/v1/admin/tenant/profile` and `PATCH /api/v1/admin/tenant/profile` updating commercial details (`name`, `phone_number`, `address`, `business_hours`).
- **FR-008**: System MUST include `phone_number`, `address`, and `business_hours` in `PublicMenuResponse` on `GET /api/v1/tenants/{tenant_slug}/menu`.

### Key Entities

- **`Category`**:
  - `Id: Guid`, `TenantId: Guid`, `Name: string`, `Description: string?`, `DisplayOrder: int`, `IsActive: bool`
  - Relationship: `Items: ICollection<MenuItem>` (1:N)
- **`Tenant`**:
  - `PhoneNumber: string?`
  - `Address: string?`
  - `BusinessHours: string?`

---

## 4. Success Criteria *(mandatory)*

- **SC-001**: 100% compliance with OpenAPI 3.1 endpoints defined in `RestoCore-Docs/specs/openapi.yaml` for categories ABMC and tenant profile.
- **SC-002**: Category deletion blocked deterministically with HTTP 409 Conflict whenever `Items.Count > 0`.
- **SC-003**: Restaurant profile updates reflect in the public menu response within latency budget (< 500ms).
- **SC-004**: Test suite passing with 100% unit test coverage for new handlers.
