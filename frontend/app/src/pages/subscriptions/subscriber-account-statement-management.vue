<template>
    <div class="column justify-between q-mb-sm col-grow">
        <div class="column">
            <!-- <q-scroll-area class="col-grow panel" visible> -->
            <div class="row q-pa-md justify-around q-gutter-md">
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
                >
                    <template v-slot:body="props">
                        <q-tr @click="page.clickedRecord = props.row">
                            <q-td> </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.billNum }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.billDate }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.subscriptionPeriod }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.package }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.subscriptionDate }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.amount }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.paid }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.rest }}</span>
                            </q-td>
                            <q-td>
                                <div class="row no-wrap">
                                    <q-btn icon="mdi-cog-outline" padding="xs" class="bg-color-dark">
                                        <q-menu auto-close>
                                            <q-list separator dense>
                                                <q-item clickable v-ripple @click="page.deleteItem(props.row.id)">
                                                    <q-item-section>
                                                        <q-icon size="md" name="notifications" />
                                                    </q-item-section>
                                                    <q-item-section>
                                                        {{ $t("Notice creditor") }}
                                                    </q-item-section>
                                                </q-item>
                                                <q-item clickable v-ripple>
                                                    <q-item-section>
                                                        <q-icon size="md" name="credit_card" />
                                                    </q-item-section>
                                                    <q-item-section> {{ $t("Pay") }} </q-item-section>
                                                </q-item>
                                                <q-item clickable v-ripple>
                                                    <q-item-section>
                                                        <q-icon size="md" name="print" />
                                                    </q-item-section>
                                                    <q-item-section>
                                                        {{ $t("Print") }}
                                                    </q-item-section>
                                                </q-item>

                                                <q-item clickable v-ripple>
                                                    <q-item-section>
                                                        <q-icon size="md" name="send" />
                                                    </q-item-section>
                                                    <q-item-section>
                                                        {{ $t("Send") }}
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
            <!-- </q-scroll-area> -->
            <div class="column q-mt-md flex flex-center">
                <q-pagination
                    v-model="page.currentPage"
                    :max="page.totalPages"
                    boundary-links
                    direction-links
                    input
                    @update:model-value="page.load"
                />
            </div>
            <q-markup-table :separator="separator" flat bordered class="q-ma-md">
                <thead>
                    <tr>
                        <th class="text-center"></th>
                        <th class="text-center">{{ $t("amount") }}</th>
                        <th class="text-center">{{ $t("paid") }}</th>
                        <th class="text-center">{{ $t("rest") }}</th>
                        <th class="text-center">{{ $t("unbilled") }}</th>
                        <th class="text-center">{{ $t("Period From") }}</th>
                        <th class="text-center">{{ $t("To") }}</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td class="text-center">{{ $t("Total") }}</td>
                        <td class="text-center"></td>
                        <td class="text-center"></td>
                        <td class="text-center"></td>
                        <td class="text-center"></td>
                        <td class="text-center"></td>
                    </tr></tbody
            ></q-markup-table>
        </div>
    </div>
</template>
<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive, ref } from "vue-demi";
const names = ref("Names");
const page = reactive(
    useCRUDList({
        apiPath: serverApis.subscriptionAccountStatement,
        storeModule: storeModules.subscriptionAccountStatement,
        pageSize: 8,
    })
);
const isActive = ref("all");
const date = ref("2022/11/03");

const options = ["subscriper 1 name", "subscriper 2 name", "subscriper 3 name", "subscriper 4 name", "subscriper 5 name"];
onMounted(() => {
    page.load();
});
</script>
