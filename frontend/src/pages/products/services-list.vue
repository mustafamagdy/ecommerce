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
                selection="single"
                row-key="id"
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
                                    <img :src="props.row.imageUrl" alt=""/>
                                </q-avatar>
                                <span class="col-grow text-h6">{{ props.row.name }}</span>
                                <q-checkbox v-model="props.selected" class="self-start"/>
                            </div>
                            <q-separator class="q-my-sm"/>
                            <div class="row justify-center">
                                <q-btn icon="mdi-delete-outline" :label="$t('btn_delete')" padding="xs"
                                       class="delete"/>
                                <q-btn icon="mdi-playlist-edit" :label="$t('btn_edit')" padding="xs" class="edit"/>
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
                <template v-slot:bottom>
                    <div class="q-pa-lg flex flex-center">
                        <q-pagination
                            v-model="currentPage"
                            :max="totalPages"
                            boundary-links
                            direction-links
                            input
                            @update:model-value="load"
                        />
                    </div>
                </template>
            </q-table>
            <!-- page loading indicator template   test -->
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
            <q-btn icon="mdi-plus" :label="$t('btn_add_new')" padding="xs" class="add" @click="showAdd=true"/>
        </div>
        <q-dialog v-model="showAdd">
            <ServicesAdd/>
        </q-dialog>
    </div>
</template>
<script setup>
import {computed, ref, reactive, toRefs, onMounted} from "vue";
import {useMeta} from "quasar";
import {useApp} from "src/composables/app.js";
import {utils} from "src/utils";
import {$t} from "src/services/i18n";
import {useListPage} from "src/composables/listpage.js";
import ServicesAdd from "./services-add";

const options = reactive({
    apiPath: "services",
    pageName: "services",
    pageSize: 20,
    filter: {},
    orderBy: [],
})
const props = defineProps({
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
const page = useListPage(props, options, filters);
//page state
const {
    totalRecords, // total records from api - Number
    loading, // Api loading state - Boolean
    currentPage,
    totalPages
} = toRefs(page.state);
const showAdd = ref(false);
//page computed properties
const {
    records,
    currentRecord,
} = page.computedProps;
//page methods
const {
    load,
    isCurrentRecord
} = page.methods;
const pageTitle = computed({
    get: function () {
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
