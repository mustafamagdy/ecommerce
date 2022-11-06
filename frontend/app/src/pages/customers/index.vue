<template>
    <div class="column justify-around q-mb-sm col-grow">
        <div class="colum">
            <div class="row justify-between no-wrap">
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("Customer name") }} </span>
                    <q-input readonly :label="$t('customer-name')" class="q-mr-sm" />
                </div>
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("Mobile number") }} </span>
                    <q-input readonly :label="$t('mobile-no')" />
                </div>
                <div class="row items-center q-ma-sm">
                    <span class="q-mr-sm">{{ $t("Pill number") }} </span>
                    <q-input class="q-mr-sm" readonly :label="$t('pill-no')" />
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
                    <q-btn icon="mdi-magnify" class="bg-color-dark" />
                    <q-btn icon="mdi-plus" padding="xs" class="bg-color-primary" @click="page.showAdd = true" />
                </div>
            </div>
        </div>
        <div class="column">
            <div class="row justify-between items-center">
                <span>{{ $t("count") }} : {{ page.totalRecords }}</span>
                <span>{{ $t("pages") }} : {{ page.totalPages }}</span>
            </div>
        </div>
        <q-separator class="q-mt-sm" inset />
        <q-scroll-area class="col-grow panel" visible>
            <div class="row q-pa-md items-start justify-around q-gutter-md">
                <div class="q-ma-sm" v-for="record in page.records" :key="record.id" style="min-width: 250px">
                    <q-card class="my-card items-start row text-center" clickable>
                        <q-card-section class="column" horizontal>
                            <q-item>
                                <q-item-section avatar>
                                    <q-avatar>
                                        <img :src="record.imageUrl" />
                                    </q-avatar>
                                </q-item-section>
                                <q-item-section>
                                    <q-item-label class="q-mb-sm">{{ record.firstName }} {{ record.lastName }}</q-item-label>
                                    <q-item-label class="q-mb-sm">{{ record.phoneNumber }}</q-item-label>
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
                                                                :name="
                                                                    record.isActive ? 'mdi-pause-circle-outline' : 'mdi-play-circle-outline'
                                                                "
                                                            ></q-icon>
                                                        </q-item-section>
                                                        <q-item-section>
                                                            {{ record.isActive ? $t("deactivate") : $t("activate") }}
                                                        </q-item-section>
                                                    </q-item>
                                                    <q-item clickable v-ripple to="bills">
                                                        <q-item-section>
                                                            <q-icon size="md" name="receipt_long" />
                                                        </q-item-section>
                                                        <q-item-section>
                                                            {{ $t("invoices") }}
                                                        </q-item-section>
                                                    </q-item>
                                                </q-list>
                                            </q-menu>
                                        </q-btn>
                                    </div>
                                </q-item-section>
                            </q-item>
                        </q-card-section>
                        <q-separator vertical inset />

                        <q-item> <div>balance</div></q-item>
                    </q-card>
                </div>
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
        <q-dialog v-model="page.showAddOrEdit" persistent> <customersAddEdit /> </q-dialog>
    </div>
</template>
<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive } from "vue-demi";
import customersAddEdit from "./customersAddEdit.vue";
const page = reactive(
    useCRUDList({
        apiPath: serverApis.customers,
        storeModule: storeModules.customers,
        pageSize: 9,
    })
);
onMounted(() => {
    page.load();
});
</script>
