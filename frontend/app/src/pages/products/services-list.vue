<template>
    <div class="column justify-around">
        <div class="row justify-between items-center no-wrap">
            <span>{{ $t("Services") }}</span>
            <span>{{ $t("count") }} : {{ page.totalRecords }}</span>
            <span>{{ $t("pages") }} : {{ page.totalPages }}</span>
            <div class="row">
                <q-btn icon="mdi-magnify" class="bg-color-dark" />
                <q-btn icon="mdi-plus" padding="xs" class="bg-color-primary" @click="page.showAdd = true" />
            </div>
        </div>

        <q-separator class="q-mt-sm" inset />

        <q-scroll-area class="col-grow panel" visible>
            <div class="row items-start justify-around">
                <div v-for="record in page.records" :key="record.id" style="max-width: 200px">
                    <q-card class="my-card items-start row text-center q-mb-sm" clickable @click="page.clickedRecord = record">
                        <q-card-section class="column" horizontal>
                            <q-item>
                                <q-item-section avatar>
                                    <q-avatar>
                                        <img :src="record.imageUrl" />
                                    </q-avatar>
                                </q-item-section>
                                <q-item-section>
                                    <q-item-label class="q-mb-sm">{{ record.name }} </q-item-label>
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
                                                    <q-item clickable v-ripple @click="page.deleteItem(props.row.id)">
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
                                                                    record.isActive ? 'mdi-pause-circle-outline' : 'mdi-play-circle-outline'
                                                                "
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
        <q-dialog v-model="page.showAddOrEdit" persistent>
            <ServicesAddEdit :showAdd="page.showAdd" />
        </q-dialog>
    </div>
</template>

<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive } from "vue-demi";
import ServicesAddEdit from "./services-add-edit";
import { useApp } from "src/composables/app.js";
import { useStore } from "vuex";

const store = useStore();
const app = useApp();
const page = reactive(
    useCRUDList({
        apiPath: serverApis.services,
        storeModule: storeModules.services,
    })
);

onMounted(() => {
    page.load();
});
</script>
