# Data Model Specification: Multi-Tenant Digital Menu & Operations Platform

**Feature**: `001-multi-tenant-digital-menu`
**Database Engine**: PostgreSQL 16+ (Relational + JSONB)
**ORM**: Entity Framework Core 9 (Npgsql provider)

---

## 1. Entity Definitions & Schemas

### 1.1. Tenant (`tenants`)
Represents an independent restaurant establishment subscribed to the platform.

| Column | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `UUID` | Primary Key, `gen_random_uuid()` | Unique tenant identifier |
| `name` | `VARCHAR(100)` | NOT NULL | Commercial name of the restaurant |
| `slug` | `VARCHAR(50)` | NOT NULL, UNIQUE | URL-safe slug used in path routing |
| `custom_domain` | `VARCHAR(150)` | NULL, UNIQUE | Optional custom domain (e.g. `menu.bistro.com`) |
| `status` | `VARCHAR(20)` | NOT NULL, DEFAULT `'Active'` | Tenant status (`Active`, `Suspended`, `Archived`) |
| `branding_config` | `JSONB` | NOT NULL, DEFAULT `'{}'` | Visual theme settings (colors, logo, typography) |
| `created_at` | `TIMESTAMPTZ` | NOT NULL, DEFAULT `NOW()` | Audit creation timestamp |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL, DEFAULT `NOW()` | Audit update timestamp |

**`branding_config` JSONB Schema:**
```json
{
  "primary_color": "#E63946",
  "secondary_color": "#1D3557",
  "background_color": "#F8F9FA",
  "text_color": "#2B2D42",
  "logo_url": "https://cdn.restocore.app/tenants/uuid/logo.webp",
  "cover_banner_url": "https://cdn.restocore.app/tenants/uuid/banner.webp",
  "font_family": "Inter, sans-serif",
  "layout_mode": "GridWithImages"
}
```

---

### 1.2. Category (`categories`)
Groupings used to organize menu items in the public catalog.

| Column | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `UUID` | Primary Key, `gen_random_uuid()` | Unique category identifier |
| `tenant_id` | `UUID` | NOT NULL, FK -> `tenants(id)` ON DELETE CASCADE | Scoped tenant owner |
| `name` | `VARCHAR(80)` | NOT NULL | Display name (e.g. "Entradas", "Bebidas") |
| `description` | `VARCHAR(250)` | NULL | Optional description |
| `display_order` | `INTEGER` | NOT NULL, DEFAULT 0 | Sort order for display |
| `is_active` | `BOOLEAN` | NOT NULL, DEFAULT TRUE | Visibility toggle |
| `created_at` | `TIMESTAMPTZ` | NOT NULL, DEFAULT `NOW()` | Audit creation timestamp |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL, DEFAULT `NOW()` | Audit update timestamp |

---

### 1.3. MenuItem (`menu_items`)
Individual dishes or beverages offered by the restaurant.

| Column | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `UUID` | Primary Key, `gen_random_uuid()` | Unique menu item identifier |
| `tenant_id` | `UUID` | NOT NULL, FK -> `tenants(id)` ON DELETE CASCADE | Scoped tenant owner |
| `category_id` | `UUID` | NOT NULL, FK -> `categories(id)` ON DELETE RESTRICT | Assigned category |
| `name` | `VARCHAR(100)` | NOT NULL | Item name |
| `description` | `TEXT` | NULL | Detailed preparation description |
| `base_price` | `NUMERIC(10, 2)` | NOT NULL, CHECK (`base_price >= 0`) | Base price in tenant currency |
| `is_available` | `BOOLEAN` | NOT NULL, DEFAULT TRUE | Kitchen availability toggle |
| `image_url` | `VARCHAR(300)` | NULL | Direct SeaweedFS/CDN URL |
| `display_order` | `INTEGER` | NOT NULL, DEFAULT 0 | Sort order within category |
| `allergens` | `VARCHAR(50)[]` | NOT NULL, DEFAULT `'{}'` | Array of allergens (e.g. `["gluten", "dairy"]`) |
| `dietary_labels`| `VARCHAR(50)[]` | NOT NULL, DEFAULT `'{}'` | Array of labels (e.g. `["vegan", "celiac"]`) |
| `metadata` | `JSONB` | NOT NULL, DEFAULT `'{}'` | Flexible attributes (calories, prep time) |
| `created_at` | `TIMESTAMPTZ` | NOT NULL, DEFAULT `NOW()` | Audit creation timestamp |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL, DEFAULT `NOW()` | Audit update timestamp |

---

### 1.4. ModifierGroup (`modifier_groups`)
Option sets linked to a dish (e.g., meat doneness, extra toppings, choice of sauce).

| Column | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `UUID` | Primary Key, `gen_random_uuid()` | Unique modifier group identifier |
| `tenant_id` | `UUID` | NOT NULL, FK -> `tenants(id)` ON DELETE CASCADE | Scoped tenant owner |
| `menu_item_id` | `UUID` | NOT NULL, FK -> `menu_items(id)` ON DELETE CASCADE | Parent dish |
| `name` | `VARCHAR(80)` | NOT NULL | Group name (e.g. "Punto de Cocción") |
| `min_selection` | `INTEGER` | NOT NULL, DEFAULT 0, CHECK (`min_selection >= 0`) | Minimum required selections |
| `max_selection` | `INTEGER` | NOT NULL, DEFAULT 1, CHECK (`max_selection >= min_selection`) | Maximum allowed selections |
| `is_required` | `BOOLEAN` | NOT NULL, DEFAULT FALSE | Whether selection is mandatory |

---

### 1.5. ModifierOption (`modifier_options`)
Individual selectable choices belonging to a modifier group.

| Column | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `UUID` | Primary Key, `gen_random_uuid()` | Unique modifier option identifier |
| `tenant_id` | `UUID` | NOT NULL, FK -> `tenants(id)` ON DELETE CASCADE | Scoped tenant owner |
| `modifier_group_id` | `UUID` | NOT NULL, FK -> `modifier_groups(id)` ON DELETE CASCADE | Parent group |
| `name` | `VARCHAR(80)` | NOT NULL | Option name (e.g. "A punto", "Queso extra") |
| `price_delta` | `NUMERIC(10, 2)` | NOT NULL, DEFAULT 0.00 | Price addition/deduction |
| `is_available` | `BOOLEAN` | NOT NULL, DEFAULT TRUE | Real-time option availability |

---

### 1.6. Table (`tables`)
Represents physical seating tables for dining and QR generation.

| Column | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `UUID` | Primary Key, `gen_random_uuid()` | Unique table identifier |
| `tenant_id` | `UUID` | NOT NULL, FK -> `tenants(id)` ON DELETE CASCADE | Scoped tenant owner |
| `table_number` | `INTEGER` | NOT NULL | Table number displayed to staff |
| `token` | `VARCHAR(64)` | NOT NULL, UNIQUE | Cryptographic verification token |
| `label` | `VARCHAR(50)` | NULL | Optional label (e.g. "Terraza 4") |
| `is_active` | `BOOLEAN` | NOT NULL, DEFAULT TRUE | Active status |

---

## 2. Indexes & Performance Optimization

```sql
-- Multi-tenant composite indexes for fast catalog reads
CREATE INDEX idx_tenants_slug ON tenants (slug);
CREATE INDEX idx_categories_tenant_order ON categories (tenant_id, display_order) WHERE is_active = TRUE;
CREATE INDEX idx_menu_items_tenant_category ON menu_items (tenant_id, category_id, display_order);
CREATE INDEX idx_menu_items_availability ON menu_items (tenant_id, is_available);
CREATE INDEX idx_tables_tenant_token ON tables (tenant_id, token);

-- GIN indexes for JSONB & Array searches
CREATE INDEX idx_tenants_branding_gin ON tenants USING GIN (branding_config);
CREATE INDEX idx_menu_items_metadata_gin ON menu_items USING GIN (metadata);
CREATE INDEX idx_menu_items_allergens_gin ON menu_items USING GIN (allergens);
CREATE INDEX idx_menu_items_dietary_gin ON menu_items USING GIN (dietary_labels);
```

---

## 3. Entity State Transitions

### Dish Availability Transition
```text
[Available] <---(KitchenStaff toggle)---> [Paused / OutOfStock]
     |                                          |
     +---(TenantAdmin/Manager archive)--------> [Archived]
```

- When a dish is marked `is_available = FALSE`, it remains visible in the catalog with an `out_of_stock` indicator unless the category itself is inactive.
- Orders submitted with an unavailable item are rejected at the command handler level with HTTP 409 Conflict.
