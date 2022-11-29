import { ApiService } from "src/services/api-service";

export const crud = {
    namespaced: true,
    state: () => ({
        currentRecord: null,
        records: [],
        requestError: null,
        showAddRecord: false,
        showEditRecord: false,
        editRecordId: null,
        clickedRecordId: null,
    }),
    getters: {
        records(state) {
            return state.records;
        },
        currentRecord(state) {
            return state.currentRecord;
        },
        showAddRecord(state) {
            return state.showAddRecord;
        },
        showEditRecord(state) {
            return state.showEditRecord;
        },
        editRecordId(state) {
            return state.editRecordId;
        },
        clickedRecordId(state) {
            return state.clickedRecordId;
        },
    },
    mutations: {
        setRecords(state, records) {
            state.records = records;
        },
        setCurrentRecord(state, record) {
            state.currentRecord = record;
        },
        mergeRecords(state, records) {
            state.records = state.records.concat(records);
        },
        addRecord(state, record) {
            if (Array.isArray(record)) {
                for (let index = 0; index < record.length; index++) {
                    state.records.unshift(record[index]);
                }
            } else {
                state.records.unshift(record);
                state.currentRecord = record;
            }
        },
        updateRecord(state, record) {
            const index = state.records.findIndex(
                (item) => item["id"] == record["id"]
            );
            if (index !== -1) {
                Object.assign(state.records[index], record);
            }
        },
        deleteRecord(state, id) {
            if (Array.isArray(id)) {
                id.forEach((itemId) => {
                    let itemIndex = state.records.findIndex(
                        (item) => item["id"] === itemId
                    );
                    if (itemIndex !== -1) {
                        state.records.splice(itemIndex, 1);
                    }
                });
            } else {
                let itemIndex = state.records.findIndex(
                    (item) => item["id"] === id
                );
                if (itemIndex !== -1) {
                    state.records.splice(itemIndex, 1);
                }
            }
        },
        setError(state, error) {
            state.requestError = error;
        },
        setShowAddRecord(state, value) {
            state.showAddRecord = value;
            if (value === true) {
                state.showEditRecord = false;
                state.editRecordId = null;
            }
        },
        setShowEditRecord(state, value) {
            let { show = false, id = null } = value;
            if (!show && value === true) show = true;
            if (!show) id = null;
            state.showEditRecord = show;
            state.editRecordId = id;
            if (show === true) state.showAddRecord = false;
        },
        setEditRecordId(state, value) {
            state.eitRecordId = value;
            if (!value || value === "" || value === 0) {
                state.showEditRecord = false;
            }
        },
        setClickedRecordId(state, value) {
            state.clickedRecordId = value;
        },
    },
    actions: {
        search: ({ commit }, payload) => {
            return new Promise((resolve, reject) => {
                let url = payload.url;
                let body = payload.criteria;
                let merge = payload.merge;
                ApiService.post(url, body)
                    .then((resp) => {
                        let data = resp?.data;
                        if (data?.data) {
                            let records = data.data;
                            if (merge) {
                                commit("mergeRecords", records);
                            } else {
                                commit("setRecords", records);
                            }
                            resolve(data);
                        } else {
                            // if json data received does not have record object
                            // or is invalid
                            reject("Unknown record form");
                        }
                    })
                    .catch((err) => {
                        reject(err);
                    });
            });
        },
        fetchRecord: ({ commit }, data) => {
            let url = data.url;
            let id = data.id;
            return new Promise((resolve, reject) => {
                ApiService.get(`${url}/${id.toString()}`)
                    .then((resp) => {
                        commit("setCurrentRecord", resp.data);
                        resolve(resp);
                    })
                    .catch((err) => {
                        reject(err);
                    });
            });
        },
        saveRecord: ({ commit }, data) => {
            return new Promise((resolve, reject) => {
                let url = data.url;
                let payload = data.payload;
                ApiService.post(url, payload)
                    .then((resp) => {
                        resolve(resp);
                    })
                    .catch((err) => {
                        reject(err);
                    });
            });
        },
        updateRecord: ({ commit }, data) => {
            return new Promise((resolve, reject) => {
                let url = data.url;
                let payload = data.payload;
                ApiService.put(url, payload)
                    .then((resp) => {
                        resolve(resp);
                    })
                    .catch((err) => {
                        reject(err);
                    });
            });
        },
        deleteRecord: ({ commit }, data) => {
            return new Promise((resolve, reject) => {
                let url = data.url;
                let id = data.id;
                ApiService.delete(`${url}/${id.toString()}`)
                    .then((resp) => {
                        commit("deleteRecord", id);
                        resolve(resp);
                    })
                    .catch((err) => {
                        reject(err);
                    });
            });
        },
    },
};
