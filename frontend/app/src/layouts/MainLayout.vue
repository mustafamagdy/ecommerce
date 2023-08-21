<template>
    <q-layout view="hHh LpR lfr" class="column">
        <!-- App header -->
        <q-header class="transparent text-blue-grey">
            <q-toolbar class="">
                <q-btn @click="toggleLeftDrawer" dense icon="menu" class="bg-color-dark"></q-btn>
                <!-- App logo and name -->
                <q-btn no-caps flat stretch to="/home" class="q-mr-lg">
                    <q-toolbar-title class="text-blue-grey">{{ $appName }}</q-toolbar-title>
                </q-btn>
                <q-separator inset dark vertical></q-separator>
                <!-- Top menu left -->
                <template v-for="(menu, index) in navbarTopLeftItems">
                    <q-btn
                        no-caps
                        :icon="menu.icon"
                        stretch
                        flat
                        :label="menu.label"
                        :to="menu.path"
                        v-if="!menu.submenu.length"
                        :key="`topleftmenubtn-${index}`"
                    ></q-btn>
                    <q-btn-dropdown no-caps :icon="menu.icon" stretch flat :label="menu.label" v-else :key="`topleftmenudrop-${index}`">
                        <q-list dense>
                            <q-item
                                v-for="(submenu, subindex) in menu.submenu"
                                :key="`topleftsubmenu-${subindex}`"
                                clickable
                                v-ripple
                                :to="submenu.path"
                            >
                                <q-item-section avatar v-if="submenu.icon">
                                    <q-avatar :icon="submenu.icon"></q-avatar>
                                </q-item-section>
                                <q-item-section>
                                    <q-item-label>{{ submenu.label }}</q-item-label>
                                </q-item-section>
                            </q-item>
                        </q-list>
                    </q-btn-dropdown>
                </template>
                <q-space></q-space>
                <!-- Top menu right -->
                <template v-for="(menu, index) in navbarTopRightItems">
                    <q-btn
                        no-caps
                        :icon="menu.icon"
                        stretch
                        flat
                        :label="menu.label"
                        :to="menu.path"
                        v-if="!menu.submenu.length"
                        :key="`toprightmenu-${index}`"
                    ></q-btn>
                    <q-btn-dropdown no-caps :icon="menu.icon" stretch flat :label="menu.label" v-else :key="`toprightmenudrop-${index}`">
                        <q-list dense>
                            <q-item
                                v-for="(submenu, subindex) in menu.submenu"
                                :key="`toprightsubmenu-${subindex}`"
                                clickable
                                v-ripple
                                :to="submenu.path"
                            >
                                <q-item-section avatar v-if="submenu.icon">
                                    <q-avatar :icon="submenu.icon"></q-avatar>
                                </q-item-section>
                                <q-item-section>
                                    <q-item-label>{{ submenu.label }}</q-item-label>
                                </q-item-section>
                            </q-item>
                        </q-list>
                    </q-btn-dropdown>
                </template>
            </q-toolbar>
        </q-header>
        <!-- App left drawer -->
        <q-drawer v-model="leftDrawer" :width="250" :breakpoint="500" :mini="leftDrawerMini">
            <q-scroll-area class="fit">
                <q-separator></q-separator>
                <q-list>
                    <template v-for="(menu, index) in navbarSideLeftItems">
                        <q-item clickable q-ripple :to="menu.path" v-if="!menu.submenu.length" :key="`sideleftmenu-${index}`">
                            <q-item-section avatar v-if="menu.icon">
                                <q-icon :color="menu.iconcolor || 'primary'" :name="menu.icon"></q-icon>
                                <q-tooltip
                                    v-if="leftDrawerMini"
                                    transition-show="scale"
                                    transition-hide="scale"
                                    content-class="bg-accent"
                                    anchor="center right"
                                    self="center left"
                                    :offset="[10, 10]"
                                >
                                    {{ menu.label }}
                                </q-tooltip>
                            </q-item-section>
                            <q-item-section>
                                {{ menu.label }}
                            </q-item-section>
                        </q-item>

                        <q-expansion-item
                            expand-separator
                            :content-inset-level="0.5"
                            group="leftmenu"
                            v-else
                            :key="`sideleftmenudrop-${index}`"
                        >
                            <template v-slot:header>
                                <q-item-section avatar v-if="menu.icon">
                                    <q-icon :color="menu.iconcolor || 'primary'" :name="menu.icon"></q-icon>
                                    <q-tooltip
                                        v-if="leftDrawerMini"
                                        transition-show="scale"
                                        transition-hide="scale"
                                        content-class="bg-accent"
                                        anchor="center right"
                                        self="center left"
                                        :offset="[10, 10]"
                                    >
                                        {{ menu.label }}
                                    </q-tooltip>
                                </q-item-section>
                                <q-item-section>
                                    {{ menu.label }}
                                </q-item-section>
                            </template>
                            <q-list dense>
                                <q-item
                                    :to="submenu.path"
                                    clickable
                                    q-ripple
                                    v-for="(submenu, subindex) in menu.submenu"
                                    :key="`sideleftsubmenubtn-${subindex}`"
                                >
                                    <q-item-section avatar v-if="submenu.icon">
                                        <q-icon :color="submenu.iconcolor || 'primary'" :name="submenu.icon"></q-icon>
                                    </q-item-section>
                                    <q-item-section>
                                        {{ submenu.label }}
                                    </q-item-section>
                                </q-item>
                            </q-list>
                        </q-expansion-item>
                    </template>
                </q-list>
            </q-scroll-area>
        </q-drawer>
        <!-- App right drawer -->
        <q-drawer
            :no-swipe-open="!drawerComponentFile"
            side="right"
            v-model="showRightDrawer"
            :width="rightDrawer.width"
            :breakpoint="500"
            overlay
            elevated
        >
            <component
                v-if="showRightDrawer"
                is-sub-page
                :is="drawerComponentFile"
                :api-path="rightDrawer.pageUrl"
                :page-data="rightDrawer.pageData"
            />
            <q-btn
                style="position: absolute; top: 2px; right: 2px"
                unelevated
                dense
                round
                color="grey-1"
                text-color="grey"
                icon="close"
                @click="showRightDrawer = false"
            ></q-btn>
        </q-drawer>
        <q-footer></q-footer>
        <!-- App layout container -->
        <q-page-container>
            <q-page class="column q-pa-md">
                <router-view />
            </q-page>
        </q-page-container>
        <!-- Page display dialog -->
        <q-dialog
            :seamless="pageDialog.seamless"
            :maximized="pageDialog.maximized"
            :persistent="pageDialog.persistent"
            :position="pageDialog.position"
            v-model="showPageDialog"
        >
            <q-card :style="{ width: pageDialog.width, maxWidth: pageDialog.maxWidth }">
                <component
                    v-if="pageDialog.showDialog"
                    is-sub-page
                    :is="dialogComponentFile"
                    :api-path="pageDialog.pageUrl"
                    :page-data="pageDialog.pageUrl"
                />
                <q-btn
                    style="position: absolute; top: 2px; right: 2px"
                    v-if="pageDialog.closeBtn"
                    icon="close"
                    flat
                    round
                    dense
                    v-close-popup
                />
            </q-card>
        </q-dialog>
        <!-- image preview dialog-->
        <q-dialog v-model="showImageDialog">
            <q-carousel
                transition-prev="scale"
                transition-next="scale"
                swipeable
                animated
                control-type="unelevated"
                control-color="primary"
                v-model="imageDialog.currentSlide"
                :navigation="false"
                :arrows="imageDialog.images.length > 1"
                infinite
                :padding="false"
                height="auto"
                class="bg-dark rounded-borders"
            >
                <q-carousel-slide class="no-padding" v-for="(img, index) in imageDialog.images" :key="index" :name="index">
                    <img style="max-width: 100%; max-height: 100%" :src="img" />
                </q-carousel-slide>
            </q-carousel>
        </q-dialog>
        <!-- request error dialog -->
        <q-dialog v-model="errorDialog" position="top">
            <q-card style="min-width: 300px; max-width: 95vw">
                <q-card-section class="row items-center align-start">
                    <q-avatar text-color="negative" size="40px" font-size="36px" icon="error"></q-avatar>
                    <div class="q-ml-sm">
                        <div class="text-weight-bold text-negative">{{ $t("unableToCompleteRequest") }}</div>
                        <q-separator class="q-my-sm" />
                        <div class="text-capitalize" v-for="(msg, index) in pageErrors" :key="index">
                            {{ msg }}
                        </div>
                    </div>
                    <q-space />
                    <q-btn icon="close" flat round dense v-close-popup />
                </q-card-section>
            </q-card>
        </q-dialog>
    </q-layout>
</template>
<script>
import { defineAsyncComponent, defineComponent, computed } from "vue";
import { useApp } from "src/composables/app";
import { useStore } from "vuex";

export default defineComponent({
    setup(props) {
        const app = useApp();
        const store = useStore();
        const leftDrawer = computed({
            get: function () {
                return store.state.app.leftDrawer;
            },
            set: function (value) {
                store.commit("app/setLeftDrawer", value);
            },
        });

        const leftDrawerMini = computed({
            get: function () {
                return store.state.app.leftDrawerMini;
            },
            set: function (value) {
                store.commit("app/setLeftDrawerMini", value);
            },
        });
        const pageDialog = computed(() => store.state.app.pageDialog);
        const imageDialog = computed(() => store.state.app.imageDialog);
        const rightDrawer = computed(() => store.state.app.rightDrawer);

        const showPageDialog = computed({
            get() {
                return pageDialog.value.showDialog;
            },
            set(value) {
                store.dispatch("app/closePageDialog");
            },
        });

        const showImageDialog = computed({
            get() {
                return imageDialog.value.showDialog;
            },
            set(value) {
                store.dispatch("app/closeImageDialog");
            },
        });

        const showRightDrawer = computed({
            get() {
                return rightDrawer.value.showDrawer;
            },
            set(value) {
                store.dispatch("app/closeRightDrawer");
            },
        });
        const pageErrors = computed({
            get() {
                return store.state.app.pageErrors;
            },
            set(value) {
                store.commit("app/setPageErrors", value);
            },
        });

        const errorDialog = computed({
            get() {
                return pageErrors.value.length > 0;
            },
            set(newValue) {
                pageErrors.value = [];
            },
        });

        const dialogComponentFile = computed(() => {
            const dialog = pageDialog.value;
            if (dialog.showDialog && dialog.pageComponent) {
                return defineAsyncComponent(() => import(`src/pages/${dialog.pageComponent}.vue`));
            }
            return null;
        });

        const drawerComponentFile = computed(() => {
            const drawer = rightDrawer.value;
            if (drawer.showDrawer && drawer.pageComponent) {
                return defineAsyncComponent(() => import(`src/pages/${drawer.pageComponent}.vue`));
            }
            return null;
        });

        function toggleLeftDrawer() {
            if (leftDrawer.value && leftDrawerMini.value) {
                leftDrawer.value = false;
            } else if (leftDrawer.value && !leftDrawerMini.value) {
                leftDrawerMini.value = true;
            } else {
                leftDrawer.value = true;
                leftDrawerMini.value = false;
            }
        }

        return {
            drawerComponentFile,
            dialogComponentFile,
            leftDrawer,
            leftDrawerMini,
            pageDialog,
            imageDialog,
            errorDialog,
            pageErrors,
            rightDrawer,
            showPageDialog,
            showImageDialog,
            showRightDrawer,
            toggleLeftDrawer,
        };
    },
    name: "MainLayout",
    computed: {
        navbarSideLeftItems() {
            return this.$menus.navbarSideLeftItems;
        },
        navbarTopLeftItems() {
            return this.$menus.navbarTopLeftItems;
        },
        navbarTopRightItems() {
            return this.$menus.navbarTopRightItems;
        },
    },
    watch: {
        $route(to, from) {
            this.$store.dispatch("app/closeDialogs");
        },
    },
    methods: {
        initAxioInterceptors() {
            // Add a request interceptor
            this.$api.axios().interceptors.request.use(
                (config) => {
                    this.$store.commit("app/setPageErrors", []);
                    return config;
                },
                (error) => {
                    // Do something with request error
                    return Promise.reject(error);
                }
            );

            this.$api.axios().interceptors.response.use(
                (response) => {
                    return response;
                },
                (error) => {
                    // reject error. Error will be handle by calling page.
                    throw error;
                }
            );
        },
    },
    created: function () {
        //let showLeftDrawer = this.$q.platform.is.desktop;
        //this.$store.commit("app/setLeftDrawer", showLeftDrawer);
        this.initAxioInterceptors();
    },
});
</script>

<style></style>
<style lang="scss"></style>
