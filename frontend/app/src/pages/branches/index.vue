<template>
    <div class="column justify-around q-mb-sm col-grow">
        <div class="column">
            <div class="row justify-between items-center">
                <span>{{ $t("branches") }}</span>
                <span>{{ $t("count") }} : {{ page.totalRecords }}</span>
                <span>{{ $t("pages") }} : {{ page.totalPages }}</span>
                <div class="row no-wrap">
                    <q-btn icon="mdi-magnify" padding="xs" class="bg-color-dark" />
                    <q-btn icon="mdi-plus" padding="xs" class="bg-color-primary" @click="page.showAdd = true" />
                </div>
            </div>
        </div>
        <q-separator class="q-mt-sm" inset />
        <q-scroll-area class="col-grow panel" visible>
            <div class="row q-pa-sm items-start justify-around q-gutter-md">
                <div class="col-6 col-sm-3 col-md-3 col-lg-3 q-ma-sm" v-for="record in page.records" :key="record.id">
                    <q-card class="my-card items-start row text-center" clickable>
                        <q-card-section class="column">
                            <q-item>
                                <q-item-section avatar>
                                    <q-avatar rounded size="60px">
                                        <img src="https://loremflickr.com/300/300/abstract" />
                                    </q-avatar>
                                </q-item-section>
                                <q-separator vertical inset class="q-mr-md" />
                                <q-item-section>
                                    <q-item-label class="q-mb-sm">{{ record.name }}</q-item-label>
                                    <q-item-section>
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
                                                                        record.isActive
                                                                            ? 'mdi-pause-circle-outline'
                                                                            : 'mdi-play-circle-outline'
                                                                    "
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
                                                        <q-item clickable v-ripple>
                                                            <q-item-section>
                                                                <q-icon size="md" name="info_outline" />
                                                            </q-item-section>
                                                            <q-item-section>
                                                                {{ $t("operations") }}
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
                                    </q-item-section>
                                </q-item-section>
                            </q-item>
                        </q-card-section>
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
        <q-dialog v-model="page.showAddOrEdit" persistent> <create-box /> </q-dialog>
    </div>
</template>
<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive } from "vue-demi";

const page = reactive(
    useCRUDList({
        apiPath: serverApis.branches,
        storeModule: storeModules.branches,
        pageSize: 9,
    })
);
onMounted(() => {
    page.load();
});
</script>
