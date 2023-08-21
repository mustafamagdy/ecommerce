<template>
    <div class="row no-wrap items-center">
        <div class="col-grow">
            <div class="col-12 q-mb-sm">
                <q-input v-model.trim="filters.name" :label="$t('stakeholder-name')" />
            </div>
            <div class="col-12 ">
                <q-input v-model.trim="filters.phoneNumber" :label="$t('stakeholder-phoneNumber') " />
            </div>
        </div>
        <div class="q-pl-sm">
            <q-btn class="bg-color-info" @click="search">
                <div class="column items-center q-py-sm">
                    <q-icon name="mdi-magnify" class="q-mr-sm" size="30px" />
                    <span>{{ $t("btn-search") }}</span>
                </div>
            </q-btn>
        </div>
    </div>
</template>
<script setup>

import { reactive } from "vue";
import { useList } from "src/composables/useList";

const list = useList({
    apiPath: ""
});
const filters = reactive({
    name: "",
    phoneNumber: ""
});

const search = () => {

};
</script>
