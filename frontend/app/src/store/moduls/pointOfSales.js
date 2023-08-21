
export const pointOfSales = {
    namespaced: true,
    state: () => ({
        services: []
    }),
    getters: {
        services(state) {
            return state.services;
        }
    },
    mutations: {
        setServices(state, records) {
            state.services = records;
        }
    }
};
