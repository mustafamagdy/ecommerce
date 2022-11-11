<!-- <template>
    <div class="row col-grow items-stretch justify-start">
        <view-boxes class="col-grow panel" />
    </div>
</template>

<script setup>
import ViewBoxesList from "./ViewBoxes.vue";
</script> -->

<template>
    <div class="column q-gutter-md">
        <div class="row justify-around items-center">
            <span>{{ $t("Boxes_Categories") }} : {{ selectedCategorie }}</span>
            <span>{{ $t("count") }} : {{ page.totalRecords }}</span>
            <span>{{ $t("pages") }} : {{ page.totalPages }}</span>

            <div class="row no-wrap">
                <q-btn icon="mdi-magnify" padding="xs" class="bg-color-dark" />
                <q-btn icon="mdi-plus" padding="xs" class="bg-color-primary" @click="page.showAdd = true" />
            </div>
        </div>

        <q-separator class="q-mt-sm" />
        <div class="row flex flex-center">
            <div class="no-wrap">
                <q-radio v-model="selectedCategorie" checked-icon="task_alt" unchecked-icon="panorama_fish_eye" val="all" label="All" />
                <q-radio v-model="selectedCategorie" checked-icon="task_alt" unchecked-icon="panorama_fish_eye" val="boxes" label="Boxes" />
                <q-radio
                    v-model="selectedCategorie"
                    checked-icon="task_alt"
                    unchecked-icon="panorama_fish_eye"
                    val="bank_accounts"
                    label="Bank accounts"
                />
            </div>
        </div>
        <div class="row justify-around">
            <div class="q-ma-sm" v-for="record in page.records" :key="record.id" style="min-width: 250px">
                <q-card class="my-card text-center" to="financialBoxes-transactions" clickable @click="setClickedRecord(record)">
                    <q-card-section class="column">
                        <q-item>
                            <q-item-section avatar>
                                <q-icon v-if="record.typeOfBox === 'box'" color="primary" name="inventory" />
                                <q-icon v-else-if="record.typeOfBox === 'bank account'" color="primary" name="account_balance" />
                            </q-item-section>
                            <q-item-section>
                                <q-item-label>{{ record.boxName }}</q-item-label>
                            </q-item-section>
                        </q-item>
                        <q-item-label class="q-mx-md"> {{ $t("Manager") }} : {{ record.managerName }} </q-item-label>
                        <q-separator class="q-mt-md" inset />
                    </q-card-section>

                    <div class="row items-center justify-between no-wrap">
                        <div class="text-overline q-mx-md text-blue-9">
                            {{ record.typeOfBox }}
                        </div>
                        <div class="row">
                            <q-btn
                                icon="mdi-playlist-edit"
                                padding="xs"
                                class="bg-color-info"
                                @click="page.showEdit = { show: true, editId: record.id }"
                            />
                            <q-btn icon="mdi-cog-outline" padding="xs" class="bg-color-dark">
                                <q-menu auto-close>
                                    <q-list separator dense>
                                        <q-item clickable v-ripple @click="page.deleteItem(record.id)">
                                            <q-item-section>
                                                <q-icon size="md" name="mdi-delete-outline" />
                                            </q-item-section>
                                            <q-item-section>
                                                {{ $t("delete") }}
                                            </q-item-section>
                                        </q-item>
                                        <q-item clickable v-ripple>
                                            <q-item-section>
                                                <q-icon
                                                    size="md"
                                                    :name="record.isActive ? 'mdi-pause-circle-outline' : 'mdi-play-circle-outline'"
                                                ></q-icon>
                                            </q-item-section>
                                            <q-item-section>
                                                {{ record.isActive ? $t("deactivate") : $t("activate") }}
                                            </q-item-section>
                                        </q-item>
                                        <q-item clickable v-ripple>
                                            <q-item-section>
                                                <q-icon size="md" name="history" />
                                            </q-item-section>
                                            <q-item-section>
                                                {{ $t("record") }}
                                            </q-item-section>
                                        </q-item>
                                        <q-item clickable v-ripple to="financialBoxesTransactions">
                                            <q-item-section>
                                                <q-icon size="md" name="info_outline" />
                                            </q-item-section>
                                            <q-item-section>
                                                {{ $t("Transactions") }}
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
                    </div>
                </q-card>
            </div>
        </div>
        <q-dialog v-model="page.showAddOrEdit"> <financialBoxesAddEdit /></q-dialog>
    </div>
</template>

<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import router from "src/router";
import { onMounted, reactive, ref } from "vue-demi";
import financialBoxesAddEdit from "./financialBoxes-add-edit.vue";
import financialBoxesTransactions from "./financialBoxes-transactions.vue";

const selectedCategorie = ref("all");

const page = reactive(
    useCRUDList({
        apiPath: serverApis.financialBoxes,
        storeModule: storeModules.financialBoxes,
        pageSize: 9,
    })
);
const setClickedRecord = (record) => {
    console.log("Method");
    page.clickedRecord = record;
    console.log(page.clickedRecord);
};
onMounted(() => {
    page.load();
});
</script>
