<template>
    <div class="row col-grow items-stretch justify-start">
        <div class="column q-pa-sm">
            <servicesList class="col-grow panel" />
        </div>

        <div class="column col q-pa-sm">
            <div class="column col-grow panel">
                <div class="column">
                    <div class="row justify-between items-center no-wrap">
                        <span>{{ $t("Products") }} : {{ page.totalRecords }}</span>
                        <span>{{ $t("pages") }} : {{ page.totalPages }}</span>
                        <div class="row no-wrap">
                            <q-btn icon="done" :label="app.btnLabel('Save')" padding="xs" class="bg-color-positive" />
                            <q-btn icon="close" :label="app.btnLabel('Cancel')" padding="xs" class="bg-color-negative" />
                        </div>
                    </div>
                </div>
                <q-separator class="q-mt-sm" inset />
                <q-scroll-area class="column col-grow" visible>
                    <q-table
                        card-container-class="q-col-gutter-md justify-start"
                        :bordered="false"
                        :rows="page.records"
                        selection="single"
                        row-key="id"
                        :loading="page.loading"
                        hide-pagination
                        :pagination="{ rowsPerPage: 0 }"
                        dense
                        flat
                        hide-header
                    >
                        <template v-slot:body="props">
                            <q-tr @click="page.clickedRecord = props.row">
                                <q-td>
                                    <q-avatar size="md">
                                        <img :src="props.row.imageUrl" alt="" />
                                    </q-avatar>
                                </q-td>
                                <q-td>
                                    <span>{{ props.row.name }}</span>
                                </q-td>
                                <q-td>
                                    <q-input v-model="text" label="price" />
                                </q-td>
                            </q-tr>
                        </template>
                    </q-table>

                    <!-- <div class="row q-pa-md justify-around" v-for="record in page.records" :key="record.id">
                        <q-avatar size="md">
                            <img :src="record.imageUrl" alt="" />
                        </q-avatar>
                        <span>{{ record.name }}</span>
                        <q-input v-model="text" label="price" />
                        <q-separator class="q-mt-sm" inset />
                    </div> -->
                </q-scroll-area>
            </div>
        </div>
    </div>
</template>

<script setup>
import ServicesList from "./services-list.vue";
import { useApp } from "src/composables/app";
import { serverApis, storeModules } from "src/enums";
import { onMounted, ref, reactive } from "vue";
import { useCRUDList } from "src/composables/useCRUDList";
import servicesList from "./services-list.vue";
const text = ref("");
const app = useApp();
const page = reactive(useCRUDList({ apiPath: serverApis.services, storeModule: storeModules.services, pageSize: 8 }));
onMounted(() => {
    page.load();
});
</script>
