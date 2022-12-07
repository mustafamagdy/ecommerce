const baseURL = process.env.API_PATH;
export const serverApis = {
    filesBaseURL: `${baseURL}/`,
    services: `${baseURL}/services`,
    financialBoxes: `${baseURL}/financialBoxes`,
    customers: `${baseURL}/customers`,
    bills: `${baseURL}/bills`,
    branches: `${baseURL}/branches`,
    subscriptions: `${baseURL}/subscriptions`,
    financialBoxesTransactions: `${baseURL}/financialBoxesTransactions`,
    subscriptionAccountStatement: `${baseURL}/subscriptionAccountStatement`,
    roles: `${baseURL}/roles`,
    users: `${baseURL}/users`,
    serviceCatalog: `${baseURL}/serviceCatalog`,
    pointOfSalesList: `${baseURL}/pointOfSalesList`,
    pointOfSalesSessions: `${baseURL}/pointOfSalesSessions`,
};
