import cors from "cors";
import { faker } from "@faker-js/faker";
import { initDB } from "../db.js";

export const doSearch = async (req, tableName) => {
  try {
    const db = initDB();
    await db.read();
    const totalCount = db.data[tableName].length;
    const pageSize = req.body.pageSize > 0 ? req.body.pageSize : totalCount;
    const totalPages =
      totalCount > pageSize ? Math.ceil(totalCount / pageSize) : 1;
    const currentPage = req.body.pageNumber;
    const startIndex = pageSize * (currentPage - 1);
    let arr = db.data[tableName].slice(startIndex, startIndex + pageSize);
    return {
      data: arr,
      currentPage: currentPage,
      totalPages: totalPages,
      totalCount: totalCount,
      pageSize: pageSize,
      hasPreviousPage: req.body.pageNumber !== 1,
      hasNextPage: req.body.pageNumber !== totalPages
    };
  } catch (err) {
    throw err;
  }
};
export const getRecord = async (req, tableName) => {
  try {
    const db = initDB();
    await db.read();
    const id = req.params.id;
    const rec = db.data[tableName].find(x => x.id === id);
    if (rec && rec.id) {
      return rec;
    } else {
      throw new Error("record not found");
    }
  } catch (err) {
    throw err;
  }
};
export const deleteRecord = async (id, tableName) => {
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
export const addRecord = async (data, tableName) => {
  try {
    const db = initDB();
    await db.read();
    data.id = faker.datatype.uuid();
    db.data[tableName].push(data);
    await db.write();
    return data.id;
  } catch (err) {
    throw err;
  }
};
export const updateRecord = async (id, data, tableName) => {
  try {
    const db = initDB();
    await db.read();
    const index = db.data[tableName].findIndex(x => x.id === id);
    if (index !== -1) {
      Object.assign(db.data[tableName][index], data);
    } else {
      throw new Error("no record found");
    }
    await db.write();
    return data.id;
  } catch (err) {
    throw err;
  }
};
