# Data Model & Storage Specifications: Containerized Environment & Asset Management

**Feature**: `002-docker-dev-testing`
**Storage Providers**: PostgreSQL 16+ (Relational + JSONB), SeaweedFS (S3 Object Storage), Redis 7+ (Key-Value & Streams)

---

## 1. Storage Schemas & Buckets

### 1.1. SeaweedFS S3 Bucket: `restocore-images`
Public asset bucket for restaurant branding, logos, and dish photos.

| Namespace / Prefix | Access Policy | Format | Description |
| :--- | :--- | :--- | :--- |
| `tenants/{tenant_id}/branding/` | Public Read / Authenticated Pre-signed PUT | `image/webp`, `image/png`, `image/jpeg` | Restaurant logos and banner images |
| `tenants/{tenant_id}/dishes/` | Public Read / Authenticated Pre-signed PUT | `image/webp`, `image/jpeg` | Menu item dish photos |

**Presigned Upload Contract:**
- Expiration TTL: 15 minutes (900 seconds)
- HTTP Method: `PUT`
- Permitted Content-Types: `image/jpeg`, `image/png`, `image/webp`
- Max Size: 5 MB

---

### 1.2. Pre-Signed URL Value Objects & DTOs

```json
{
  "upload_url": "http://localhost:8333/restocore-images/tenants/11111111-1111-1111-1111-111111111111/dishes/ravioles.webp?X-Amz-Algorithm=AWS4-HMAC-SHA256&...",
  "public_url": "http://localhost:8333/restocore-images/tenants/11111111-1111-1111-1111-111111111111/dishes/ravioles.webp",
  "expires_at": "2026-09-06T22:15:00Z"
}
```

---

## 2. Infrastructure Service Definitions

| Service | Container Image | Host Port | Internal Port | Health Check Mechanism |
| :--- | :--- | :--- | :--- | :--- |
| **PostgreSQL** | `postgres:16-alpine` | `5432` | `5432` | `pg_isready -U postgres -d restocore_dev` |
| **Redis** | `redis:7-alpine` | `6379` | `6379` | `redis-cli ping` |
| **SeaweedFS** | `chrislusf/seaweedfs:latest` | `8333`, `8888`, `9333` | `8333`, `8888`, `9333` | `wget -q -O - http://localhost:8333/ || exit 1` |
| **OPA** | `openpolicyagent/opa:latest` | `8181` | `8181` | `wget -q -O - http://localhost:8181/v1/data || exit 1` |
