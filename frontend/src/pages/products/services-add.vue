<template>
    <div class="main-page">
        <template v-if="showHeader">
            <q-card :flat="isSubPage" class="page-section q-py-sm q-px-md q-mb-md nice-shadow-18">
                <div class="container">
                    <div class="row justify-between q-col-gutter-md">
                        <div class="col-12 col-md-auto ">
                            <div class="">
                                <div class="row  items-center q-col-gutter-sm q-px-sm">
                                    <div class="col">
                                        <div class="text-h6 text-primary">{{ $t("addNewSampletable") }}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </q-card>
        </template>
        <section class="page-section ">
            <div class="container">
                <div class="row q-col-gutter-x-md">
                    <div class="col-md-9 col-12 comp-grid">
                        <q-card :flat="isSubPage" class="q-pa-md nice-shadow-18">
                            <div>
                                <q-form ref="observer" @submit.prevent="submitForm()">
                                    <div class="row q-col-gutter-x-md">
                                        <div class="col-12">
                                            <div class="row">
                                                <div class="col-sm-3 col-12">
                                                    {{ $t("id") }} *
                                                </div>
                                                <div class="col-sm-9 col-12">
                                                    <q-input outlined dense ref="ctrlid" v-model.trim="formData.id"
                                                             :label="$t('id')" type="text" :placeholder="$t('enterId')"
                                                             class="" :error="isFieldValid('id')"
                                                             :error-message="getFieldError('id')">
                                                    </q-input>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12">
                                            <div class="row">
                                                <div class="col-sm-3 col-12">
                                                    {{ $t("codefield") }} *
                                                </div>
                                                <div class="col-sm-9 col-12">
                                                    <q-input outlined dense ref="ctrlcodefield"
                                                             v-model.trim="formData.codefield" :label="$t('codefield')"
                                                             type="text" :placeholder="$t('enterCodefield')"
                                                             class="" :error="isFieldValid('codefield')"
                                                             :error-message="getFieldError('codefield')">
                                                    </q-input>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12">
                                            <div class="row">
                                                <div class="col-sm-3 col-12">
                                                    {{ $t("namefield") }} *
                                                </div>
                                                <div class="col-sm-9 col-12">
                                                    <q-input outlined dense ref="ctrlnamefield"
                                                             v-model.trim="formData.namefield" :label="$t('namefield')"
                                                             type="text" :placeholder="$t('enterNamefield')"
                                                             class="" :error="isFieldValid('namefield')"
                                                             :error-message="getFieldError('namefield')">
                                                    </q-input>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12">
                                            <div class="row">
                                                <div class="col-sm-3 col-12">
                                                    {{ $t("datefield") }} *
                                                </div>
                                                <div class="col-sm-9 col-12">
                                                    <q-input outlined dense v-model.trim="formData.datefield"
                                                             :error="isFieldValid('datefield')"
                                                             :error-message="getFieldError('datefield')">
                                                        <template v-slot:prepend>
                                                            <q-icon name="date_range" class="cursor-pointer">
                                                                <q-popup-proxy transition-show="scale"
                                                                               transition-hide="scale">
                                                                    <q-date mask="YYYY-MM-DD HH:mm"
                                                                            v-model="formData.datefield" />
                                                                </q-popup-proxy>
                                                            </q-icon>
                                                        </template>
                                                        <template v-slot:append>
                                                            <q-icon name="access_time" class="cursor-pointer">
                                                                <q-popup-proxy transition-show="scale"
                                                                               transition-hide="scale">
                                                                    <q-time v-model="formData.datefield"
                                                                            mask="YYYY-MM-DD HH:mm" />
                                                                </q-popup-proxy>
                                                            </q-icon>
                                                        </template>
                                                    </q-input>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div v-if="showSubmitButton" class="text-center q-my-md">
                                        <q-btn type="submit" :rounded="false" color="primary" no-caps unelevated
                                               icon-right="send" :loading="saving">
                                            {{ submitButtonLabel }}
                                            <template v-slot:loading>
                                                <q-spinner-oval />
                                            </template>
                                        </q-btn>
                                    </div>
                                </q-form>
                                <slot :submit="submitForm" :saving="saving"></slot>
                            </div>
                        </q-card>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>
<script setup>
import { computed, ref, reactive, toRefs } from "vue";
import useVuelidate from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { useMeta } from "quasar";
import { useApp } from "src/composables/app.js";
import { $t } from "src/services/i18n";
import { useAddPage } from "src/composables/addpage.js";

const props = defineProps({
    pageName: {
        type: String,
        default: "sampletable"
    },
    routeName: {
        type: String,
        default: "sampletableadd"
    },
    apiPath: {
        type: String,
        default: "sampletable/add"
    },
    submitButtonLabel: {
        type: String,
        default: () => $t("submit")
    },
    msgAfterSave: {
        type: String,
        default: () => $t("recordAddedSuccessfully")
    },
    showHeader: {
        type: Boolean,
        default: true
    },
    showSubmitButton: {
        type: Boolean,
        default: true
    },
    redirect: {
        type: Boolean,
        default: true
    },
    isSubPage: {
        type: Boolean,
        default: false
    },
    formInputs: {
        type: Object,
        default: function() {
            return {
                id: "",
                codefield: "",
                namefield: "",
                datefield: ""
            };
        }
    }
});
const app = useApp();// application state and methods
const formData = reactive({ ...props.formInputs });
// perform custom validation before submit form
// set custom form fields
// return false if validation fails
function beforeSubmit() {
    return true;
}

// after form submited to api
// reset form data.
// redirect to another page
function onFormSubmited(response) {
    app.flashMsg(props.msgAfterSave);
    Object.assign(formData, props.formInputs); //reset form data
    if (props.redirect) app.navigateTo(`/sampletable`);
}

//form validation rules
const rules = computed(() => {
    return {
        id: { required },
        codefield: { required },
        namefield: { required },
        datefield: { required }
    };
});
const v$ = useVuelidate(rules, formData); // form validation
// page form hook
const page = useAddPage({ props, formData, v$, onFormSubmited, beforeSubmit });
//page state
const {
    submitted, // form submitted state - Boolean
    saving // form api submiting state - Boolean
} = toRefs(page.state);
//page methods
const {
    submitForm, //submit form data to api
    isFieldValid, // check if field is validated - args(fieldname)
    getFieldError //  get validation error message - args(fieldname)
    // map api datasource  to Select options label-value
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
