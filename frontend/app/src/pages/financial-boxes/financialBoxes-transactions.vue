<template>
    <div class="row col-grow items-stretch justify-start">
        <div class="col-4 column col-grow panel q-pa-sm">
            <q-scroll-area class="col-grow panel" visible>
                <financialBoxesList class="col-grow panel" />
            </q-scroll-area>
        </div>

        <div class="col-8 column q-pa-sm">
            <div class="column q-pa-sm col-grow panel">
                <div class="column">
                    <div class="row justify-between no-wrap">
                        <div class="row items-center q-ma-sm no-wrap">
                            <span class="q-mr-sm">{{ $t("from") }} </span>
                            <q-input filled v-model="date" mask="date" :rules="['date']">
                                <template v-slot:append>
                                    <q-icon name="event" class="cursor-pointer">
                                        <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                            <q-date v-model="date">
                                                <div class="row items-center">
                                                    <q-btn v-close-popup label="Close" color="primary" flat />
                                                </div>
                                            </q-date>
                                        </q-popup-proxy>
                                    </q-icon>
                                </template>
                            </q-input>
                        </div>
                        <div class="row items-center q-ma-sm no-wrap">
                            <span class="q-mr-sm">{{ $t("To") }} </span>
                            <q-input filled v-model="date" mask="date" :rules="['date']">
                                <template v-slot:append>
                                    <q-icon name="event" class="cursor-pointer">
                                        <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                            <q-date v-model="date">
                                                <div class="row items-center justify-end">
                                                    <q-btn v-close-popup label="Close" color="primary" flat />
                                                </div>
                                            </q-date>
                                        </q-popup-proxy>
                                    </q-icon>
                                </template>
                            </q-input>
                        </div>
                        <div class="row items-center q-ma-sm no-wrap">
                            <span class="q-mr-sm">{{ $t("Operation") }} </span>
                            <q-select v-model="types" :options="options" transition-show="scale" transition-hide="scale" />
                        </div>
                    </div>
                </div>
                <div class="column">
                    <div class="row justify-between no-wrap">
                        <div class="row items-center q-ma-sm">
                            <q-radio
                                v-model="checkType"
                                checked-icon="task_alt"
                                unchecked-icon="panorama_fish_eye"
                                val="debtor"
                                label="debtor"
                            />
                            <q-radio
                                v-model="checkType"
                                checked-icon="task_alt"
                                unchecked-icon="panorama_fish_eye"
                                val="creditor"
                                label="creditor"
                            />
                            <q-radio v-model="checkType" checked-icon="task_alt" unchecked-icon="panorama_fish_eye" val="all" label="All" />
                        </div>

                        <div class="row">
                            <div class="row q-ma-sm items-center">
                                <span class="q-ma-sm">{{ $t("Rows num") }} </span>
                                <q-select v-model="rowsNum" :options="rowsOptions" transition-show="scale" transition-hide="scale" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="column">
                    <div class="row justify-between items-center">
                        <span>{{ $t("Operations") }} : {{ operationsPage.totalRecords }}</span>
                        <span>{{ $t("pages") }} : {{ operationsPage.totalPages }}</span>

                        <div class="row no-wrap">
                            <q-btn icon="mdi-magnify" class="bg-color-dark" />
                            <q-btn icon="print" padding="xs" class="bg-color-info" />
                            <q-btn icon="forward" padding="xs" class="bg-color-primary" />
                            <q-btn icon="mdi-plus" padding="xs" class="bg-color-primary" @click="operationsPage.showAdd = true" />
                        </div>
                    </div>
                </div>

                <q-separator class="q-mt-sm" inset />
                <q-scroll-area class="col-grow panel" visible>
                    <div class="row q-pa-md justify-around q-gutter-md">
                        <q-table
                            card-container-class="q-col-gutter-md justify-start"
                            :bordered="false"
                            :rows="operationsPage.records"
                            selection="single"
                            row-key="id"
                            :loading="operationsPage.loading"
                            hide-pagination
                            :pagination="{ rowsPerPage: 0 }"
                            dense
                            flat
                        >
                            <template v-slot:body="props">
                                <q-tr @click="operationsPage.clickedRecord = props.row">
                                    <q-td> </q-td>

                                    <q-td>
                                        <span class="col-grow text-body1">{{ props.row.id }}</span>
                                    </q-td>
                                    <q-td>
                                        <span class="col-grow text-body1">{{ props.row.oprertionType }}</span>
                                    </q-td>
                                    <q-td>
                                        <span class="col-grow text-body1">{{ props.row.date }}</span>
                                    </q-td>
                                    <q-td>
                                        <span class="col-grow text-body1">{{ props.row.statement }}</span>
                                    </q-td>
                                    <q-td>
                                        <span class="col-grow text-body1">{{ props.row.debtor }}</span>
                                    </q-td>
                                    <q-td>
                                        <span class="col-grow text-body1">{{ props.row.creditor }}</span>
                                    </q-td>
                                    <q-td>
                                        <div class="row no-wrap">
                                            <q-btn icon="mdi-cog-outline" padding="xs" class="bg-color-dark">
                                                <q-menu auto-close>
                                                    <q-list separator dense>
                                                        <q-item clickable v-ripple @click="operationsPage.deleteItem(props.row.id)">
                                                            <q-item-section>
                                                                <q-icon size="md" name="mdi-delete-outline" />
                                                            </q-item-section>
                                                            <q-item-section>
                                                                {{ $t("btn_delete") }}
                                                            </q-item-section>
                                                        </q-item>
                                                        <q-item clickable v-ripple>
                                                            <q-item-section>
                                                                <q-icon
                                                                    size="md"
                                                                    :name="
                                                                        props.row.isActive
                                                                            ? 'mdi-pause-circle-outline'
                                                                            : 'mdi-play-circle-outline'
                                                                    "
                                                                ></q-icon>
                                                            </q-item-section>
                                                            <q-item-section>
                                                                {{ props.row.isActive ? $t("deactivate") : $t("activate") }}
                                                            </q-item-section>
                                                        </q-item>
                                                        <q-item clickable v-ripple>
                                                            <q-item-section>
                                                                <q-icon size="md" name="login" />
                                                            </q-item-section>
                                                            <q-item-section>
                                                                {{ $t("manager") }}
                                                            </q-item-section>
                                                        </q-item>

                                                        <q-item clickable v-ripple>
                                                            <q-item-section>
                                                                <q-icon size="md" name="highlight_off" />
                                                            </q-item-section>
                                                            <q-item-section>
                                                                {{ $t("close") }}
                                                            </q-item-section>
                                                        </q-item>
                                                    </q-list>
                                                </q-menu>
                                            </q-btn>
                                        </div>
                                    </q-td>
                                </q-tr>
                            </template>
                        </q-table>
                    </div>
                </q-scroll-area>
                <div class="column q-mt-md flex flex-center">
                    <q-pagination
                        v-model="operationsPage.currentPage"
                        :max="operationsPage.totalPages"
                        boundary-links
                        direction-links
                        input
                        @update:model-value="operationsPage.load"
                    />
                </div>
            </div>
        </div>
        <q-dialog v-model="operationsPage.showAddOrEdit"> <financialBoxesAddEditTransaction :showAdd="operationsPage.showAdd" /></q-dialog>
    </div>
</template>

<script setup>
import financialBoxesAddEdit from "./financialBoxes-add-edit.vue";
import financialBoxesList from "./financialBoxes-list.vue";
import financialBoxesAddEditTransaction from "./financialBoxes-add-edit-transaction.vue";
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive, ref } from "vue-demi";
const checkType = ref("all");
const operationsPage = reactive(
    useCRUDList({
        apiPath: serverApis.financialBoxesTransactions,
        storeModule: storeModules.financialBoxesTransactions,
        pageSize: 6,
    })
);

const types = ref("type");
const rowsNum = ref("0");

const options = ["type 1", "type 2", "type 3", "type 4", "type 5"];
const rowsOptions = [" 1", " 2", " 3", " 4", " 5"];

onMounted(() => {
    operationsPage.load();
});
</script>
