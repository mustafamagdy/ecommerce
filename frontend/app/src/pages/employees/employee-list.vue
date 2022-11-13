<template>
    <div class="column justify-around q-ma-sm">
        <div class="column">
            <div class="column">
                <div class="row justify-between items-center">
                    <span>{{ $t("count") }} : {{ page.totalRecords }}</span>
                    <span>{{ $t("pages") }} : {{ page.totalPages }}</span>
                    <div class="row">
                        <q-btn icon="mdi-magnify" class="bg-color-dark" />
                        <q-btn icon="mdi-plus" padding="xs" class="bg-color-primary" @click="page.showAdd = true" />
                    </div>
                </div>
            </div>
        </div>
        <q-separator class="q-mt-sm" inset />

        <q-scroll-area class="col-grow panel" visible>
            <div class="row q-pa-md items-start justify-around q-gutter-md">
                <div class="q-ma-sm" v-for="record in page.records" :key="record.id" style="min-width: 200px">
                    <q-card class="my-card items-start row text-center" clickable>
                        <q-card-section class="column" horizontal>
                            <q-item>
                                <q-item-section avatar>
                                    <q-avatar>
                                        <img :src="record.imageUrl" />
                                    </q-avatar>
                                </q-item-section>
                                <q-item-section>
                                    <q-item-label class="q-mb-sm">{{ record.employeeName }} </q-item-label>
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
        <q-dialog v-model="page.showAddOrEdit" persistent> <employeeAddEmployee /> </q-dialog>
    </div>
</template>
<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive } from "vue-demi";
import employeeAddEmployee from "./employee-add-employee.vue";
const page = reactive(
    useCRUDList({
        apiPath: serverApis.employees,
        storeModule: storeModules.employees,
        pageSize: 9,
    })
);
onMounted(() => {
    page.load();
});
</script>
