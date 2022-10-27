import cors from "cors";
import { faker } from "@faker-js/faker";
import { initDB } from "../db.js";

export const doSearch = async (req, tableName) => {
  try {
    const db = initDB();
    await db.read();
    const totalCount = db.data[tableName].length;
    const pageSize = req.body.pageSize > 0 ? req.body.pageSize : totalCount;
    const totalPages = totalCount > pageSize ? Math.ceil(totalCount / pageSize) : 1;
    const currentPage = req.body.pageNumber;
    const startIndex = pageSize * (currentPage - 1);
    let arr = db.data[tableName].slice(startIndex, startIndex + pageSize);
    return {
      "data": arr,
      "currentPage": currentPage,
      "totalPages": totalPages,
      "totalCount": totalCount,
      "pageSize": pageSize,
      "hasPreviousPage": req.body.pageNumber !== 1,
      "hasNextPage": req.body.pageNumber !== totalPages
    };
  } catch (err) {
    throw err;
  }
};
export const deleteItem = async (id, tableName) => {
  try {
    const db = initDB();
    await db.read();
    const index = db.data[tableName].findIndex((x) => x.id === id);
    db.data[tableName].splice(index, 1);
    await db.write();
  } catch (err) {
    throw err;
  }
};
export const add = (app, api) => {
  return app.post(`/api/v1/${api}`, cors(), (req, res) => {
    res.send(faker.datatype.uuid());
  });
};
export const update = (app, api) => {
  return app.put(`/api/v1/${api}`, cors(), (req, res) => {
    res.send(req.params.id);
  });
};

