import axios from "axios";
import { StorageService } from "./storage";
import { useApp } from "src/composables/app";

axios.defaults.baseURL = process.env.API_PATH;
//axios.defaults.headers.post['Content-Type']  = 'application/x-www-form-urlencoded';

axios.defaults.headers.post["Content-Type"] = "application/json";

// If token exists set header
let token = StorageService.getToken();
if (token) {
    axios.defaults.headers.common["Authorization"] = `Bearer ${token}`;
}
axios.interceptors.request.use(req => {
    console.log("Starting Request", req);
    return req;
});
axios.interceptors.response.use(res => {
    console.log("response", res);
    return res;
}, err => {
    app.showPageRequestError(err);
});
export const ApiService = {
    removeHeader() {
        axios.defaults.headers.common = {};
    },
    get(apiPath) {
        return axios.get(apiPath);
    },

    download(apiPath) {
        return axios({
            url: apiPath,
            method: "GET",
            responseType: "blob" // important
        });
    },
    post(apiPath, data) {
        return axios.post(apiPath, data);
    },

    put(apiPath, data) {
        return axios.put(apiPath, data);
    },

    delete(apiPath) {
        return axios.delete(apiPath);
    },

    /**
     * Perform a custom Axios request.
     *
     * data is an object containing the following properties:
     *  - method
     *  - url
     *  - data ... request payload
     *  - auth (optional)
     *    - username
     *    - password
     **/
    customRequest(data) {
        return axios(data);
    },
    axios() {
        return axios;
    }
};
