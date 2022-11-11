<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="column">
            <div class="q-ma-sm text-center">
                <div class="text-h5 q-pa-md">{{ $t(" Add new Customer") }}</div>

                <div class="column q-pa-sm" style="min-width: 400px">
                    <q-form @submit="onSubmit" @reset="onReset" class="q-gutter-md text-right">
                        <div class="row justify-between no-wrap">
                            <q-input
                                class="q-ma-sm"
                                filled
                                v-model="formData.firstName"
                                :label="$t('first_name')"
                                lazy-rules
                                :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                            />
                            <q-input
                                class="q-ma-sm"
                                filled
                                v-model="formData.lastName"
                                :label="$t('last_name')"
                                lazy-rules
                                :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                            />
                        </div>
                        <div class="column">
                            <q-input
                                class="q-ma-sm"
                                filled
                                v-model="formData.email"
                                :label="$t('email')"
                                lazy-rules
                                :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                            />
                        </div>
                        <div class="column">
                            <q-input
                                class="q-ma-sm"
                                filled
                                v-model="formData.phoneNumber"
                                :label="$t('phoneNumber')"
                                lazy-rules
                                :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                            />
                        </div>

                        <div class="row flex-center no-wrap q-my-md">
                            <q-btn
                                :label="$t('save')"
                                icon="mdi-content-save-outline"
                                type="submit"
                                :loading="saving"
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

const options = reactive({
    apiPath: serverApis.customers,
    storeModule: storeModules.customers,
    formInputs: {
        id: "",
        userName: "",
        firstName: "",
        lastName: "",
        email: "",
        isActive: "",
        emailConfirmed: "",
        phoneNumber: "",
        imageUrl: "",
    },
});
const app = useApp();
const formData = reactive({ ...options.formInputs });

function beforeSubmit() {
    delete formData.imageUrl;
    formData.imageFile.name = uid();
    if (showAdd.value) delete formData.id;
    return true;
}

const onFormSubmitted = (data) => {
    if (showAdd.value) {
        let record = { id: data, ...formData };
        addRecordToList(record);
    } else if (showEdit.value) {
        updateRecordInList(formData);
    }
};

const rules = {
    name: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, formData); // form validation

const page = useAddEditPage(options, formData, v$, onFormSubmitted, beforeSubmit);

const { saving } = toRefs(page.state);
const { showAdd, showEdit } = page.computedProps;
const { submitForm, getFieldErrorsMsg, addRecordToList, updateRecordInList } = page.methods;

useMeta(() => {
    return {
        title: $t("addNewSampletable"),
    };
});

onMounted(() => {
    page.load();
});

defineExpose({
    page,
});
</script>
<style></style>
