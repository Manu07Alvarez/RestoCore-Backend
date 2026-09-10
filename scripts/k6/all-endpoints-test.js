import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '3s', target: 5 },
    { duration: '10s', target: 10 },
    { duration: '2s', target: 0 },
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500'],
    'checks': ['rate>0.99'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5221';
const TENANT_SLUG = __ENV.TENANT_SLUG || 'bodegon-0320458';

export default function () {
  // 1. Health Checks
  const liveRes = http.get(`${BASE_URL}/healthz`);
  check(liveRes, { 'GET /healthz status is 200': (r) => r.status === 200 });

  const readyRes = http.get(`${BASE_URL}/ready`);
  check(readyRes, { 'GET /ready status is 200': (r) => r.status === 200 });

  // 2. Swagger Endpoints
  const swaggerDocRes = http.get(`${BASE_URL}/swagger/v1/swagger.json`);
  check(swaggerDocRes, { 'GET /swagger/v1/swagger.json status is 200': (r) => r.status === 200 });

  const swaggerUiRes = http.get(`${BASE_URL}/swagger/index.html`);
  check(swaggerUiRes, { 'GET /swagger/index.html status is 200': (r) => r.status === 200 });

  // 3. Public Digital Menu (Dynamic)
  const menuRes = http.get(`${BASE_URL}/api/v1/tenants/${TENANT_SLUG}/menu`);
  check(menuRes, { 'GET /menu status is 200': (r) => r.status === 200 });

  // 4. Public Digital Menu (Cached ETag 304)
  const etag = menuRes.headers['Etag'] || menuRes.headers['ETag'];
  if (etag) {
    const cachedRes = http.get(`${BASE_URL}/api/v1/tenants/${TENANT_SLUG}/menu`, {
      headers: { 'If-None-Match': etag },
    });
    check(cachedRes, {
      'GET /menu with ETag returns 304': (r) => r.status === 304,
      '304 response duration < 150ms': (r) => r.timings.duration < 150,
    });
  }

  // 5. Short URL Redirection
  const redirectRes = http.get(`${BASE_URL}/r/${TENANT_SLUG}`, { redirects: 0 });
  check(redirectRes, { 'GET /r/{slug} redirects': (r) => r.status === 301 || r.status === 302 });

  // 6. Admin Storage Pre-Signed URL (POST /api/v1/admin/images/presigned-url)
  // When unauthenticated, must consistently enforce 401 Unauthorized
  const presignedRes = http.post(`${BASE_URL}/api/v1/admin/images/presigned-url`, JSON.stringify({
    fileName: 'dish-sample.jpg',
    contentType: 'image/jpeg',
  }), {
    headers: { 'Content-Type': 'application/json' },
  });
  check(presignedRes, { 'POST /presigned-url rejects unauthenticated with 401': (r) => r.status === 401 });

  // 7. Admin Tenant Creation (POST /api/v1/admin/tenants)
  const createTenantRes = http.post(`${BASE_URL}/api/v1/admin/tenants`, JSON.stringify({
    name: 'New Restaurant',
    slug: 'new-rest',
  }), {
    headers: { 'Content-Type': 'application/json' },
  });
  check(createTenantRes, { 'POST /admin/tenants rejects unauthenticated with 401': (r) => r.status === 401 });

  // 8. Admin Categories Creation (POST /api/v1/admin/categories)
  const createCatRes = http.post(`${BASE_URL}/api/v1/admin/categories`, JSON.stringify({
    name: 'Bebidas',
    displayOrder: 1,
  }), {
    headers: { 'Content-Type': 'application/json' },
  });
  check(createCatRes, { 'POST /admin/categories rejects unauthenticated with 401': (r) => r.status === 401 });

  // 9. Kitchen Availability Toggle (PATCH /api/v1/kitchen/items/{id}/availability)
  const toggleRes = http.patch(`${BASE_URL}/api/v1/kitchen/items/ddc4e69e-5e22-4e34-bb92-91a7a37ec55f/availability`, JSON.stringify({
    isAvailable: false,
  }), {
    headers: { 'Content-Type': 'application/json' },
  });
  check(toggleRes, { 'PATCH /kitchen/items/... rejects unauthenticated with 401': (r) => r.status === 401 });

  sleep(0.05);
}
