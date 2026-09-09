import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '5s', target: 20 },
    { duration: '20s', target: 50 },
    { duration: '5s', target: 0 },
  ],
  thresholds: {
    'http_req_duration{status:304}': ['p(95)<150'],
    'http_req_failed': ['rate<0.01'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const TENANT_SLUG = __ENV.TENANT_SLUG || 'test-restaurant';

export default function () {
  const url = `${BASE_URL}/api/v1/tenants/${TENANT_SLUG}/menu`;

  // First request to obtain initial ETag
  const initialRes = http.get(url);
  const etag = initialRes.headers['Etag'] || initialRes.headers['ETag'];

  if (etag) {
    const cachedRes = http.get(url, {
      headers: {
        'If-None-Match': etag,
      },
    });

    check(cachedRes, {
      'status is 304 Not Modified': (r) => r.status === 304,
      'cached response duration under 150ms': (r) => r.timings.duration < 150,
    });
  }

  sleep(0.05);
}
