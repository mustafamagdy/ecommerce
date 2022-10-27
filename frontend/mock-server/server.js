import express from "express";
import cors from "cors";
import { initDB } from "./db.js";
import { initRoutes } from "./routes.js";

const app = express();

app.options("*", cors());
app.use(function(req, res, next) {
  res.header("Access-Control-Allow-Origin", "*");
  res.header("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE");
  res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization");
  next();
});

initRoutes(app);
const port = 3000;

app.listen(port, () => console.log(`Listening on port ${port}...`));
