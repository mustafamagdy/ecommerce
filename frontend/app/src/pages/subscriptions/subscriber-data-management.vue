<template>
    <q-card class="column">
        <div class="q-pa-md text-center">
            <q-form @submit="onSubmit" @reset="onReset" class="q-gutter-md text-right">
                <div>
                    <div class="row">
                        <div class="col-8">
                            <q-input
                                outlined
                                class="q-my-sm"
                                v-model="formData.name"
                                :label="$t('Company name')"
                                lazy-rules
                                :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                            />
                            <q-input
                                outlined
                                v-model="formData.taxNumber"
                                :label="$t('Tax number')"
                                lazy-rules
                                :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                            />
                        </div>

                        <div class="col-4 flex flex-center">
                            <q-avatar>
                                <img src="https://loremflickr.com/300/300/abstract" />
                            </q-avatar>
                        </div>
                    </div>
                </div>
                <q-select outlined v-model="formData.approvalPeriod" :options="approvalPeriodOptions" :label="$t('Approval period')" />
                <div class="row">
                    <q-input
                        outlined
                        class="col q-mr-sm"
                        v-model="formData.Tel"
                        :label="$t('Tel number')"
                        lazy-rules
                        :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                    />
                    <q-input
                        outlined
                        class="col"
                        v-model="formData.email"
                        :label="$t('E-mail')"
                        lazy-rules
                        :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                    />
                </div>
                <q-input
                    outlined
                    v-model="formData.address"
                    :label="$t('Address')"
                    lazy-rules
                    :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                />
                <q-input
                    outlined
                    v-model="formData.PostNumber"
                    :label="$t('Post number')"
                    lazy-rules
                    :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                />
                <q-field readonly>
                    <template v-slot:control>
                        <div class="self-center full-width no-outline">Manager info</div>
                    </template>
                </q-field>

                <div class="row">
                    <q-input
                        outlined
                        class="col q-mr-sm"
                        v-model="formData.managerName"
                        :label="$t('Manager name')"
                        lazy-rules
                        :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                    />
                    <q-input
                        outlined
                        class="col"
                        v-model="formData.managerTel"
                        :label="$t('Manager tel')"
                        lazy-rules
                        :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                    />
                </div>
                <q-input
                    outlined
                    v-model="formData.managerEmail"
                    :label="$t('Manager E-mail ')"
                    lazy-rules
                    :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                />

                <div class="row flex-center no-wrap q-my-md">
                    <q-btn :label="$t('save')" icon="mdi-content-save-outline" type="submit" :loading="saving" class="bg-color-positive">
                        <template v-slot:loading>
                            <q-spinner-oval />
                        </template>
                    </q-btn>
                    <q-btn :label="$t('cancel')" icon="mdi-close" class="bg-color-light" v-close-popup />
                </div>
            </q-form>
        </div>
    </q-card>
</template>
<script setup>
import { useQuasar } from "quasar";
import { useList } from "src/composables/useList";
import { useApp } from "src/composables/app.js";
import { ref, reactive, toRefs, onMounted } from "vue";
import useVuelidate from "@vuelidate/core";
import { required, maxLength } from "@vuelidate/validators";
import { uid, useMeta } from "quasar";
import { $t } from "src/services/i18n";
import { useAddEditPage } from "src/composables/useAddEditPage.js";
import { serverApis, storeModules } from "src/enums";
const $q = useQuasar();

const options = reactive({
    apiPath: serverApis.financialBoxes,
    storeModule: storeModules.financialBoxes,
    formInputs: {
        id: "",
        name: "",
        taxNumber: "",
        approvalPeriod: "",
        tel: "",
        email: "",
        address: "",
        PostNumber: "",
        managerName: "",
        managerTel: "",
        managerEmail: "",
    },
});
const app = useApp();
const formData = reactive({ ...options.formInputs });
function beforeSubmit() {
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
    boxName: { required: required, maxLength: maxLength(75) },
};

const approvalPeriodOptions = ["Option1", "option2"];

const accept = ref(false);

function onSubmit() {
    if (accept.value !== true) {
        // alert("accept terms first");
        $q.notify({
            color: "red-5",
            textColor: "white",
            icon: "warning",
            message: "You need to accept the license and terms first",
        });
    } else {
        onFormSubmitted();
        $q.notify({
            color: "green-4",
            textColor: "white",
            icon: "cloud_done",
            message: "done",
        });
    }
}
const v$ = useVuelidate(rules, formData); // form validation

const page = useAddEditPage(options, formData, v$, onFormSubmitted, beforeSubmit);

const { saving } = toRefs(page.state);
// const { showAdd, showEdit } = page.computedProps;
// const { submitForm, getFieldErrorsMsg, addRecordToList, updateRecordInList } = page.methods;

const branchOptions = ["branch1", "branch2"];
</script>
<style></style>
