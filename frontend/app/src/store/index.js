import { store } from "quasar/wrappers";
import { createStore } from "vuex";
import { app } from "./moduls/app.js";
import { crud } from "./moduls/crud.js";
import { pointOfSales } from "./moduls/pointOfSales.js";

/*
 * If not building with SSR mode, you can
 * directly export the Store instantiation;
 *
 * The function below can be async too; either use
 * async/await or return a Promise which resolves
 * with the Store instance.
 */

export default store(function (/* { ssrContext } */) {
    return createStore({
        modules: {
            app: app,
            services: crud,
            products: crud,
            serviceCatalog: crud,
            financialBoxes: crud,
            customers: crud,
            pointOfSales: pointOfSales,
            bills: crud,
            branches: crud,
            subscriptions: crud,
            financialBoxesTransactions: crud,
            subscriptionAccountStatement: crud,
            employeesJobs: crud,
            employees: crud,
        },
        // enable strict mode (adds overhead!)
        // for dev mode and --debug builds only
        // strict: process.env.DEBUGGING
    });
});
