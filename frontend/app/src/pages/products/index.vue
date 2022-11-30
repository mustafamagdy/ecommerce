<template>
    <div class="row col-grow items-stretch justify-start">
        <div class="col-12 col-sm-6 column q-pa-sm">
            <services-list class="col-grow panel" />
        </div>
        <div class="col-12 col-sm-6 column q-pa-sm">
            <service-catalog-list class="col-grow panel" :service="page.clickedRecord" />
        </div>
    </div>
</template>

<script setup>
import ServicesList from "./services-list.vue";
import ServiceCatalogList from "./serviceCatalog-list";
import { useList } from "src/composables/useList";
import { storeModules } from "src/enums";
import { reactive } from "vue";

const page = reactive(useList({ storeModule: storeModules.services }));
</script>
