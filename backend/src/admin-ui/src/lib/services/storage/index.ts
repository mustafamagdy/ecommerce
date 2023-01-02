import {InMemoryStorage} from "./inMemoory";
import {Storage} from './interface'

const inMemoryStorage = new InMemoryStorage();

const localStorage = (): Storage => {
  if (typeof window === "object" && window.localStorage) {
    return window.localStorage;
  }

  return inMemoryStorage;
}

export default localStorage();
