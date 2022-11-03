import express from "express";
import { crudApis } from "./apis/crudApis.js";

export const initRoutes = (app) => {
  app.use(express.json());
  crudApis(app, "services");
  crudApis(app, "products");
  crudApis(app, "serviceCatalog");
  crudApis(app, "financialBoxes");
  crudApis(app, "customers");
  crudApis(app, "bills");
  crudApis(app, "branches");
  crudApis(app, "subscriptions");
};
