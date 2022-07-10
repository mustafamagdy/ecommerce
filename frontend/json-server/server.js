const jsonServer = require("json-server");
const server = jsonServer.create();
const router = jsonServer.router("db.json");
const middlewares = jsonServer.defaults();

// Set default middlewares (logger, static, cors and no-cache)
server.use(middlewares);

// To handle POST, PUT and PATCH you need to use a body-parser
// You can use the one used by JSON Server
server.use(jsonServer.bodyParser);
server.use((req, res, next) => {
    if (req.method === "POST") {
        req.body.createdAt = Date.now();
    }
    // Continue to JSON Server router
    next();
});

router.render = (req, res) => {
    if (req.method === "GET" && Array.isArray(res.locals.data)) {
        res.jsonp({
            records: res.locals.data,
            total_records: res.locals.data.length,
        });
    }
};

// Use default router
server.use(router);
server.listen(3000, () => {
    console.log("JSON Server is running");
});
