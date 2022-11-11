<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="row">
            <div class="q-ma-sm text-center">
                <div class="text-h5 q-pa-md">{{ $t("Add Transaction") }}</div>

                <div class="q-pa-md" style="min-width: 400px">
                    <q-form @submit="onSubmit" @reset="onReset" class="q-gutter-md text-right">
                        <q-input
                            filled
                            v-model="formData.amount"
                            :label="$t('amount')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />
                        <q-select filled v-model="formData.fromBox" :options="boxOptions" :label="$t('from box')" />
                        <q-select filled v-model="formData.toBox" :options="boxOptions" :label="$t('to box')" />

                        <q-input v-model="formData.notices" filled type="textarea" label="notices" />

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
        amount: "",
        fromBox: "",
        toBox: "",
        notices: "",
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

const boxOptions = ["box1", "box2"];

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
// const page1 = reactive(
//     useList({
//         apiPath: serverApis.branches,
//         storeModule: storeModules.branches,
//     })
// );

const branchOptions = ["branch1", "branch2"];

// page.records.map(function (obj) {
//     var o = [];
//     o.name = obj.name;
//     return o;
// });

// onMounted(() => {
//     page1.load();
// });
</script>
<style></style>
