const express = require("express");
const cors = require("cors");
module.exports = function (app) {
    app.use(express.json());
    //app.use("/", require("./apis/index"));
    app.post("/api/v1/services/search", cors(), require("./apis/services/services-search"));
    app.delete("/api/v1/services/:id", cors(), require("./apis/services/services-delete"));
    app.post("/api/v1/services", cors(), require("./apis/services/services-add"));
    app.put("/api/v1/services", cors(), require("./apis/services/services-edit"))
};
