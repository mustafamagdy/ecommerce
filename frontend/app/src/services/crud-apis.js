import { ApiService } from "src/services/api-service";

export const search = (url, body) => {
    return new Promise((resolve, reject) => {
        ApiService.post(url, body)
            .then((resp) => {
                let data = resp?.data;
                if (data?.data) {
                    resolve(data);
                } else {
                    reject("Unknown record form");
                }
            })
            .catch((err) => {
                reject(err);
            });
    });
};

export const advancdSearch = (url, body) => {
    return new Promise((resolve, reject) => {
        ApiService.post(url, body)
            .then((resp) => {
                let data = resp?.data;
                if (data?.data) {
                    resolve(data);
                } else {
                    reject("Unknown record form");
                }
            })
            .catch((err) => {
                reject(err);
            });
    });
};

export const deleteRecord = (url, id) => {
    return new Promise((resolve, reject) => {
        ApiService.delete(`${url}/${id.toString()}`)
            .then((resp) => {
                resolve(resp);
            })
            .catch((err) => {
                reject(err);
            });
    });
};
export const fetchRecord = (url, id) => {
    return new Promise((resolve, reject) => {
        ApiService.get(`${url}/${id.toString()}`)
            .then((resp) => {
                resolve(resp);
            })
            .catch((err) => {
                reject(err);
            });
    });
};
export const saveRecord = (url, payload) => {
    return new Promise((resolve, reject) => {
        ApiService.post(url, payload)
            .then((resp) => {
                resolve(resp);
            })
            .catch((err) => {
                reject(err);
            });
    });
};
export const updateRecord = (url, payload) => {
    return new Promise((resolve, reject) => {
        ApiService.put(url, payload)
            .then((resp) => {
                resolve(resp);
            })
            .catch((err) => {
                reject(err);
            });
    });
};
