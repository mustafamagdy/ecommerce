import http from 'k6/http';
import { check, sleep, group } from 'k6';
// import { getToken } from './token.js'; // Assuming a token helper

export let options = {
  vus: 3, // Start with a lower VU count due to potentially heavy setup
  duration: '30s',
  insecureSkipTLSVerify: true,
  noConnectionReuse: false,
  thresholds: {
    http_req_duration: ['p(95)<1000'], // 95% of requests should be below 1s for reports
    'group_duration{group:::Generate Profit and Loss}': ['p(95)<800'],
    'group_duration{group:::Generate Balance Sheet}': ['p(95)<800'],
    http_req_failed: ['rate<0.02'], // Error rate should be less than 2%
    checks: ['rate>0.98'],
  },
  ext: {
    loadimpact: {
      // projectID: YOUR_PROJECT_ID,
      // name: "Accounting Financial Statement Generation"
    }
  }
};

const BASE_URL = __ENV.BASE_URL || 'https://localhost:5001/api/v1';
const TENANT_ID = __ENV.TENANT_ID || 'root';
let authToken; // To be populated by setup

// Account IDs - will be set up
let revenueAccountId;
let expenseAccountIdAsset; // For P&L
let assetAccountId;
let liabilityAccountId;
let equityAccountId; // For Balance Sheet

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
  console.log('Setting up test: Creating accounts and initial transactions...');
  const setupAuthToken = "PASTE_VALID_SETUP_TOKEN_HERE_FOR_STATEMENTS_TEST"; // Replace or use dynamic fetching
  if (setupAuthToken === "PASTE_VALID_SETUP_TOKEN_HERE_FOR_STATEMENTS_TEST") {
    console.warn("Using placeholder token for statements test setup. Real token fetching needed.");
  }
  authToken = setupAuthToken; // VUs will use this token

  const headers = {
    'Content-Type': 'application/json', 'Accept': 'application/json',
    'tenant': TENANT_ID, 'Authorization': `Bearer ${authToken}`
  };

  // Create accounts for P&L and Balance Sheet
  revenueAccountId = createAccount("LoadTest Sales Revenue", `LTRS${Date.now()}`, "Revenue", 0, headers);
  expenseAccountIdAsset = createAccount("LoadTest Rent Expense", `LTRE${Date.now()}`, "Expense", 0, headers); // Also used as an asset for some BS entries
  assetAccountId = createAccount("LoadTest Cash BS", `LTCASHBS${Date.now()}`, "Asset", 10000, headers);
  liabilityAccountId = createAccount("LoadTest Payables BS", `LTLIABBS${Date.now()}`, "Liability", 0, headers);
  equityAccountId = createAccount("LoadTest Common Stock BS", `LTEQTYBS${Date.now()}`, "Equity", 5000, headers);

  if (!revenueAccountId || !expenseAccountIdAsset || !assetAccountId || !liabilityAccountId || !equityAccountId) {
    throw new Error("Setup failed: One or more critical accounts could not be created.");
  }

  // Post a moderate number of journal entries to populate data
  const numEntriesToPost = 20; // Keep low for setup time; increase if needed for more realistic load
  console.log(`Posting ${numEntriesToPost} journal entries...`);
  for (let i = 0; i < numEntriesToPost; i++) {
    const amount = 100 + (i * 10);
    createAndPostJE(
      `Load Test JE ${i + 1}`,
      new Date(2023, 0, 15 + i).toISOString(), // Spread out dates within a month for P&L
      [
        { accountId: revenueAccountId, transactionType: "Credit", amount: amount }, // Revenue
        { accountId: assetAccountId, transactionType: "Debit", amount: amount },   // Cash for revenue

        { accountId: expenseAccountIdAsset, transactionType: "Debit", amount: amount / 2 }, // Expense
        { accountId: assetAccountId, transactionType: "Credit", amount: amount / 2 },  // Cash for expense
        
        { accountId: liabilityAccountId, transactionType: "Credit", amount: amount / 4 }, // Increase Liability
        { accountId: assetAccountId, transactionType: "Debit", amount: amount / 4 }     // Asset from Liability
      ],
      headers
    );
    if (i % 10 === 0) sleep(0.1); // Small sleep to avoid overwhelming server during setup
  }
  console.log('Setup complete for financial statements test.');
  return { token: authToken };
}

export default function(data) {
  if (!data.token) {
    console.error("Setup data (token) not available. VU cannot proceed.");
    return;
  }
  const vuToken = data.token;
  const headers = {
    'Content-Type': 'application/json', 'Accept': 'application/json',
    'tenant': TENANT_ID, 'Authorization': `Bearer ${vuToken}`
  };

  const fromDate = new Date(2023, 0, 1).toISOString(); // Jan 1, 2023
  const toDate = new Date(2023, 0, 31).toISOString();   // Jan 31, 2023 (adjust if setup dates change)
  const asOfDate = toDate;

  group("Generate Profit and Loss", function() {
    const pnlPayload = JSON.stringify({ fromDate: fromDate, toDate: toDate });
    const pnlRes = http.post(`${BASE_URL}/accounting/financial-statements/profit-and-loss`, pnlPayload, { headers: headers });
    check(pnlRes, {
      "P&L: status is 200": (r) => r.status === 200,
      "P&L: response is valid JSON": (r) => {
        try { r.json(); return true; } catch (e) { return false; }
      },
      "P&L: TotalRevenue is present": (r) => r.json("totalRevenue") !== undefined,
    });
    if (pnlRes.status !== 200) console.error(`P&L Error: ${pnlRes.status} ${pnlRes.body}`);

  });

  sleep(1); // Pause between report generation

  group("Generate Balance Sheet", function() {
    const bsPayload = JSON.stringify({ asOfDate: asOfDate });
    const bsRes = http.post(`${BASE_URL}/accounting/financial-statements/balance-sheet`, bsPayload, { headers: headers });
    check(bsRes, {
      "Balance Sheet: status is 200": (r) => r.status === 200,
      "Balance Sheet: response is valid JSON": (r) => {
        try { r.json(); return true; } catch (e) { return false; }
      },
      "Balance Sheet: TotalAssets is present": (r) => r.json("totalAssets") !== undefined,
    });
    if (bsRes.status !== 200) console.error(`Balance Sheet Error: ${bsRes.status} ${bsRes.body}`);
  });

  sleep(1);
}

export function teardown(data) {
  console.log('Financial statement test run finished. Teardown (optional cleanup)...');
  // Optional: Clean up accounts created in setup
  // This would require an auth token and careful deletion logic
  // Example:
  // if (authToken && revenueAccountId && expenseAccountIdAsset && assetAccountId && liabilityAccountId && equityAccountId) {
  //   const headers = { 'Authorization': `Bearer ${authToken}`, 'tenant': TENANT_ID };
  //   http.del(`${BASE_URL}/accounting/accounts/${revenueAccountId}`, null, { headers: headers });
  //   http.del(`${BASE_URL}/accounting/accounts/${expenseAccountIdAsset}`, null, { headers: headers });
  //   // ... and so on for other accounts
  //   console.log('Cleaned up created accounts.');
  // }
}
