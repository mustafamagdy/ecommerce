<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="text-h6 q-mb-md">{{ $t("addNewSampletable") }}</div>
        <q-form ref="observer" @submit.prevent="submitForm()">
            <div class="col-12 row">
                <div class="col-sm-3 col-12">
                    {{ $t("service_add_name") }} *
                </div>
                <div class="col-sm-9 col-12">
                    <q-input v-model.trim="formData.name" :error="v$.name.$error"
                             :error-message="getFieldErrorsMsg(v$.name)">
                    </q-input>
                </div>
            </div>
            <div class="col-12 row">
                <div class="col-sm-3 col-12">
                    {{ $t("service_add_description") }} *
                </div>
                <div class="col-sm-9 col-12">
                    <q-input v-model.trim="formData.description" type="textarea"></q-input>
                </div>
            </div>
            <div class="row flex-center no-wrap q-my-md">
                <q-btn :label="$t('btn_save')" icon="mdi-content-save-outline" type="submit" :loading="saving"
                       class="save">
                    <template v-slot:loading>
                        <q-spinner-oval/>
                    </template>
                </q-btn>
                <q-btn :label="$t('btn_close')" icon="mdi-close" class="close" v-close-popup/>
            </div>
        </q-form>
        <slot :submit="submitForm" :saving="saving"></slot>
    </q-card>
</template>
<script setup>
import {computed, ref, reactive, toRefs} from "vue";
import useVuelidate from "@vuelidate/core";
import {required, maxLength} from "@vuelidate/validators";
import {useMeta} from "quasar";
import {useApp} from "src/composables/app.js";
import {$t} from "src/services/i18n";
import {useAddPage} from "src/composables/addpage.js";

const options = reactive({
    apiPath: "services",
    pageName: "services",
    formInputs: {
        name: "",
        description: ""
    },
});
const app = useApp();// application state and methods
const formData = reactive({...options.formInputs});
// perform custom validation before submit form
// set custom form fields
// return false if validation fails
function beforeSubmit() {
    return true;
}

// after form submited to api
// reset form data.
// redirect to another page
const onFormSubmitted = (data) => {
    let record = {id: data, ...formData};
    addRecordToList(record)
}

//form validation rules
const rules = {
    name: {required: required, maxLength: maxLength(75)},
};
const v$ = useVuelidate(rules, formData); // form validation
// page form hook
const page = useAddPage(options, formData, v$, onFormSubmitted, beforeSubmit);
//page state
const {
    submitted, // form submitted state - Boolean
    saving // form api submiting state - Boolean
} = toRefs(page.state);
//page methods
const {
    submitForm, //submit form data to api
    getFieldErrorsMsg, //  get validation error message - args(fieldname)
    addRecordToList
} = page.methods;
useMeta(() => {
    return {
        //set browser title
        title: $t("addNewSampletable")
    };
});
// expose page object for other components access
defineExpose({
    page
});
</script>
<style scoped>
</style>
