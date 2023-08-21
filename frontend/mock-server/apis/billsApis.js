import cors from "cors";
import { faker } from "@faker-js/faker";
import { addRecord, deleteRecord, doSearch, updateRecord } from "./helpers.js";

export const billsApis = (app, api) => {
  app.post(`/api/v1/${api}/search`, cors(), async (req, res) => {
    try {
      const data = await doSearch(req, api);
      data.totals = {
        total: "50000",
        paid: "20000",
        rest: "30000",
        // total: data.reduce(
        //   (accumulator, current) => accumulator + current.total,
        //   0
        // ),
        // paid: data.reduce(
        //   (accumulator, current) => accumulator + current.paid,
        //   0
        // ),
        // rest: data.reduce(
        //   (accumulator, current) => accumulator + current.rest,
        //   0
        // ),
      };
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
      await deleteRecord(req.params.id, api);
      res.send(req.params.id);
    } catch (err) {
      res.status(500).send(err);
    }
  });
};
