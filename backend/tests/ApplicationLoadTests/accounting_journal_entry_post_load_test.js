import http from 'k6/http';
import { check, sleep, group } from 'k6';
// import { getToken } from './token.js'; // Assuming a token helper

// Options for the test run
export let options = {
  vus: 5, // Start with 5 Virtual Users
  duration: '30s', // Run for 30 seconds
  insecureSkipTLSVerify: true, // If using self-signed certs locally
  noConnectionReuse: false,
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    'group_duration{group:::Create Journal Entry}': ['p(95)<300'],
    'group_duration{group:::Post Journal Entry}': ['p(95)<200'],
    http_req_failed: ['rate<0.01'], // Error rate should be less than 1%
    checks: ['rate>0.99'], // Success rate of checks should be higher than 99%
  },
  ext: {
    loadimpact: {
      // projectID: YOUR_PROJECT_ID, // Optional: If using k6 Cloud
      // name: "Accounting Journal Entry Posting"
    }
  }
};

// Environment variables or defaults
const BASE_URL = __ENV.BASE_URL || 'https://localhost:5001/api/v1'; // Adjust port if needed
const TENANT_ID = __ENV.TENANT_ID || 'root'; // Default to 'root' tenant

// Placeholder for dynamically fetched token
let authToken; 

// Accounts to be used by VUs - these will be set up in the setup function
let debitAccountId;
let creditAccountId;

// Setup function: runs once before the test VUs start
export function setup() {
  console.log('Setting up test: Creating accounts...');
  
  // Use a real token for setup - this part needs actual token fetching logic
  // For now, using a placeholder. In real tests, call an auth endpoint.
  const setupAuthToken = "PASTE_VALID_SETUP_TOKEN_HERE"; // Replace with a real token or dynamic fetching for setup

  if (setupAuthToken === "PASTE_VALID_SETUP_TOKEN_HERE") {
    console.warn("Using placeholder token for setup. Real token fetching needed for actual test runs.");
  }
  
  const headers = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    'tenant': TENANT_ID,
    'Authorization': `Bearer ${setupAuthToken}`
  };

  // Create Debit Account
  let payloadDebit = JSON.stringify({
    accountName: `LoadTest DebitAcc ${Date.now()}`, // Unique name
    accountNumber: `LTDEBIT${Date.now()}`,
    accountType: "Asset",
    initialBalance: 1000000
  });
  let resDebit = http.post(`${BASE_URL}/accounting/accounts`, payloadDebit, { headers: headers });
  if (resDebit.status !== 200) {
    console.error(`Failed to create debit account: ${resDebit.status} ${resDebit.body}`);
    throw new Error('Setup failed: Could not create debit account.');
  }
  debitAccountId = resDebit.json("id"); // k6 json extractor

  // Create Credit Account
  let payloadCredit = JSON.stringify({
    accountName: `LoadTest CreditAcc ${Date.now()}`, // Unique name
    accountNumber: `LTCREDIT${Date.now()}`,
    accountType: "Revenue",
    initialBalance: 0
  });
  let resCredit = http.post(`${BASE_URL}/accounting/accounts`, payloadCredit, { headers: headers });
   if (resCredit.status !== 200) {
    console.error(`Failed to create credit account: ${resCredit.status} ${resCredit.body}`);
    throw new Error('Setup failed: Could not create credit account.');
  }
  creditAccountId = resCredit.json("id");

  console.log(`Setup complete: Debit Account ID: ${debitAccountId}, Credit Account ID: ${creditAccountId}`);
  
  // Fetch a token to be used by VUs - replace with actual token logic
  // authToken = getToken(); // Example if using token.js
  authToken = setupAuthToken; // For this example, reuse the setup token. VUs should ideally get their own.
  if (!authToken) {
    throw new Error('Authentication token not obtained in setup.');
  }

  return { debitAccId: debitAccountId, creditAccId: creditAccountId, token: authToken };
}

// Default function: main logic for each VU
export default function(data) {
  if (!data.token || !data.debitAccId || !data.creditAccId) {
    console.error("Setup data not available. VU cannot proceed.");
    return;
  }
  
  const vuToken = data.token; // Use token from setup or fetch new one
  const vuDebitAcc = data.debitAccId;
  const vuCreditAcc = data.creditAccId;

  const headers = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    'tenant': TENANT_ID,
    'Authorization': `Bearer ${vuToken}`
  };

  group("Create Journal Entry", function() {
    const createPayload = JSON.stringify({
      entryDate: new Date().toISOString(),
      description: `Load test JE - VU: ${__VU} Iter: ${__ITER}`,
      transactions: [
        { accountId: vuDebitAcc, transactionType: "Debit", amount: 10.50 + (__VU * 0.1) + (__ITER * 0.01) },
        { accountId: vuCreditAcc, transactionType: "Credit", amount: 10.50 + (__VU * 0.1) + (__ITER * 0.01) }
      ]
    });

    const createRes = http.post(`${BASE_URL}/accounting/journal-entries`, createPayload, { headers: headers });
    
    check(createRes, {
      "Create JE: status is 200": (r) => r.status === 200,
      "Create JE: response body contains ID": (r) => r.json("id") !== undefined && r.json("id") !== null,
    });

    const journalEntryId = createRes.json("id");

    if (createRes.status === 200 && journalEntryId) {
      group("Post Journal Entry", function() {
        const postRes = http.post(`${BASE_URL}/accounting/journal-entries/${journalEntryId}/post`, null, { headers: headers });
        check(postRes, {
          "Post JE: status is 200": (r) => r.status === 200,
        });
      });
    } else {
        console.error(`VU ${__VU} Iter ${__ITER}: Failed to create JE or extract ID. Status: ${createRes.status}, Body: ${createRes.body}`);
    }
  });

  sleep(1); // Sleep for 1 second between iterations
}

// Teardown function: runs once after all VUs have finished
export function teardown(data) {
  console.log('Tearing down test: Deleting created accounts (optional)...');
  // Optionally, delete the accounts created in setup
  // This would require another HTTP call with appropriate auth
  // For simplicity in this example, teardown is minimal.
  // Example:
  // if (data.debitAccId && data.creditAccId && data.token) {
  //   const headers = { 'Authorization': `Bearer ${data.token}`, 'tenant': TENANT_ID };
  //   http.del(`${BASE_URL}/accounting/accounts/${data.debitAccId}`, null, { headers: headers });
  //   http.del(`${BASE_URL}/accounting/accounts/${data.creditAccId}`, null, { headers: headers });
  //   console.log(`Deleted accounts: ${data.debitAccId}, ${data.creditAccId}`);
  // }
  console.log('Test run finished.');
}
