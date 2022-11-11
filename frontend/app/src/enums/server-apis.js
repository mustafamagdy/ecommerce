const baseURL = process.env.API_PATH;
export const serverApis = {
    services: `${baseURL}/services`,
    financialBoxes: `${baseURL}/financialBoxes`,
    customers: `${baseURL}/customers`,
    bills: `${baseURL}/bills`,
    branches: `${baseURL}/branches`,
    subscriptions: `${baseURL}/subscriptions`,
    financialBoxesTransactions: `${baseURL}/financialBoxesTransactions`,
};
