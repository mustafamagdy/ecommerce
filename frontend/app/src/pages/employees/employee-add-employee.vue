<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="column">
            <div class="q-ma-sm text-center">
                <div class="text-h5 q-pa-md">{{ $t(" Add new Employee") }}</div>

                <div class="column q-pa-sm" style="min-width: 400px">
                    <q-form @submit="onSubmit" @reset="onReset" class="q-gutter-md text-right">
                        <q-input
                            class="q-ma-sm"
                            filled
                            v-model="formData.employeeName"
                            :label="$t('Employee-name')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />
                        <q-input
                            class="q-ma-sm"
                            filled
                            v-model="formData.username"
                            :label="$t('user-name')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />

                        <q-input
                            class="q-ma-sm"
                            filled
                            v-model="formData.Password"
                            :label="$t('password')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />

                        <q-input
                            class="q-ma-sm"
                            filled
                            v-model="formData.Tel"
                            :label="$t('phoneNumber')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />
                        <q-input
                            class="q-ma-sm"
                            filled
                            v-model="formData.email"
                            :label="$t('E-mail')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />
                        <div class="row flex-center no-wrap q-my-sm">
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
                        </div>
                        <q-field readonly class="q-my-sm">
                            <template v-slot:control>
                                <div class="self-center full-width no-outline">Or choose recent employee</div>
                            </template>
                        </q-field>
                        <q-select filled v-model="formData.employeeName" :options="employeeOptions" :label="$t('choose employee')" />
                        <div class="row flex-center q-my-sm">
                            <q-btn :label="$t('Add-employee')" icon="add" type="submit" :loading="saving" class="bg-color-positive">
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
        employeeName: "",
        username: "",
        Password: "",
        email: "",
        Tel: "",
        isActive: "",
        imageUrl: "",
    },
});
const app = useApp();
const formData = reactive({ ...options.formInputs });
const employeeOptions = ["emp1", "emp2", "emp3"];

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
