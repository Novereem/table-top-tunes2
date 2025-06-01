import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
export const options = {
  vus: 10,             // 10 virtual users
  duration: '15s',     // for 15 seconds
};

export default function () {
  const res = http.get(`${BASE_URL}/health`);
  check(res, { 'status == 200': r => r.status === 200 });
  sleep(1);
}