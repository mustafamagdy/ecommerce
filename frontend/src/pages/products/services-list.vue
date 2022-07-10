<template>
    <div class="col-grow column">
        <div class="col-grow">
            <q-table
                grid
                card-container-class="q-col-gutter-md justify-start"
                :bordered="false"
                :columns="$menus.SampletableTableHeaderItems"
                :rows="records"
                binary-state-sort
                v-model:selected="selectedItems"
                selection="single"
                row-key="id"
                v-model:pagination="pagination"
                hide-bottom
                @request="setPagination"
                :loading="loading"
            >
                <template v-slot:top>
                    <div class="col-grow row justify-between">
                        <span>{{ $t("products_groups") }}</span>
                        <span>{{ $t("count") }} : {{ totalRecords }}</span>
                        <span>{{ $t("pages") }} : {{ totalPages }}</span>
                    </div>
                </template>
                <template v-slot:item="props">
                    <div
                        :class="{ selected: isCurrentRecord(props.row) }"
                        class="col-12 col-md-6"
                        :style="props.selected ? 'transform: scale(0.95);' : ''"
                    >
                        <q-card class="">
                            <div class="row no-wrap items-center">
                                <q-avatar>
                                    <img :src="props.row.imageUrl" alt="" />
                                </q-avatar>
                                <span class="col-grow text-h6">{{ props.row.name }}</span>
                                <q-checkbox v-model="props.selected" class="self-start" />
                            </div>
                            <q-separator class="q-my-sm" />
                            <div class="row justify-center">
                                <q-btn icon="mdi-delete-outline" :label="$t('btn_delete')" padding="xs"
                                       class="delete" />
                                <q-btn icon="mdi-playlist-edit" :label="$t('btn_edit')" padding="xs" class="edit" />
                                <q-btn icon="mdi-cog-outline" :label="$t('btn_actions')" padding="xs" class="settings"
                                >
                                    <q-menu auto-close>
                                        <q-list rounded nav>
                                            <q-item link clickable v-ripple>
                                                <q-item-section>
                                                    <q-icon size="md"
                                                            :name="props.row.isActive ? 'mdi-pause-circle-outline' : 'mdi-play-circle-outline'"></q-icon>
                                                </q-item-section>
                                                <q-item-section>
                                                    {{ props.row.isActive ? $t("deactivate") : $t("activate") }}
                                                </q-item-section>
                                            </q-item>
                                        </q-list>
                                    </q-menu>
                                </q-btn
                                >
                            </div>
                        </q-card>
                    </div>
                </template>
            </q-table>
            <!-- page loading indicator template -->
            <template v-if="loading">
                <q-inner-loading :showing="loading">
                    <q-spinner color="primary" size="30px"></q-spinner>
                </q-inner-loading>
            </template>
            <!-- page empty record template -->
            <template v-if="!loading && !records.length">
                <q-card :flat="$q.screen.gt.md" class="q-mt-md col-grow">
                    <q-card-section>
                        <div class="text-grey text-h6 text-center">
                            {{ $t("noRecordFound") }}
                        </div>
                    </q-card-section>
                </q-card>
            </template>
        </div>
        <div class="row justify-end q-mt-md">
            <q-btn icon="mdi-plus" :label="$t('btn_add_new')" padding="xs" class="add" @click="showAdd=true" />
        </div>
        <q-dialog v-model="showAdd">
            <ServicesAdd />
        </q-dialog>
    </div>
</template>
<script setup>
import { computed, ref, reactive, toRefs, onMounted } from "vue";
import { useMeta } from "quasar";
import { useApp } from "src/composables/app.js";
import { utils } from "src/utils";
import { $t } from "src/services/i18n";
import { useListPage } from "src/composables/listpage.js";
import ServicesAdd from "./services-add";

const props = defineProps({
    primaryKey: {
        type: String,
        default: "id"
    },
    pageName: {
        type: String,
        default: "services"
    },
    routeName: {
        type: String,
        default: ""
    },
    apiPath: {
        type: String,
        default: "services"
    },
    paginate: {
        type: Boolean,
        default: true
    },
    isSubPage: {
        type: Boolean,
        default: false
    },
    showHeader: {
        type: Boolean,
        default: true
    },
    showFooter: {
        type: Boolean,
        default: true
    },
    showBreadcrumbs: {
        type: Boolean,
        default: true
    },
    exportButton: {
        type: Boolean,
        default: true
    },
    importButton: {
        type: Boolean,
        default: false
    },
    listSequence: {
        type: Boolean,
        default: true
    },
    multiCheckbox: {
        type: Boolean,
        default: true
    },
    emptyRecordMsg: {
        type: String,
        default: () => $t("noRecordFound")
    },
    msgBeforeDelete: {
        type: String,
        default: () => $t("promptDeleteRecord")
    },
    msgAfterDelete: {
        type: String,
        default: () => $t("recordDeletedSuccessfully")
    },
    page: {
        type: Number,
        default: 1
    },
    limit: {
        type: Number,
        default: 10
    },
    search: {
        type: String,
        default: ""
    },
    fieldName: null,
    fieldValue: null,
    sortBy: {
        type: String,
        default: ""
    },
    sortType: {
        type: String,
        default: "" //desc or asc
    }
});
const app = useApp();
const filters = reactive({});
//init list page hook
const page = useListPage(props, filters);
//page state
const {
    totalRecords, // total records from api - Number
    recordCount, // current record count - Number
    loading, // Api loading state - Boolean
    selectedItems, // Data table selected items -Array
    pagination, //Pagination object - Object
    searchText // search text - String
} = toRefs(page.state);
const showAdd = ref(false);
//page computed properties
const {
    records, // page record from store - Array
    //apiUrl, // current api path - URL
    currentRecord, // master detail selected record - Object
    pageBreadCrumb, // get page navigation paths - Object
    canLoadMore, // if api has more data for loading - Boolean
    finishedLoading, // if api has finished loading - Boolean
    totalPages, // total number of pages from api - Number
    recordsPosition // current record position from api - Number
} = page.computedProps;
//page methods
const {
    load, // load data from api
    reload, // reset pagination and load data
    exportPage, // export page records - args = (exportFormats, apiUrl, pageName)
    clearSearch, // clear input search
    onSort, // reset page number before sort from api
    setPagination, // update pagination with data from api
    deleteItem, // delete item by id or selected items - args = (id) or (selectedItems)
    setCurrentRecord, // master detail set current record
    isCurrentRecord, // master detail
    removeFilter, // remove page filter item - args = (filter.propertyname)
    getFilterLabel, // get filter item display label - args = (filter.propertyname)
    filterHasValue, //check if filter item has value - args = (filter.propertyname)
    importComplete // reload page after data import
} = page.methods;
const pageTitle = computed({
    get: function() {
        return $t("sampletable");
    }
});
useMeta(() => {
    return {
        title: pageTitle.value //set browser title
    };
});
onMounted(() => {
    load();
});
</script>
<style scoped>
</style>
