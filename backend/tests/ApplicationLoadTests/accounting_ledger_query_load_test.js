import http from 'k6/http';
import { check, sleep, group } from 'k6';
// import { getToken } from './token.js'; // Assuming a token helper

export let options = {
  vus: 5, // Start with 5 VUs
  duration: '1m', // Run for 1 minute
  insecureSkipTLSVerify: true,
  noConnectionReuse: false,
  thresholds: {
    http_req_duration: ['p(95)<750'], // 95% of requests should be below 750ms for ledger queries
    'group_duration{group:::Get Account Ledger}': ['p(95)<600'],
    http_req_failed: ['rate<0.01'], // Error rate should be less than 1%
    checks: ['rate>0.99'],
  },
  ext: {
    loadimpact: {
      // projectID: YOUR_PROJECT_ID,
      // name: "Accounting Ledger Query"
    }
  }
};

const BASE_URL = __ENV.BASE_URL || 'https://localhost:5001/api/v1';
const TENANT_ID = __ENV.TENANT_ID || 'root';
let authToken; // To be populated by setup

let primaryAccountId; // Account for ledger queries
const NUM_TRANSACTION_PAIRS_TO_CREATE = 100; // Creates 200 transactions affecting the primary account

// Helper function to create an account (simplified for setup)
function createAccount(name, number, type, initialBalance, headers) {
  const payload = JSON.stringify({ accountName: name, accountNumber: number, accountType: type, initialBalance: initialBalance });
  const res = http.post(`${BASE_URL}/accounting/accounts`, payload, { headers: headers });
  if (res.status !== 200) {
    console.error(`Setup: Failed to create account ${name}: ${res.status} ${res.body}`);
    return null;
  }
  return res.json("id");
}

// Helper function to create and post a JE (simplified for setup)
function createAndPostJE(description, date, transactions, headers) {
  const jePayload = JSON.stringify({ entryDate: date, description: description, transactions: transactions });
  const jeRes = http.post(`${BASE_URL}/accounting/journal-entries`, jePayload, { headers: headers });
  if (jeRes.status !== 200 || !jeRes.json("id")) {
    console.error(`Setup: Failed to create JE "${description}": ${jeRes.status} ${jeRes.body}`);
    return null;
  }
  const jeId = jeRes.json("id");
  const postRes = http.post(`${BASE_URL}/accounting/journal-entries/${jeId}/post`, null, { headers: headers });
  if (postRes.status !== 200) {
    console.error(`Setup: Failed to post JE "${description}" (ID: ${jeId}): ${postRes.status} ${postRes.body}`);
    return null;
  }
  return jeId;
}

export function setup() {
  console.log('Setting up test: Creating primary account and posting numerous transactions...');
  const setupAuthToken = "PASTE_VALID_SETUP_TOKEN_HERE_FOR_LEDGER_TEST"; // Replace or use dynamic fetching
  if (setupAuthToken === "PASTE_VALID_SETUP_TOKEN_HERE_FOR_LEDGER_TEST") {
    console.warn("Using placeholder token for ledger test setup. Real token fetching needed.");
  }
  authToken = setupAuthToken;

  const headers = {
    'Content-Type': 'application/json', 'Accept': 'application/json',
    'tenant': TENANT_ID, 'Authorization': `Bearer ${authToken}`
  };

  primaryAccountId = createAccount("Main Cash Ledger Account", `CASHLEDG${Date.now()}`, "Asset", 100000, headers);
  if (!primaryAccountId) {
    throw new Error("Setup failed: Could not create primary account.");
  }

  // Create a contra-account
  const contraAccountId = createAccount("Contra Ledger Account", `CONTRALEDG${Date.now()}`, "Expense", 0, headers);
  if (!contraAccountId) {
    throw new Error("Setup failed: Could not create contra account.");
  }

  console.log(`Posting ${NUM_TRANSACTION_PAIRS_TO_CREATE * 2} transactions for account ${primaryAccountId}...`);
  const baseDate = new Date(2023, 0, 1); // Start from Jan 1, 2023

  for (let i = 0; i < NUM_TRANSACTION_PAIRS_TO_CREATE; i++) {
    const transactionDate = new Date(baseDate);
    transactionDate.setDate(baseDate.getDate() + i); // Spread transactions over time

    const amount = 50 + (i % 10); // Vary amounts slightly
    let transactions;
    if (i % 2 === 0) { // Alternate between debit and credit for variety
        transactions = [
            { accountId: primaryAccountId, transactionType: "Debit", amount: amount },
            { accountId: contraAccountId, transactionType: "Credit", amount: amount }
        ];
    } else {
        transactions = [
            { accountId: primaryAccountId, transactionType: "Credit", amount: amount },
            { accountId: contraAccountId, transactionType: "Debit", amount: amount }
        ];
    }
    
    createAndPostJE(
      `Ledger Load Test JE ${i + 1}`,
      transactionDate.toISOString(),
      transactions,
      headers
    );
    if (i % 20 === 0) sleep(0.1); // Small sleep during setup
  }

  console.log('Setup complete for ledger query test.');
  return { token: authToken, mainAccountId: primaryAccountId, startDate: baseDate.toISOString() };
}

export default function(data) {
  if (!data.token || !data.mainAccountId) {
    console.error("Setup data not available. VU cannot proceed.");
    return;
  }
  
  const vuToken = data.token;
  const targetAccountId = data.mainAccountId;
  
  const headers = {
    'Content-Type': 'application/json', 'Accept': 'application/json',
    'tenant': TENANT_ID, 'Authorization': `Bearer ${vuToken}`
  };

  // Define a date range for queries. Could be dynamic or fixed.
  // For this example, query for a month that includes all setup transactions.
  const fromDate = data.startDate; // Jan 1, 2023
  const toDate = new Date(new Date(data.startDate).setDate(new Date(data.startDate).getDate() + NUM_TRANSACTION_PAIRS_TO_CREATE + 5)).toISOString();


  group("Get Account Ledger", function() {
    const ledgerPayload = JSON.stringify({
      accountId: targetAccountId,
      fromDate: fromDate,
      toDate: toDate
    });

    const ledgerRes = http.post(`${BASE_URL}/accounting/ledgers/account-statement`, ledgerPayload, { headers: headers });
    
    check(ledgerRes, {
      "Ledger Query: status is 200": (r) => r.status === 200,
      "Ledger Query: response is valid JSON": (r) => {
        try { r.json(); return true; } catch (e) { return false; }
      },
      "Ledger Query: has 'entries' array": (r) => r.json("entries") !== undefined && Array.isArray(r.json("entries")),
      "Ledger Query: has 'openingBalance'": (r) => r.json("openingBalance") !== undefined,
      "Ledger Query: has 'closingBalance'": (r) => r.json("closingBalance") !== undefined,
    });

    if (ledgerRes.status !== 200) {
      console.error(`Ledger Query Error - VU ${__VU} Iter ${__ITER}: ${ledgerRes.status} ${ledgerRes.body}`);
    }
  });

  sleep(1 + Math.random()); // Sleep for 1-2 seconds between iterations
}

export function teardown(data) {
  console.log('Ledger query test run finished. Teardown (optional cleanup)...');
  // Optional: Clean up accounts created in setup
  // if (data.token && data.mainAccountId) {
  //   const headers = { 'Authorization': `Bearer ${data.token}`, 'tenant': TENANT_ID };
  //   // Also delete contra-account if its ID was returned from setup
  //   // http.del(`${BASE_URL}/accounting/accounts/${data.mainAccountId}`, null, { headers: headers });
  //   // console.log(`Deleted primary account: ${data.mainAccountId}`);
  // }
}
