<template>
    <q-card class="q-pa-sm">
        <div class="text-h5 q-pa-md">{{ props.showAdd ? $t("Add new Branch") : $t("Edit Branch") }}</div>
        <div class="column" style="min-width: 400px">
            <q-form ref="observer" @submit.prevent="page.submitForm()">
                <div class="row q-ma-sm">
                    <div class="col">
                        <span class="label q-mx-sm">{{ $t("Branch_name :") }}</span>
                        <q-input class="q-ma-sm" v-model.trim="formData.name"></q-input>
                    </div>
                </div>
                <div class="row q-ma-sm">
                    <div class="col">
                        <span class="label q-mx-sm">{{ $t("Description :") }}</span>
                        <q-input class="q-ma-sm" v-model.trim="formData.description" type="textarea"></q-input>
                    </div>
                </div>
                <div class="row flex-center no-wrap q-mb-md">
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
    if (page.showAdd.value) delete formData.id;
    return true;
}

const onFormSubmitted = (data) => {
    if (page.showAdd.value) {
        let record = { id: data, ...formData };
        page.addRecordToList(record);
    } else if (page.showEdit.value) {
        page.updateRecordInList({ ...formData });
    }
};

const rules = {
    name: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, formData);
const props = defineProps(["showAdd"]);
console.log(props.showAdd);
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
