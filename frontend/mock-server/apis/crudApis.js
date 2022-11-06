import cors from "cors";
import { faker } from "@faker-js/faker";
import { add, deleteItem, doSearch, update } from "./helpers.js";

export const crudApis = (app, api) => {
  app.post(`/api/v1/${api}/search`, cors(), async (req, res) => {
    try {
      const data = await doSearch(req, api);
      res.send(data);
    } catch (err) {
      res.status(500).send(err);
    }
  });
  app.get(`/api/v1/${api}/:id`, cors(), (req, res) => {
    const id = req.params.id;
    if (id && id !== "") {
      res.send({
        id: id,
        name: faker.fake(`اسم الخدمة {{random.numeric(3)}}`),
        description: faker.lorem.words(20),
        imageUrl: faker.image.abstract(300, 300),
      });
    }
  });
  app.delete(`/api/v1/${api}/:id`, cors(), async (req, res) => {
    try {
      await deleteItem(req.params.id, api);
      res.send(req.params.id);
    } catch (err) {
      res.status(500).send(err);
    }
  });
  add(app, api);
  update(app, api);
};
