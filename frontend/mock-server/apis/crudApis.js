import cors from "cors";
import { addRecord, deleteRecord, doSearch, getRecord, updateRecord } from "./helpers.js";

export const crudApis = (app, api) => {
  app.post(`/api/v1/${api}/search`, cors(), async (req, res) => {
    try {
      const data = await doSearch(req, api);
      res.send(data);
    } catch (err) {
      res.status(500).send(err);
    }
  });
  app.get(`/api/v1/${api}/:id`, cors(), async (req, res) => {
    try {
      const data = await getRecord(req, api);
      res.send(data);
    } catch (err) {
      res.status(500).send(err);
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
  app.post(`/api/v1/${api}`, cors(), async (req, res) => {
    try {
      const id = addRecord(req.body, api);
      res.send(id);
    } catch (err) {
      res.status(500).send(err);
    }
  });
  app.put(`/api/v1/${api}/:id`, cors(), async (req, res) => {
    try {
      await updateRecord(req.params.id, req.body, api);
      res.send(req.params.id);
    } catch (err) {
      res.status(500).send(err);
    }
  });
};