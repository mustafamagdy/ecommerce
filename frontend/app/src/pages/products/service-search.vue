<template>
    <div class="column q-pa-md">
        <q-form ref="observer" @submit.prevent="page.submitForm()">
            <span class="label q-mx-md">{{ $t("service_name ") }}</span>
            <div class="col q-mx-md q-my-sm">
                <q-input v-model.trim="searchCriteria.serviceName" :error="v$.serviceName.$error" :hint="$t('Like: service 1')"></q-input>
            </div>
            <div class="row flex-center no-wrap">
                <q-btn :label="$t('btn-search')" icon="mdi-content-save-outline" class="bg-color-positive" />
                <q-btn :label="$t('btn-close')" icon="mdi-close" class="bg-color-light" v-close-popup />
            </div>
        </q-form>
    </div>
</template>

<script setup>
import { reactive, ref } from "vue";
import { maxLength, required } from "@vuelidate/validators";
import { useList } from "src/composables/useList";
import useVuelidate from "@vuelidate/core";
import { storeModules } from "src/enums";

const searchCriteria = reactive({
    serviceName: "",
});
const rules = {
    serviceName: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, searchCriteria);
const page = reactive(useList({ storeModule: storeModules.services }));
</script>
