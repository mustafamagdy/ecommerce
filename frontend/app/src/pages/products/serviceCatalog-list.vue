<template>
    <div class="column">
        <div class="row justify-between items-center">
            <span>{{ $t("products_groups") }}</span>
            <span>{{ $t("count") }} : {{ totalRecords }}</span>
            <span>{{ $t("pages") }} : {{ totalPages }}</span>
            <q-btn icon="mdi-plus" :label="app.btnLabel('btn_add_new')" padding="xs" class=""
                   @click="showAdd=true" />
        </div>
        <div>
            {{ props.service ? props.service.name : "" }}
        </div>
        <q-separator class="q-mt-sm" />
        <q-scroll-area class="col-grow" visible>
            <q-table
                card-container-class="q-col-gutter-md justify-start"
                :bordered="false"
                :rows="records"
                selection="single"
                row-key="id"
                :loading="loading"
                hide-pagination
                :pagination="{rowsPerPage:0}"
                table-style="max-height:100%"
                dense flat
                hide-header
            >
                <template v-slot:body="props">
                    <q-tr>
                        <q-td>
                            <q-avatar size="md">
                                <img :src="props.row.productImageUrl" alt="" />
                            </q-avatar>
                        </q-td>
                        <q-td>
                            <span class="col-grow text-body1">{{ props.row.productName }}</span>
                        </q-td>
                        <q-td>
                            <div class="row no-wrap">
                                <q-btn icon="mdi-playlist-edit"
                                       :label="app.btnLabel('btn_edit')" padding="xs"
                                       class="bg-color-info"
                                       @click="showEdit={show:true,editId:props.row.id}" />
                                <q-btn icon="mdi-cog-outline"
                                       :label="app.btnLabel('btn_actions')" padding="xs"
                                       class="bg-color-dark">
                                    <q-menu auto-close>
                                        <q-list separator dense>
                                            <q-item clickable v-ripple @click="deleteItem(props.row.id)">
                                                <q-item-section>
                                                    <q-icon size="md" name="mdi-delete-outline" />
                                                </q-item-section>
                                                <q-item-section>
                                                    {{ $t("btn_delete") }}
                                                </q-item-section>
                                            </q-item>
                                            <q-item clickable v-ripple>
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
                        </q-td>
                    </q-tr>
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
                                <q-btn icon="mdi-delete-outline"
                                       :label="app.btnLabel('btn_delete')" padding="xs"
                                       class="bg-color-negative" @click="deleteItem(props.row.id)" />
                                <q-btn icon="mdi-playlist-edit"
                                       :label="app.btnLabel('btn_edit')" padding="xs"
                                       class="bg-color-info"
                                       @click="showEdit={show:true,editId:props.row.id}" />
                                <q-btn icon="mdi-cog-outline"
                                       :label="app.btnLabel('btn_actions')" padding="xs"
                                       class="bg-color-dark"
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
            <template v-if="loading">
                <q-inner-loading :showing="loading">
                    <q-spinner color="primary" size="30px"></q-spinner>
                </q-inner-loading>
            </template>
            <template v-if="!loading && !records.length">
                <q-card :flat="$q.screen.gt.md" class="q-mt-md col-grow">
                    <q-card-section>
                        <div class="text-grey text-h6 text-center">
                            {{ $t("noRecordFound") }}
                        </div>
                    </q-card-section>
                </q-card>
            </template>
        </q-scroll-area>
        <div class="q-mt-md flex flex-center">
            <q-pagination
                v-model="currentPage"
                :max="totalPages"
                boundary-links
                direction-links
                input
                @update:model-value="load"
                dense
            />
        </div>
        <q-dialog v-model="showAddOrEdit" persistent>
            <ProductAddEdit />
        </q-dialog>
    </div>
</template>

<script setup>
import { reactive, toRefs, onMounted, watch } from "vue";
import { useApp } from "src/composables/app.js";
import { useList } from "src/composables/list.js";
import ProductAddEdit from "./product-add-edit";

const options = reactive({
    apiPath: "serviceCatalog",
    storeModule: "serviceCatalog",
    pageSize: 50,
    filter: {},
    orderBy: []
});
const props = defineProps({
    service: Object || null
});
const app = useApp();

const page = useList(options);

const {
    totalRecords,
    loading,
    currentPage,
    totalPages
} = toRefs(page.state);

const {
    records,
    showAdd,
    showEdit,
    showAddOrEdit
} = page.computedProps;

const {
    load,
    isCurrentRecord,
    deleteItem
} = page.methods;

watch(() => props.service, (val) => {
    if (val && val.id && val.id !== "") {
        load();
    }
});
</script>
