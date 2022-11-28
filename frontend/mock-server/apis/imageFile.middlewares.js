import cors from "cors";
import fs from "fs";
import { faker } from "@faker-js/faker";

export const imageFileMiddlewares = (app, api, imageUrlProp, imageFileProp) => {
  app.post(`/api/v1/${api}`, cors(), async (req, res, next) => {
    try {
      const imgFile = req.body[imageFileProp];
      if (imgFile) {
        const data = imgFile.data.replace(/^data:image\/png;base64,/, "");
        const imgName = "files" + faker.datatype.uuid();
        fs.writeFile(imgName, data, () => {
          req.body[imageUrlProp] = imgName;
          delete req.body[imageFileProp];
          next();
        });
      }
    } catch (err) {
      res.status(500).send(err);
    }
  });
  app.put(`/api/v1/${api}/:id`, cors(), async (req, res, next) => {
    try {

      next();
    } catch (err) {
      res.status(500).send(err);
    }
  });
};