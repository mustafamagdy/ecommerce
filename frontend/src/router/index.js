import {route} from 'quasar/wrappers'
import {createRouter, createMemoryHistory, createWebHistory, createWebHashHistory} from 'vue-router'

/*
 * If not building with SSR mode, you can
 * directly export the Router instantiation;
 *
 * The function below can be async too; either use
 * async/await or return a Promise which resolves
 * with the Router instance.
 */

let routes = [{
    name: 'main',
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [
        {path: '/home', name: 'home', component: () => import('pages/home/home.vue'), props: true},
        {path: '/test', name: 'test', component: () => import('components/test'), props: true},
        {path: '/products', name: 'products', component: () => import('src/pages/products/index.vue'), props: true},
//Error pages
        {path: '/error/forbidden', name: 'forbidden', component: () => import('pages/errors/forbidden.vue')},
        {path: '/error/notfound', name: 'notfound', component: () => import('pages/errors/pagenotfound.vue')}
        // { path: '/sampletable', name: 'sampletablelist', component: () => import('pages/sampletable/list.vue'), props: true },
        // 	{ path: '/sampletable/index/:fieldName?/:fieldValue?', component: () => import('pages/sampletable/list.vue'), props: true },
        // 	{ path: '/sampletable/view/:id', name: 'sampletableview', component: () => import('pages/sampletable/view.vue'), props: true },
        // 	{ path: '/sampletable/add', name: 'sampletableadd', component: () => import('pages/sampletable/add.vue'), props: true },
        // 	{ path: '/sampletable/edit/:id', name: 'sampletableedit', component: () => import('pages/sampletable/edit.vue'), props: true },
    ],

},
    {path: '/:catchAll(.*)*', component: () => import('pages/errors/pagenotfound.vue')}
];

export default route(async function ({store}) {
    const createHistory = process.env.SERVER
        ? createMemoryHistory
        : (process.env.VUE_ROUTER_MODE === 'history' ? createWebHistory : createWebHashHistory)

    let mainRoute = routes.find(x => x.name = "main");


    const Router = createRouter({
        scrollBehavior: () => ({left: 0, top: 0}),
        routes,
        // Leave this as is and make changes in quasar.conf.js instead!
        // quasar.conf.js -> build -> vueRouterMode
        // quasar.conf.js -> build -> publicPath
        history: createHistory(process.env.VUE_ROUTER_BASE)
    })
    return Router
})




