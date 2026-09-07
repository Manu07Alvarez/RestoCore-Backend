package restocore.authz

import future.keywords.in
import future.keywords.if

default allow := false

# Public Menu Access
allow if {
    input.method == "GET"
    input.path = ["api", "v1", "tenants", _, "menu"]
}

# Super Admin Global Access
allow if {
    "super_admin" in input.user.roles
}

# Tenant Owner or Administrator Access
allow if {
    input.user.tenant_id == input.resource.tenant_id
    some role in input.user.roles
    role in ["owner", "tenant_admin"]
    input.action in [
        "read_dashboard",
        "manage_menu",
        "manage_categories",
        "manage_dishes",
        "manage_prices",
        "manage_branding",
        "manage_tables",
        "generate_presigned_url",
        "read_metrics",
        "manage_staff",
        "read_orders",
        "manage_orders"
    ]
}

# Kitchen Staff Operational Access
allow if {
    input.user.tenant_id == input.resource.tenant_id
    some role in input.user.roles
    role in ["cook", "kitchen_staff"]
    input.method in ["GET", "PATCH"]
    input.action in ["read_kitchen_orders", "update_order_status", "kitchen_stock"]
}

# Waiter Hall Access
allow if {
    input.user.tenant_id == input.resource.tenant_id
    some role in input.user.roles
    role == "waiter"
    input.action in ["read_tables", "create_order", "read_orders", "update_order_status"]
}