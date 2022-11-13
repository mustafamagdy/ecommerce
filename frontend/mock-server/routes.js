import express from "express";
import { crudApis } from "./apis/crudApis.js";
import { billsApis } from "./apis/billsApis.js";
export const initRoutes = (app) => {
  app.use(express.json());
  crudApis(app, "services");
  crudApis(app, "products");
  crudApis(app, "serviceCatalog");
  crudApis(app, "financialBoxes");
  crudApis(app, "customers");
  billsApis(app, "bills");
  crudApis(app, "branches");
  crudApis(app, "subscriptions");
  crudApis(app, "financialBoxesTransactions");
  crudApis(app, "subscriptionAccountStatement");
  crudApis(app, "roles");
  crudApis(app, "users");
};
