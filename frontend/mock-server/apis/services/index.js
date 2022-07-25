const cors = require("cors");
module.exports = function (app, api) {
    app.post(`/api/v1/${api}/search`, cors(), require("./services-search"));
    app.get(`/api/v1/${api}/:id`, cors(), require("./services-fetchRecord"))
    app.delete(`/api/v1/${api}/:id`, cors(), require("./services-delete"));
    app.post(`/api/v1/${api}`, cors(), require("./services-add"));
    app.put(`/api/v1/${api}`, cors(), require("./services-update"));
};
