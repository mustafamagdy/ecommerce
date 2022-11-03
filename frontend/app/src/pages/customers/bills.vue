<template>
    <div class="row col-grow items-stretch justify-start">
        <div class="col-4 column q-pa-sm">
            <div class="col-grow panel"></div>
        </div>
        <div class="col-8 col-grow column q-pa-sm">
            <div class="column col-grow panel">
                <div class="column">
                    <div class="row justify-between items-center">
                        <span>{{ $t("count") }} : {{ page.totalRecords }}</span>
                        <span>{{ $t("pages") }} : {{ page.totalPages }}</span>
                        <div class="row items-center q-ma-sm">
                            <q-btn icon="mdi-magnify" class="bg-color-dark" />
                            <q-btn icon="print" padding="xs" class="bg-color-primary" />
                        </div>
                    </div>
                </div>
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
                                    <span class="col-grow text-body1">{{ props.row.id }}</span>
                                </q-td>
                                <q-td>
                                    <span class="col-grow text-body1">{{ props.row.date }}</span>
                                </q-td>
                                <q-td>
                                    <span class="col-grow text-body1">{{ props.row.total }}</span>
                                </q-td>
                                <q-td>
                                    <span class="col-grow text-body1">{{ props.row.paid }}</span>
                                </q-td>
                                <q-td>
                                    <span class="col-grow text-body1">{{ props.row.rest }}</span>
                                </q-td>
                                <q-td>
                                    <div class="row no-wrap">
                                        <q-btn
                                            icon="preview"
                                            padding="xs"
                                            class="bg-color-info"
                                            @click="page.showEdit = { show: true, editId: props.row.id }"
                                        />
                                        <!-- <q-btn icon="mdi-cog-outline" padding="xs" class="bg-color-dark">
                                            <q-menu auto-close>
                                                <q-list separator dense>
                                                    <q-item clickable v-ripple @click="page.deleteItem(props.row.id)">
                                                        <q-item-section>
                                                            <q-icon size="md" name="mdi-delete-outline" />
                                                        </q-item-section>
                                                        <q-item-section>
                                                            {{ $t("btn_delete") }}
                                                        </q-item-section>
                                                    </q-item>
                                                </q-list>
                                            </q-menu>
                                        </q-btn> -->
                                    </div>
                                </q-td>
                            </q-tr>
                        </template>
                    </q-table>
                </q-scroll-area>
                <div class="column q-pa-md">
                    <q-table title="Totals" dense :rows="rows" :columns="columns" row-key="name" hide-bottom />
                </div>
                <q-dialog v-model="page.showAddOrEdit" persistent>
                    <ServicesAddEdit />
                </q-dialog>
            </div>
        </div>
    </div>
</template>
<script setup>
import { useList } from "src/composables/useList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive } from "vue-demi";

const page = reactive(
    useList({
        apiPath: serverApis.bills,
        storeModule: storeModules.bills,
    })
);

const columns = [
    {
        name: "name",
        required: true,
        label: "Total",
        align: "left",
        field: (row) => row.name,
        sortable: false,
    },
    { name: "All", align: "center", label: "All ", field: "all" },
    { name: "paid", align: "center", label: "Paid", field: "paid" },
    { name: "rest", align: "center", label: "Rest", field: "rest" },
];

const rows = [
    {
        name: "",
        all: 292992929,
        paid: 28828828,
        rest: 20000,
    },
];
onMounted(() => {
    page.load();
});
</script>
