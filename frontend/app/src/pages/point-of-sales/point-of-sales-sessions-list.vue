<template>
    <div class="column justify-around q-mb-sm col-grow">
        <div class="row justify-between items-center no-wrap">
            <span>{{ $t("count") }} : {{ page.totalRecords }}</span>
            <span>{{ $t("pages") }} : {{ page.totalPages }}</span>
            <div class="row">
                <q-btn icon="mdi-magnify" padding="xs" class="bg-color-dark">
                    <q-menu persistent style="width: 50%">
                        <serviceSearch />
                    </q-menu>
                </q-btn>
                <q-btn icon="mdi-plus" padding="xs" class="bg-color-primary" @click="page.showAdd = true" />
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
                                {{ record.isActive ? $t("Active") : $t("Deactive") }}
                                <q-separator :color="record.isActive ? 'green' : 'red'" inset class="full-width" />
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
                                    @click="page.showEdit = { show: true, id: record.id }"
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
        <!-- <q-dialog v-model="page.showAddOrEdit" persistent> <pointOfSalesSessionsAddEdit :showAdd="page.showAdd" /> </q-dialog> -->
    </div>
</template>
<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive } from "vue-demi";
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
