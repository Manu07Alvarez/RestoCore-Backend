import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '5s', target: 20 },
    { duration: '10s', target: 50 },
    { duration: '15s', target: 100 },
    { duration: '5s', target: 0 },
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500'],
    'http_req_failed': ['rate<0.01'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const TENANT_SLUG = __ENV.TENANT_SLUG || 'test-restaurant';

export default function () {
  const url = `${BASE_URL}/api/v1/tenants/${TENANT_SLUG}/menu`;
  const res = http.get(url);

  check(res, {
    'status is 200 or 404': (r) => r.status === 200 || r.status === 404,
    'server response under 500ms': (r) => r.timings.duration < 500,
  });

  sleep(0.05);
}
