import { reactive, ref } from "vue";
import { deleteRecord, search } from "src/services/crud-apis";
import { computed } from "vue";
import { useStore } from "vuex";
import { useApp } from "src/composables/app";

export const useList = ({
    primaryKey = "id",
    apiPath = "",
    storeModule = "",
    listName = "records",
    recordName = "record",
    pageSize = 10,
}) => {
    const upperRecordName =
        recordName.charAt(0).toUpperCase() + recordName.slice(1);
    const upperListName = listName.charAt(0).toUpperCase() + listName.slice(1);
    const app = useApp();
    const store = useStore();
    const loading = ref(false);
    const currentPage = ref(1);
    const totalRecords = ref(0);
    const totalPages = ref(0);
    let totals = reactive({});
    let records = computed({
        get() {
            return store.getters[`${storeModule}/${listName}`];
        },
        set(value) {
            const upperListName =
                listName.charAt(0).toUpperCase() + listName.slice(1);
            store.commit(`${storeModule}/set${upperListName}`, value);
        },
    });
    let clickedRecord = computed({
        get() {
            return store.getters[`${storeModule}/clicked${upperRecordName}`];
        },
        set(value) {
            store.commit(`${storeModule}/setClicked${upperRecordName}`, value);
        },
    });
    let advancedFiltersForRecords = computed({
        get() {
            return store.getters[
                `${storeModule}/advancedFiltersFor${upperListName}`
            ];
        },
        set(value) {
            store.commit(
                `${storeModule}/setAdvancedFiltersFor${upperListName}`,
                value
            );
        },
    });

    async function load() {
        loading.value = true;
        let criteria = {
            pageNumber: currentPage.value,
            pageSize: pageSize,
            advancedFiltersForRecords: advancedFiltersForRecords.value,
        };
        try {
            let data = await search(apiPath + "/search", criteria);
            records.value = data.data;
            console.log(data);
            totalRecords.value = data.totalCount;
            totalPages.value = data.totalPages;
            loading.value = false;
            Object.assign(totals, data.totals);
        } catch (e) {
            loading.value = false;
            app.showPageRequestError(e);
        }
    }

    return {
        loading,
        currentPage,
        totalRecords,
        totalPages,
        records,
        totals,
        load,
        clickedRecord,
        advancedFiltersForRecords,
    };
};
