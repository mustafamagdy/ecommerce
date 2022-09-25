import axios, {AxiosInstance, AxiosRequestConfig, AxiosRequestHeaders} from 'axios'
import {endPoints} from "./endpoints";
import authConfig from 'src/configs/auth'

const BASE_URL = (process.env.BASE_URL ?? "https://localhost:5001").replace(/\/+$/, "");

export class Http {


  private _axios: AxiosInstance;

  constructor() {
    this._axios = axios.create({
      baseURL: BASE_URL,
    });

    this._axios.interceptors.request.use(this.defaultHeaders)
    this._axios.interceptors.request.use(this.authorization)
  }

  private defaultHeaders = (config: AxiosRequestConfig) => {
    if (config.headers)
      config.headers ['tenant'] = 'root';
    return config;
  }

  private authorization = (config: AxiosRequestConfig) => {
    const path = config.url!;
    if (endPoints[path].anonymous)
      return config;

    if (config.headers)
      config.headers ['Authorization'] = window.localStorage.getItem(authConfig.storageTokenKeyName)!;
    return config;

  }

  async get(path: string) {
    try {
      const response = await this._axios.get(path);
      if (response) {
        return response.data;
      }
    } catch (e) {
      this.handleErrors(e);
      return Promise.reject(e);
    }
  }

  async post<T>(path: string, body: T) {
    try {
      const response = await this._axios.post(path, body);
      if (response) {
        return response.data;
      }
    } catch (e) {
      this.handleErrors(e);
      return Promise.reject(e);
    }
  }

  async delete(path: string) {
    try {
      const response = await this._axios.delete(path);
      if (response) {
        return response.data;
      }
    } catch (e) {
      this.handleErrors(e);
      return Promise.reject(e);
    }
  }

  async put(path: string) {
    try {
      const response = await this._axios.put(path);
      if (response) {
        return response.data;
      }
    } catch (e) {
      this.handleErrors(e);
      return Promise.reject(e);
    }
  }

  handleErrors(error: any) {

  }


}


