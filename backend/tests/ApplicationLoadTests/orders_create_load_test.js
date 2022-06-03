import http from 'k6/http';

export let options = {
  insecureSkipTLSerify: true,
  noConnectionReuse: false,
  stages: [
    {duration: '5m', target: 50},
    {duration: '10m', target: 50},
    {duration: '5m', target: 0},
  ],
  thresholds: {
    http_req_duration: ['p(99)<1000'], //99% of requests need to finish under 1s
  }
}

const BASE_URL = 'https://localhost:5001';

export default () => {
  const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwZDQ3NDRmLTY4ZjQtNGQ4My1hNzIyLTE2NTcwYTI0YTM5MSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQHJvb3QuY29tIiwiZnVsbE5hbWUiOiJyb290IEFkbWluIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InJvb3QiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zdXJuYW1lIjoiQWRtaW4iLCJpcEFkZHJlc3MiOiIwLjAuMC4xIiwidGVuYW50Ijoicm9vdCIsImltYWdlX3VybCI6IiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL21vYmlsZXBob25lIjoiIiwiZXhwIjoxNjU0MjU5ODUxfQ.mzrOScI5yYakfFTU--Swsxf1gUkWSaSpXjhnlwNRh2A';
  const url = `${BASE_URL}/api/v1/orders/cash`;
  const payload = JSON.stringify({
    items: [
      {
        itemId: "08da4540-2942-48b7-88f1-ef72103f6183",
        qty: 10
      }
    ]
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'tenant': 'root',
      'Authorization': `Bearer ${token}`
    },
  };

  http.post(url, payload, params);
}