import {reactive, computed, onMounted, watch, watchEffect} from "vue";
import {utils} from "src/utils";
import {useApp} from "src/composables/app";
import {useStore} from "vuex";
import {useRoute} from "vue-router";
import {useQuasar} from "quasar";
import {$t} from "src/services/i18n";

export const useListPage = (props, options, filters) => {
    const {
        primaryKey = "id",
        apiPath = "",
        pageName = "",
        pageSize = 20
    } = options;
    const app = useApp();
    const route = useRoute();
    const store = useStore();
    const $q = useQuasar();
    const state = reactive({
        totalRecords: 0,
        loading: false,
        currentPage: 1,
        totalPages: 1
    });
    let records = computed({
        get() {
            return store.getters[`${pageName}/records`];
        },
        set(value) {
            store.commit(`${pageName}/setRecords`, value);
        }
    });
    let currentRecord = computed({
        get() {
            return store.getters[`${pageName}/currentRecord`];
        },
        set(value) {
            store.commit(`${pageName}/setCurrentRecord`, value);
        }
    });

    async function load() {
        state.loading = true;
        currentRecord.value = null;
        let url = `${apiPath}/search`;
        let payload = {
            url,
            criteria: {pageNumber: state.currentPage, pageSize: pageSize},
            merge: props.mergeRecords
        };
        try {
            let response = await store.dispatch(
                `${pageName}/search`,
                payload
            );
            state.loading = false;
            state.totalRecords = response.totalCount;
            state.totalPages = response.totalPages;
            state.recordCount = response.totalPages * response.pageSize;
        } catch (e) {
            state.loading = false;
            app.showPageRequestError(e);
        }
    }

    async function deleteItem(id) {
        if (Array.isArray(id)) {
            id = id.map((value) => value[primaryKey]);
        }
        if (id) {
            let title = $t("delete_confirmation_dialog_title");
            let msg = $t("delete_confirmation_dialog_message");
            $q.dialog({
                title: title,
                message: msg,
                cancel: true,
                persistent: true,
            })
                .onOk(async () => {
                    let url = `${pageName}/${id.toString()}`;
                    let payload = {id, url};
                    try {
                        await store.dispatch(
                            `${pageName}/deleteRecord`,
                            payload
                        );
                        app.flashMsg($t("after_delete_message"));
                    } catch (e) {
                        app.showPageRequestError(e);
                    }
                })
                .onCancel(() => {
                    // console.log('>>>> Cancel')
                })
                .onDismiss(() => {
                    // console.log('I am triggered on both OK and Cancel')
                });
        }
    }

    function isCurrentRecord(row) {
        return row === currentRecord;
    }

    const computedProps = {
        records,
        currentRecord,
    };
    const methods = {
        load,
        isCurrentRecord,
        deleteItem,
    };
    return {
        state,
        computedProps,
        methods
    };
};
