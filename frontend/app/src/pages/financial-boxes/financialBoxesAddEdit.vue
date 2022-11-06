<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="row">
            <div class="q-ma-xl text-center">
                <div class="text-h5 q-pa-md">{{ $t(" Add Box") }}</div>

                <div class="q-pa-md" style="max-width: 400px">
                    <q-form @submit="onSubmit" @reset="onReset" class="q-gutter-md text-right">
                        <q-select
                            filled
                            v-model="formData.typeOfBox"
                            :options="boxOptions"
                            :hint="$t('box | bank-account')"
                            :label="$t('type')"
                        />
                        <q-input
                            filled
                            v-model="formData.boxName"
                            :label="$t('box name')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />

                        <q-select filled v-model="formData.managerName" :options="managerOptions" :label="$t('manager')" />
                        <q-select filled v-model="formData.branch" :options="branchOptions" :label="$t('branch')" />
                        <div>
                            <q-checkbox v-model="formData.primary" :label="$t('primary')" color="primary" />
                            <q-toggle v-model="accept" :label="$t('I accept the license and terms')" />
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
        typeOfBox: "",
        boxName: "",
        primary: "",
        managerName: "",
        branch: "",
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

const boxOptions = ["box", "bank account"];
const managerOptions = ["manager1", "manager2"];

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
