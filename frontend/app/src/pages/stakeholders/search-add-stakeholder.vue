<template>
    <q-tabs
        v-model="tab"
    >
        <q-tab name="add" icon="mdi-plus" class="text-primary" :label="$t('btn-add-new')" />
        <q-tab name="search" icon="mdi-magnify" class="text-dark" :label="$t('btn-search')" />
    </q-tabs>
    <q-tab-panels v-model="tab" animated>
        <q-tab-panel name="add">
                <div class="col-12 row">
                    <div class="col-sm-3 col-12">
                        {{ $t("stakeholder-name") }} *
                    </div>
                    <div class="col-sm-9 col-12">
                        <q-input v-model.trim="newStakeholder.name" :error="v$.name.$error">
                        </q-input>
                    </div>
                </div>
                <div class="col-12 row">
                    <div class="col-sm-3 col-12">
                        {{ $t("stakeholder-phoneNumber") }} *
                    </div>
                    <div class="col-sm-9 col-12">
                        <q-input v-model.trim="newStakeholder.phoneNumber" :error="v$.phoneNumber.$error">
                        </q-input>
                    </div>
                </div>
        </q-tab-panel>
        <q-tab-panel name="search">
            <search-stakeholder />
        </q-tab-panel>
    </q-tab-panels>
    <div class="row flex-center no-wrap q-my-md">
        <q-btn :label="$t('btn-save')" icon="mdi-content-save-outline" class="bg-color-positive" />
        <q-btn :label="$t('btn-close')" icon="mdi-close" class="bg-color-light" v-close-popup />
    </div>
</template>

<script setup>
import { reactive, ref } from "vue";
import { maxLength, required } from "@vuelidate/validators";
import useVuelidate from "@vuelidate/core";
import SearchStakeholder from "src/pages/stakeholders/search-stakeholder";

const tab = ref("add");
const newStakeholder = reactive({
    name: "",
    phoneNumber: ""
});
const rules = {
    name: { required: required, maxLength: maxLength(75) },
    phoneNumber: { required: required, maxLength: maxLength(75) }
};
const v$ = useVuelidate(rules, newStakeholder);
</script>
