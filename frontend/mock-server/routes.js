const express = require("express");
module.exports = function (app) {
    app.use(express.json());
    require("./apis/services")(app, "services");
};
