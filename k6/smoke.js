import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 10,             // 10 virtual users
  duration: '15s',     // for 15 seconds
};

export default function () {
  const res = http.get('http://localhost:8080/api/health');
  check(res, { 'status == 200': r => r.status === 200 });
  sleep(1);
}