import { reactive, computed, onMounted, watch, watchEffect } from "vue";
import { utils } from "src/utils";
import { useApp } from "src/composables/app";
import { useStore } from "vuex";
import { useRoute } from "vue-router";
import { useQuasar } from "quasar";
import { $t } from "src/services/i18n";

export const useListPage = (props, filters) => {
    const app = useApp();
    const route = useRoute();

    const store = useStore();
    const $q = useQuasar();

    const state = reactive({
        totalRecords: 0,
        recordCount: 0,
        loading: false,
        singleSelect: false,
        selectedItems: [],
        pagination: {
            page: 1,
            rowsPerPage: props.limit,
            sortBy: "",
            rowsNumber: 0,
            descending: true
        },
        deleting: false,
        searchText: "",
        errorMsg: ""
    });

    let records = computed({
        get() {
            return store.getters[`${props.pageName}/records`];
        },
        set(value) {
            store.commit(`${props.pageName}/setRecords`, value);
        }
    });

    let currentRecord = computed({
        get() {
            return store.getters[`${props.pageName}/currentRecord`];
        },
        set(value) {
            store.commit(`${props.pageName}/setCurrentRecord`, value);
        }
    });

    /*    const apiUrl = computed(() => {
            let path = props.apiPath;
            if (props.fieldName) {
                path =
                    path +
                    "/" +
                    encodeURIComponent(props.fieldName) +
                    "/" +
                    encodeURIComponent(props.fieldValue);
            }
            if (props.sortBy) {
                state.pagination.sortBy = props.sortBy;
            } else if (route.query.sortby) {
                state.pagination.sortBy = route.query.sortby;
            }

            if (props.sortType) {
                state.pagination.descending =
                    props.sortType.toLowerCase() === "desc";
            } else if (route.query.sorttype) {
                state.pagination.descending =
                    route.query.sorttype.toLowerCase() === "desc";
            }

            const query = {};
            query.page = state.pagination.page;
            query.limit = state.pagination.rowsPerPage;

            if (state.pagination.sortBy) {
                let orderType = state.pagination.descending ? "desc" : "asc";
                query.orderby = state.pagination.sortBy;
                query.ordertype = orderType;
            }

            if (state.searchText) {
                query.search = state.searchText;
            }

            for (const [key, filter] of Object.entries(filters)) {
                if (filterHasValue(filter)) {
                    if (filter.valueType === "range") {
                        query[key] = {
                            min: filter.value.min,
                            max: filter.value.max
                        };
                    } else if (filter.valueType === "range-date") {
                        let fromDate = filter.value.from;
                        let toDate = filter.value.to;
                        query[key] = { from: fromDate, to: toDate };
                    } else if (filter.valueType === "multiple-date") {
                        query[key] = filter.value; //.map((val) => utils.formatDate(val));
                    } else {
                        query[key] = filter.value;
                    }
                }
            }

            const queryParams = utils.serializeQuery(query);
            return path + "?" + queryParams;
        });*/

    const recordsPosition = computed(() => {
        return Math.min(
            state.pagination.page * state.pagination.rowsPerPage,
            state.totalRecords
        );
    });

    const totalRecords = computed(() => {
        return state.totalRecords;
    });

    const totalPages = computed(() => {
        if (state.totalRecords > state.pagination.rowsPerPage) {
            return Math.ceil(state.totalRecords / state.pagination.rowsPerPage);
        }
        return 1;
    });

    const finishedLoading = computed(() => {
        return state.recordCount < state.pagination.rowsPerPage &&
            records.length;

    });

    const canLoadMore = computed(() => {
        return !(state.loading || finishedLoading);

    });

    const pageBreadCrumb = computed(() => {
        let items = [];
        let filterName = route.query.tag || props.fieldName;
        items.push({
            label: filterName,
            to: `/${props.pageName}`
        });

        let filterValue = route.query.label || props.fieldValue;
        items.push({
            label: filterValue,
            to: `/${props.pageName}`
        });
        return items;
    });

    function setPagination(pager) {
        let { page, rowsPerPage, rowsNumber, sortBy, descending } =
            pager.pagination;
        state.pagination.sortBy = sortBy;
        state.pagination.descending = descending;
    }

    function onSort(event) {
        if (props.mergeRecords) {
            records = [];
            pagination.page = 1;
        }
        state.pagination.sortBy = event.sortField;
        state.pagination.descending = event.sortOrder === -1;
    }

    async function load() {
        if (!canLoadMore) {
            return;
        }

        state.loading = true;
        currentRecord.value = null;
        let url = `${props.apiPath}/search`;
        let payload = {
            url,
            criteria: {},
            merge: props.mergeRecords
        };
        try {
            let response = await store.dispatch(
                `${props.pageName}/search`,
                payload
            );
            state.loading = false;
            state.totalRecords = response.totalCount;
            state.recordCount = response.totalPages * response.pageSize;
            state.pagination.rowsNumber = state.totalRecords;
            if (props.mergeRecord) {
                state.pagination.page++;
            }
        } catch (e) {
            state.loading = false;
            app.showPageRequestError(e);
        }
    }

    function reload() {
        records = [];
        const query = route.query;
        state.pagination.rowsPerPage = query.limit || null;
        state.pagination.page = query.page || 1;
        if (query.sorttype) {
            state.pagination.descending = query.sorttype == "desc";
        }
        state.searchText = query.search || null;
    }

    function setCurrentRecord(record) {
        store.commit(`${props.pageName}/setCurrentRecord`, record);
    }

    function isCurrentRecord(row) {
        return row == currentRecord;
    }

    function importComplete(message) {
        app.flashMsg(message);
        reload();
    }

    function exportPage() {
        app.exportPageRecords(
            props.exportFormats,
            //apiUrl.value,
            props.pageName
        );
    }

    async function deleteItem(id) {
        if (Array.isArray(id)) {
            id = id.map((value) => value[props.primaryKey]);
        }

        if (id) {
            let title = $t("deleteRecord");
            let prompt = props.msgBeforeDelete;
            $q.dialog({
                title: title,
                message: prompt,
                cancel: true,
                persistent: true
            })
                .onOk(async () => {
                    let url = `${props.pageName}/delete/${id.toString()}`;
                    let payload = { id, url };
                    try {
                        await store.dispatch(
                            `${props.pageName}/deleteRecord`,
                            payload
                        );
                        app.flashMsg(props.msgAfterDelete);
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

    function removeFilter(filter, selectedVal) {
        let valueType = filter.valueType;
        if (valueType == "single") {
            filter.value = null;
        } else if (valueType == "range") {
            filter.value = { min: 0, max: 0 };
        } else if (valueType == "range-date") {
            filter.value = { from: null, to: null };
        } else if (valueType == "multiple" || valueType == "multiple-date") {
            let idx = filter.value.indexOf(selectedVal);
            filter.value.splice(idx, 1);
        }
    }

    function filterHasValue(filter) {
        if (filter.valueType == "range") {
            if (filter.value.max) {
                return true;
            }
            return false;
        } else if (filter.valueType == "range-date") {
            if (filter.value.to) {
                return true;
            }
            return false;
        } else if (Array.isArray(filter.value)) {
            return filter.value.length > 0;
        } else if (filter.value) {
            return true;
        }
        return false;
    }

    function getFilterLabel(filter, selectedVal) {
        if (filter.valueType == "range" && filter.value.max) {
            let min = filter.value.min;
            let max = filter.value.max;
            return `${min} - ${max}`;
        } else if (filter.valueType == "range-date") {
            if (filter.value.to) {
                let minDate = utils.humanDate(filter.value.from);
                let maxDate = utils.humanDate(filter.value.to);
                return `${minDate} - ${maxDate}`;
            }
            return null;
        } else if (filter.valueType == "multiple-date") {
            let val = selectedVal || filter.value;
            return utils.humanDate(val);
        } else if (filter.valueType == "single-date") {
            return utils.humanDate(filter.value);
        } else if (filter.options.length) {
            let val = selectedVal || filter.value;
            let selectedFilter = filter.options.find((obj) => obj.value == val);
            if (selectedFilter) {
                return selectedFilter.label;
            }
        } else if (selectedVal) {
            return selectedVal.toString();
        }
        return filter.value.toString();
    }

    function clearSearch() {
        state.searchText = "";
        route.query.search = "";
    }

    onMounted(() => {
        if (route.query.search) {
            state.searchText = route.query.search;
        }
    });

    watchEffect(() => {
        if (state.searchText) {
            state.pagination.page = 1;
        }
    });

    //watch(apiUrl, () => load());

    const computedProps = {
        records,
        //apiUrl,
        currentRecord,
        pageBreadCrumb,
        canLoadMore,
        finishedLoading,
        totalPages,
        recordsPosition
    };

    const methods = {
        load,
        reload,
        exportPage,
        clearSearch,
        onSort,
        setPagination,
        deleteItem,
        setCurrentRecord,
        isCurrentRecord,
        removeFilter,
        getFilterLabel,
        filterHasValue,
        importComplete
    };

    return {
        state,
        computedProps,
        methods
    };
};
