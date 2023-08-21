<template>
    <div class="column justify-around q-mb-sm col-grow">
        <div class="row justify-between items-center no-wrap">
            <span>{{ $t("count") }} : {{ page.totalRecords }}</span>
            <span>{{ $t("pages") }} : {{ page.totalPages }}</span>
            <div class="row">
                <q-btn icon="mdi-magnify" padding="xs" class="bg-color-dark">
                    <!-- <q-menu persistent style="width: 50%">
                        <serviceSearch />
                    </q-menu> -->
                </q-btn>
                <q-btn icon="mdi-plus" padding="xs" class="bg-color-primary" @click="(page.showAdd = true), (status = 'new')" />
            </div>
        </div>
        <q-separator class="q-mt-sm" inset />
        <q-scroll-area class="col-grow panel" visible>
            <div class="row q-pa-md items-start justify-around q-gutter-md">
                <div class="q-ma-sm" v-for="record in page.records" :key="record.id" style="min-width: 250px">
                    <q-card class="my-card items-start row text-center" clickable>
                        <q-card-section class="column items-center" horizontal>
                            <q-item class="column">
                                <q-item-section avatar>
                                    <q-avatar>
                                        <img :src="record.imageUrl" />
                                    </q-avatar>
                                </q-item-section>
                            </q-item>
                            <q-item class="column">
                                {{ record.status }}
                                <q-separator
                                    :color="
                                        record.status === 'active'
                                            ? 'green'
                                            : record.status === 'closed'
                                            ? 'red'
                                            : record.status === 'approved'
                                            ? 'blue'
                                            : 'orange'
                                    "
                                />
                            </q-item>
                        </q-card-section>
                        <q-item-section class="column q-ma-sm items-center">
                            <div class="row">
                                <div class="q-ma-sm">{{ record.name }}</div>
                                <div class="q-ma-sm">{{ record.branch }}</div>
                            </div>
                            <div class="row">
                                <div class="q-ma-sm">{{ record.employeeName }}</div>
                            </div>
                            <div class="row">
                                <div class="q-ma-sm">{{ record.date }}</div>
                                <div class="q-ma-sm">{{ record.time }}</div>
                            </div>
                            <div class="row">
                                <q-btn
                                    icon="mdi-playlist-edit"
                                    padding="xs"
                                    class="bg-color-info"
                                    @click="(page.showEdit = { show: true, id: record.id }), (status = record.status)"
                                />
                                <q-btn icon="mdi-cog-outline" padding="xs" class="bg-color-dark">
                                    <q-menu auto-close>
                                        <q-list separator dense>
                                            <q-item clickable v-ripple @click="changeStatusAndDate(record)">
                                                <q-item-section>
                                                    <q-icon
                                                        size="md"
                                                        :name="
                                                            record.status === 'new'
                                                                ? 'play_circle'
                                                                : record.status === 'active'
                                                                ? 'cancel'
                                                                : record.status === 'closed'
                                                                ? 'check_box'
                                                                : 'recommend'
                                                        "
                                                    ></q-icon>
                                                </q-item-section>
                                                <q-item-section>
                                                    {{
                                                        record.status === "new"
                                                            ? $t("activate")
                                                            : record.status === "active"
                                                            ? "close"
                                                            : record.status === "closed"
                                                            ? "approve"
                                                            : "approved"
                                                    }}
                                                </q-item-section>
                                            </q-item>
                                            <q-item clickable v-ripple @click="goToPointOfSalesPage()">
                                                <q-item-section>
                                                    <q-icon size="md" name="login" />
                                                </q-item-section>
                                                <q-item-section>
                                                    {{ $t("pointOfSales") }}
                                                </q-item-section>
                                            </q-item>
                                            <q-item clickable v-ripple>
                                                <q-item-section>
                                                    <q-icon size="md" name="summarize" />
                                                </q-item-section>
                                                <q-item-section>
                                                    {{ $t("reports") }}
                                                </q-item-section>
                                            </q-item>
                                            <q-item clickable v-ripple @click="page.deleteItem(record.id)">
                                                <q-item-section>
                                                    <q-icon size="md" name="mdi-delete-outline" />
                                                </q-item-section>
                                                <q-item-section>
                                                    {{ $t("delete") }}
                                                </q-item-section>
                                            </q-item>
                                        </q-list>
                                    </q-menu>
                                </q-btn>
                            </div>
                        </q-item-section>
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
        <q-dialog v-if="status === 'new'" v-model="page.showAddOrEdit" persistent>
            <pointOfSalesSessionsAddEditNew :showAdd="page.showAdd" />
        </q-dialog>
        <q-dialog v-if="status === 'active' || status === 'closed' || status === 'approved'" v-model="page.showAddOrEdit" persistent>
            <pointOfSalesAddEditForActiveOne :showAdd="page.showAdd" />
        </q-dialog>
    </div>
</template>
<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { useStatusChange } from "src/composables/useStatusChange";
import { serverApis, storeModules } from "src/enums";
import { Notify } from "quasar";
import { useRouter } from "vue-router";
import { onMounted, reactive, ref } from "vue-demi";
import pointOfSalesSessionsAddEditNew from "./point-of-sales-sessions-add-edit-new.vue";
import pointOfSalesAddEditForActiveOne from "./point-of-sales-sessions-add-edit-for-activeOne.vue";
const router = useRouter();
const goToPointOfSalesPage = () => {
    router.push("/pointOfSales");
};
const status = ref("new");
const { changeStatusAndDate } = useStatusChange();
const page = reactive(
    useCRUDList({
        apiPath: serverApis.pointOfSalesSessions,
        storeModule: storeModules.pointOfSalesSessions,
        pageSize: 9,
    })
);
onMounted(() => {
    page.load();
});
</script>
