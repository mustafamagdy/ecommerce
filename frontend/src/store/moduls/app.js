export const app = {
    namespaced: true,
    state: {
        leftDrawer: true,
        leftDrawerMini: false,
        rightDrawer: {
            showDrawer: false,
            pageComponent: "",
            pageUrl: "",
            pageData: null,
            rightDrawerWidth: 400
        },
        pageErrors: [],
        pageDialog: {
            showDialog: false,
            pageComponent: "",
            pageUrl: "",
            pageData: null,
            seamless: false,
            position: "standard",
            persistent: false,
            maximized: false,
            closeBtn: false,
            width: "700px"
            //width: '95vw',
        },
        imageDialog: {
            showDialog: false,
            currentSlide: 1,
            images: []
        }
    },
    getters: {
        isDialogOpen(state) {
            return state.pageDialog.showDialog || state.rightDrawer.showDrawer;
        }
    },
    mutations: {
        setPageDialog(state, payload) {
            if (payload.showDialog) {
                state.pageDialog.pageComponent = payload.page;
                state.pageDialog.pageUrl = payload.url;

                state.pageDialog.seamless = payload.seamless || false;
                state.pageDialog.persistent = payload.persistent || false;
                state.pageDialog.position = payload.position || "standard";
                state.pageDialog.maximized = payload.maximized || false;
                state.pageDialog.closeBtn = payload.closeBtn || false;
                if (payload.maximized) {
                    state.pageDialog.width = payload.width || "";
                    state.pageDialog.maxWidth = "";
                } else {
                    state.pageDialog.width = payload.width || "700px";
                    state.pageDialog.maxWidth = "95vw";
                }
            }
            state.pageDialog.showDialog = payload.showDialog;
        },
        setRightDrawer(state, payload) {
            if (payload.showDrawer) {
                state.rightDrawer.width = payload.width || 400;
                state.rightDrawer.pageComponent = payload.page;
                state.rightDrawer.pageUrl = payload.url;
            }
            state.rightDrawer.showDrawer = payload.showDrawer;
        },
        setImageDialog(state, payload) {
            if (payload.showDialog) {
                state.imageDialog.images = payload.images;
                state.imageDialog.currentSlide = payload.currentSlide;
            }
            state.imageDialog.showDialog = payload.showDialog;
        },
        closeDialogs(state) {
            state.pageDialog.showDialog = false;
            state.imageDialog.showDialog = false;
            state.rightDrawer.showDrawer = false;
        },
        closePageDialog(state) {
            state.pageDialog.showDialog = false;
        },
        closeImageDialog(state) {
            state.imageDialog.showDialog = false;
        },
        closeRightDrawer(state) {
            state.rightDrawer.showDrawer = false;
        },
        setLeftDrawer(state, value) {
            state.leftDrawer = value;
        },
        setLeftDrawerMini(state, value) {
            state.leftDrawerMini = value;
        },
        setPageErrors(state, errors) {
            state.pageErrors = errors;
        }
    },
    actions: {
        openPageDrawer: ({ commit }, payload) => {
            payload.showDrawer = true;
            commit("setRightDrawer", payload);
        },
        openPageDialog: ({ commit }, payload) => {
            payload.showDialog = true;
            commit("setPageDialog", payload);
        },
        openImageDialog: ({ commit }, payload) => {
            payload.showDialog = true;
            commit("setImageDialog", payload);
        },
        closeDialogs: ({ commit }) => {
            commit("closeDialogs");
        },

        closePageDialog: ({ commit }) => {
            commit("closePageDialog");
        },

        closeImageDialog: ({ commit }) => {
            commit("closeImageDialog");
        },

        closeRightDrawer: ({ commit }) => {
            commit("closeRightDrawer");
        },

        showPageErrors: ({ commit }, errors) => {
            commit("setPageErrors", errors);
        }
    }
};
