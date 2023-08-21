import { JSONFile, Low } from "lowdb";

export const initDB = () => {
  const file = "./db.json";
  const adapter = new JSONFile(file);
  return new Low(adapter);
};
