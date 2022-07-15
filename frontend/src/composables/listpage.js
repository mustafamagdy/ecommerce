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

    function isCurrentRecord(row) {
        return row === currentRecord;
    }

    const computedProps = {
        records,
        currentRecord,
    };
    const methods = {
        load,
        isCurrentRecord
    };
    return {
        state,
        computedProps,
        methods
    };
};
