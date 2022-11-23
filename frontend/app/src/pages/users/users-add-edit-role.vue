<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="column">
            <div class="q-ma-sm text-center">
                <div class="text-h5">{{ $t(" Add new role") }}</div>

                <div class="column q-pa-sm" style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()" class="text-right">
                        <div class="row flex flex-center">
                            <q-input
                                class="q-ma-sm flex full-width"
                                filled
                                v-model.trim="formData.roleTitle"
                                :label="$t('role-Name')"
                                :error="v$.roleTitle.$error"
                                :error-message="page.getFieldErrorsMsg(v$.roleTitle)"
                            />
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
                            <q-btn :label="$t('close')" icon="mdi-close" class="bg-color-light" v-close-popup />
                        </div>
                    </q-form>
                </div>
            </div>
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
import ImagePicker from "src/components/image-picker";
import { useAddEditPage } from "src/composables/useAddEditPage.js";
import { serverApis, storeModules } from "src/enums";

const formInputs = {
    id: "",
    roleTitle: "",
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
    roleTitle: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, formData);

const page = useAddEditPage({
    apiPath: serverApis.roles,
    storeModule: storeModules.roles,
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
<style></style>
