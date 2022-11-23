<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="column">
            <div class="q-ma-sm text-center">
                <div class="text-h5 q-pa-md">{{ $t(" Add new Customer") }}</div>

                <div class="column q-pa-sm" style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()" class="q-gutter-md text-right">
                        <div class="row justify-between no-wrap">
                            <q-input class="q-ma-sm" filled v-model.trim="formData.firstName" :label="$t('first_name')"></q-input>
                            <q-input class="q-ma-sm" filled v-model.trim="formData.lastName" :label="$t('last_name')"></q-input>
                        </div>
                        <div class="column">
                            <q-input class="q-ma-sm" filled v-model.trim="formData.email" :label="$t('email')"></q-input>
                        </div>
                        <div class="column">
                            <q-input class="q-ma-sm" filled v-model.trim="formData.phoneNumber" :label="$t('phone')"></q-input>
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
    userName: "",
    firstName: "",
    lastName: "",
    email: "",
    isActive: "",
    emailConfirmed: "",
    phoneNumber: "",
    imageUrl: "",
};
const app = useApp();
const formData = reactive({ ...formInputs });

function beforeSubmit() {
    delete formData.imageUrl;
    // formData.imageFile.name = uid();
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
    firstName: { required: required, maxLength: maxLength(75) },
    lastName: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, formData);

const page = useAddEditPage({
    apiPath: serverApis.customers,
    storeModule: storeModules.customers,
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
