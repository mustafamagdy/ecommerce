import express from "express";
import { crudApis } from "./apis/crudApis.js";

export const initRoutes = (app) => {
  app.use(express.json());
  crudApis(app, "services");
  crudApis(app, "products");
  crudApis(app, "serviceCatalog");
};
