import axios, {AxiosInstance, AxiosRequestConfig, AxiosRequestHeaders} from 'axios'
import authConfig from 'src/configs/auth'
import storage from './storage'
import {isAnonymous} from './endpoints'

const BASE_URL = (process.env.BASE_URL ?? "https://localhost:5001/api").replace(/\/+$/, "")

class HttpService {

  private _axios: AxiosInstance;

  private constructor() {
    this._axios = axios.create({
      baseURL: BASE_URL,
    });

    this._axios.interceptors.request.use(this.defaultHeaders)
    this._axios.interceptors.request.use(this.authorization)
  }

  private static instance: HttpService;

  public static get Instance(): HttpService {
    if (!HttpService.instance) {
      HttpService.instance = new HttpService();
    }

    return HttpService.instance;
  }

  private defaultHeaders = (config: AxiosRequestConfig) => {
    if (config.headers)
      config.headers ['tenant'] = 'root';
    return config;
  }

  private authorization = (config: AxiosRequestConfig) => {
    const path = config.url!;

    if (isAnonymous(path))
      return config;

    if (config.headers)
      config.headers ['Authorization'] = `Bearer ${storage.getItem(authConfig.storageTokenKeyName)!}`;
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

  async put(path: string, body: object) {
    try {
      const response = await this._axios.put(path, body);
      if (response) {
        return response.data;
      }
    } catch (e) {
      this.handleErrors(e);
      return Promise.reject(e);
    }
  }

  handleErrors(error: any) {
    console.error('API ERROR => ' + error.message);
  }
}


export const Http = HttpService.Instance;
