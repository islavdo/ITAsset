import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '1m', target: 30 },
    { duration: '30s', target: 0 }
  ],
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<800']
  }
};

export default function () {
  const login = http.post('http://localhost:5100/identity/auth/login', JSON.stringify({ email: 'admin@it.local', password: 'Admin123!' }), { headers: { 'Content-Type': 'application/json' } });
  check(login, { 'login ok': r => r.status === 200 });
  const token = login.json('accessToken');
  const res = http.get('http://localhost:5100/equipment/equipment?page=1&pageSize=20', { headers: { Authorization: `Bearer ${token}` } });
  check(res, { 'equipment ok': r => r.status === 200 });
  sleep(1);
}
