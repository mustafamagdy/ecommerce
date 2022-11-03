<template>
    <div class="column justify-between q-mb-sm col-grow">
        <div class="colum">
            <div class="row justify-between">
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("Name : ") }} </span>
                    <q-select :options="options" transition-show="scale" transition-hide="scale" label="Names" />
                </div>
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("Mobile number") }} </span>
                    <q-input readonly :label="$t('mobile-no')" />
                </div>
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("Subscribe from :") }} </span>
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
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("To : ") }} </span>
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
            </div>
        </div>
        <div class="column">
            <div class="row justify-between">
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("Balance from :") }} </span>
                    <q-input readonly :label="$t('from_date')" class="q-mr-sm" />
                </div>
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("To: ") }} </span>
                    <q-input readonly :label="$t('to_date')" />
                </div>
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("Status : ") }} </span>
                    <div class="q-gutter-sm">
                        <q-radio v-model="status" checked-icon="task_alt" unchecked-icon="panorama_fish_eye" val="active" label="Active" />
                        <q-radio
                            v-model="status"
                            checked-icon="task_alt"
                            unchecked-icon="panorama_fish_eye"
                            val="inactive"
                            label="Inactive"
                        />
                        <q-radio v-model="status" checked-icon="task_alt" unchecked-icon="panorama_fish_eye" val="all" label="All" />
                    </div>
                </div>
            </div>
        </div>
        <div class="column">
            <div class="row justify-between items-center">
                <span>{{ $t("count") }} : {{ page.totalRecords }}</span>
                <span>{{ $t("pages") }} : {{ page.totalPages }}</span>
                <div class="row items-center q-ma-sm">
                    <q-btn icon="mdi-magnify" class="bg-color-dark" />
                    <q-btn icon="print" padding="xs" class="bg-color-info" />
                    <q-btn icon="forward" padding="xs" class="bg-color-primary" />
                </div>
            </div>
        </div>
        <q-separator class="q-mt-sm" inset />
        <q-scroll-area class="col-grow panel" visible>
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
                                <span class="col-grow text-body1">{{ props.row.id }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.name }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.Tel }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.email }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.subscriptionDate }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.status }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.recentPackage }}</span>
                            </q-td>
                            <q-td>
                                <span class="col-grow text-body1">{{ props.row.balance }}</span>
                            </q-td>
                            <q-td>
                                <div class="row no-wrap">
                                    <q-btn
                                        icon="preview"
                                        padding="xs"
                                        class="bg-color-info"
                                        @click="page.showEdit = { show: true, editId: props.row.id }"
                                    />
                                    <q-btn icon="mdi-cog-outline" padding="xs" class="bg-color-dark">
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
                v-model="page.currentPage"
                :max="page.totalPages"
                boundary-links
                direction-links
                input
                @update:model-value="page.load"
            />
        </div>
        <q-dialog v-model="page.showAddOrEdit" persistent> <create-box /> </q-dialog>
    </div>
</template>
<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive, ref } from "vue-demi";

const page = reactive(
    useCRUDList({
        apiPath: serverApis.subscriptions,
        storeModule: storeModules.subscriptions,
        pageSize: 8,
    })
);
const status = ref("all");
const date = ref("2022/11/03");
const options = ["subscriper 1 name", "subscriper 2 name", "subscriper 3 name", "subscriper 4 name", "subscriper 5 name"];
onMounted(() => {
    page.load();
});
</script>
