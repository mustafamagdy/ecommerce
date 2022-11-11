<template>
    <div class="col-grow column items-stretch justify-start">
        <q-scroll-area class="col-grow panel" visible>
            <financialBoxesList />
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
    </div>
</template>

<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive, ref } from "vue-demi";
import financialBoxesAddEdit from "./financialBoxes-add-edit.vue";
import financialBoxesList from "./financialBoxes-list.vue";

const selectedCategorie = ref("all");

const page = reactive(
    useCRUDList({
        apiPath: serverApis.financialBoxes,
        storeModule: storeModules.financialBoxes,
        pageSize: 9,
    })
);

onMounted(() => {
    page.load();
});
</script>
