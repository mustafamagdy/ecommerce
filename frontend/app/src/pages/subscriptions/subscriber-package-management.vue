<template>
    <q-card class="column">
        <div class="q-pa-md text-center">
            <q-form @submit="onSubmit" @reset="onReset" class="q-gutter-md text-right">
                <div class="row justify-between">
                    <q-input
                        outlined
                        class="col q-mr-sm"
                        v-model="formData.package"
                        :label="$t('Recent package')"
                        lazy-rules
                        :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                    />
                    <q-btn :label="$t('Change')" icon="find_replace" class="bg-color-primary" />
                </div>
                <q-input
                    outlined
                    class="q-mr-sm"
                    v-model="formData.subscriptionDate"
                    :label="$t('Subscription date')"
                    lazy-rules
                    :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                />

                <q-field readonly>
                    <template v-slot:control>
                        <div class="self-center full-width no-outline">Previous packages</div>
                    </template>
                </q-field>
                <q-markup-table :separator="separator" flat bordered class="flex flex-center">
                    <thead>
                        <tr>
                            <th class="text-center"></th>
                            <th class="text-center">{{ $t("Package") }}</th>
                            <th class="text-center">{{ $t("Subscription from") }}</th>
                            <th class="text-center">{{ $t("to") }}</th>
                            <th class="text-center"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td class="text-center"></td>
                            <td class="text-center"></td>
                            <td class="text-center"></td>
                            <td class="text-center"></td>
                        </tr>
                        <tr>
                            <td class="text-center"></td>
                            <td class="text-center"></td>
                            <td class="text-center"></td>
                            <td class="text-center"></td>
                        </tr></tbody
                ></q-markup-table>
                <q-select
                    outlined
                    v-model="formData.technicalSupport"
                    :options="TechnicalSupportOptions"
                    :label="$t('Technical support')"
                />
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
        packageName: "",
        subscriptionDate: "",
        technicalSupport: "",
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
const TechnicalSupportOptions = ["tech1", "tech2"];
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
const { showAdd, showEdit } = page.computedProps;
const { submitForm, getFieldErrorsMsg, addRecordToList, updateRecordInList } = page.methods;

const branchOptions = ["branch1", "branch2"];
</script>
<style></style>
