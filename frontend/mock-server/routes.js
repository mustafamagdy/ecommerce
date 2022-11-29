import express from "express";
import { crudApis } from "./apis/crudApis.js";
import { billsApis } from "./apis/billsApis.js";
import { imageFileMiddlewares } from "./apis/imageFile.middlewares.js";

export const initRoutes = (app) => {
  app.use(express.json());
  imageFileMiddlewares(app, "services", "imageUrl", "imageFile");
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
  crudApis(app, "serviceCatalog");
};
