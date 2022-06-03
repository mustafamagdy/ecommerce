import http from 'k6/http';


export let options = {
  insecureSkipTLSerify: true,
  noConnectionReuse: false,
  vus: 1,
  duration: '1s'
}

export default () => {
  const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwZDQ3NDRmLTY4ZjQtNGQ4My1hNzIyLTE2NTcwYTI0YTM5MSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQHJvb3QuY29tIiwiZnVsbE5hbWUiOiJyb290IEFkbWluIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InJvb3QiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zdXJuYW1lIjoiQWRtaW4iLCJpcEFkZHJlc3MiOiIwLjAuMC4xIiwidGVuYW50Ijoicm9vdCIsImltYWdlX3VybCI6IiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL21vYmlsZXBob25lIjoiIiwiZXhwIjoxNjU0MjU5MzAzfQ.SfH7qd4YDfIR5n82Be4kE1CEi8P-wRXfoO2dp4bvW0';
  const url = 'https://localhost:5001/api/v1/orders/search';
  const payload = JSON.stringify({});

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