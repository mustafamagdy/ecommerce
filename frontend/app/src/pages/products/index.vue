<template>
    <div class="row col-grow items-stretch justify-start">
        <div class="col-12 col-sm-6 column q-pa-sm">
            <services-list class="col-grow panel" />
        </div>
        <div class="col-12 col-sm-6 column q-pa-sm">
            <service-catalog-list class="col-grow panel" :clickedServiceId="clickedServiceId" />
        </div>
    </div>
</template>

<script setup>
import ServicesList from "./services-list.vue";
import ServiceCatalogList from "./serviceCatalog-list";
import { useList } from "src/composables/list.js";
import { useStore } from "vuex";
import { computed, ref, watch } from "vue";
const store = useStore();

const page = useList({ storeModule: "services" });

console.log(store.getters[`services/clickedRecordId`]);
let clickedServiceId = ref("0");
clickedServiceId = computed({
    get() {
        return store.getters[`services/clickedRecordId`];
    },
});
// watch(clickedServiceId, (currentValue, oldValue) => {
//     console.log(currentValue);
// });
// clickedServiceId = computed(() => store.getters["services/clickedRecordId"]);
</script>
