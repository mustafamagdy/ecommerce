<template>
    <div class="row col-grow items-stretch justify-start">
        <div class="col-12 col-sm-6 column q-pa-sm">
            <services-list class="col-grow panel"/>
        </div>
        <div class="col-12 col-sm-6 column q-pa-sm">
            <service-catalog-list class="col-grow panel" :service="clickedService"/>
        </div>
    </div>
</template>

<script setup>
import ServicesList from "./services-list.vue";
import ServiceCatalogList from "./serviceCatalog-list";
import {useList} from "src/composables/list.js";

const page = useList({storeModule: "services"});
const {clickedRecord:clickedService} = page.computedProps;
</script>
