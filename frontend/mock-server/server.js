const express = require("express");
const cors = require("cors");
const app = express();

app.options("*", cors());
require("./routes")(app);
const port = 3000;

app.listen(port, () => console.log(`Listening on port ${port}...`));
