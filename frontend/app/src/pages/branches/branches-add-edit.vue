<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="text-h5 q-pa-md">{{ $t(" Add new Branch") }}</div>

        <div class="column q-pa-sm" style="min-width: 400px">
            <q-form ref="observer" @submit.prevent="page.submitForm()">
                <div class="row q-ma-md">
                    <div class="col">
                        <q-input filled v-model.trim="formData.name" :label="$t('branch_name')"></q-input>
                    </div>
                </div>
                <div class="row q-ma-md">
                    <div class="col">
                        <q-input v-model.trim="formData.description" type="textarea"></q-input>
                    </div>
                </div>
                <div class="row flex-center no-wrap q-my-md">
                    <q-btn
                        :label="$t('btn_save')"
                        icon="mdi-content-save-outline"
                        type="submit"
                        :loading="pageState.saving"
                        class="bg-color-positive"
                    >
                        <template v-slot:loading>
                            <q-spinner-oval />
                        </template>
                    </q-btn>
                    <q-btn :label="$t('btn_close')" icon="mdi-close" class="bg-color-light" v-close-popup />
                </div>
            </q-form>
        </div>
    </q-card>
</template>
<script setup>
import { computed, ref, reactive, toRefs, onMounted } from "vue";
import useVuelidate from "@vuelidate/core";
import { required, maxLength } from "@vuelidate/validators";
import { uid, useMeta } from "quasar";
import { useApp } from "src/composables/app.js";
import { $t } from "src/services/i18n";
import { useAddEditPage } from "src/composables/useAddEditPage.js";
import { serverApis, storeModules } from "src/enums";

const formInputs = {
    id: "",
    name: "",
    description: "",
};
const app = useApp();
const formData = reactive({ ...formInputs });

function beforeSubmit() {
    formData.id = uid();
    if (page.showAdd.value) delete formData.id;
    return true;
}

const onFormSubmitted = (data) => {
    if (page.showAdd.value) {
        let record = { id: data, ...formData };
        page.addRecordToList(record);
    } else if (page.showEdit.value) {
        page.updateRecordInList(formData);
    }
};

const rules = {
    name: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, formData);

const page = useAddEditPage({
    apiPath: serverApis.branches,
    storeModule: storeModules.branches,
    formInputs,
    formData,
    v$,
    onFormSubmitted,
    beforeSubmit,
});
const pageState = reactive({ ...page.state });

onMounted(() => {
    page.load();
});
</script>
