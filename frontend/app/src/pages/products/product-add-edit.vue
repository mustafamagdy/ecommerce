<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="text-h6 q-mb-md">{{ $t("add_new_product") }}</div>
        <q-form ref="observer" @submit.prevent="submitForm()">
            <div>
                <ImagePicker v-model:src="productFormData.imagePath" v-model:file="productFormData.image"/>
            </div>
            <div class="col-12 row">
                <div class="col-sm-3 col-12">
                    {{ $t("product_name") }} *
                </div>
                <div class="col-sm-9 col-12">
                    <q-input v-model.trim="productFormData.name" :error="v$.name.$error"
                             :error-message="getFieldErrorsMsg(productValidate$.name)"/>
                </div>
            </div>
            <div class="col-12 row">
                <div class="col-sm-3 col-12">
                    {{ $t("product_rate") }} *
                </div>
                <div class="col-sm-9 col-12">
                    <q-input v-model.trim="productFormData.rate" :error="productValidate$.rate.$error"
                             :error-message="getFieldErrorsMsg(productValidate$.rate)" type="number"/>
                </div>
            </div>
            <div class="col-12 row">
                <div class="col-sm-3 col-12">
                    {{ $t("service_catalog_price") }} *
                </div>
                <div class="col-sm-9 col-12">
                    <q-input v-model.trim="serviceCatalogFormData" :error="serviceCatalogValidate$.price.$error"
                             :error-message="getFieldErrorsMsg(serviceCatalogValidate$.price)" type="number"/>
                </div>
            </div>
            <div class="col-12 row">
                <div class="col-sm-3 col-12">
                    {{ $t("service_catalog_priority") }} *
                </div>
                <div class="col-sm-9 col-12">
                    <q-select v-model="serviceCatalogFormData.priority"
                              :options="serviceCatalogPriorityOptions"/>
                </div>
            </div>
            <div class="col-12 row">
                <div class="col-sm-3 col-12">
                    {{ $t("product_description") }} *
                </div>
                <div class="col-sm-9 col-12">
                    <q-input v-model.trim="productFormData.description" type="textarea"></q-input>
                </div>
            </div>
            <div class="row flex-center no-wrap q-my-md">
                <q-btn :label="$t('btn_save')" icon="mdi-content-save-outline" type="submit" :loading="saving"
                       class="bg-color-positive">
                    <template v-slot:loading>
                        <q-spinner-oval/>
                    </template>
                </q-btn>
                <q-btn :label="$t('btn_close')" icon="mdi-close" class="bg-color-light" v-close-popup/>
            </div>
        </q-form>
        <slot :submit="submitForm" :saving="saving"></slot>
    </q-card>
</template>
<script setup>
import {reactive, toRefs, onMounted} from "vue";
import useVuelidate from "@vuelidate/core";
import {required, maxLength} from "@vuelidate/validators";
import {uid, useMeta} from "quasar";
import {useApp} from "src/composables/app.js";
import {$t} from "src/services/i18n";
import ImagePicker from "src/components/image-picker";
import {useAddEditPage} from "src/composables/useAddEditPage.js";
import {serviceCatalogPriorityOptions} from "src/store/options";

const productOptions = reactive({
    apiPath: "products",
    pageName: "products",
    formInputs: {
        id: "",
        name: "",
        description: "",
        image: {},
        imagePath: ""
    },
});

const serviceCatalogOptions = reactive({
    apiPath: "servicesCatalogs",
    pageName: "servicesCatalogs",
    formInputs: {
        price: "",
        priority: "",
        serviceId: "",
        productId: ""
    },
});

const app = useApp();
const productFormData = reactive({...productOptions.formInputs});
const serviceCatalogFormData = reactive({...serviceCatalogOptions.formInputs})

function beforeSubmit() {
    delete productFormData.imagePath;
    productFormData.image.name = uid();
    if (showAdd.value) delete productFormData.id;
    return true
}

const onFormSubmitted = (data) => {
    if (showAdd.value) {
        let record = {id: data, ...productFormData};
        //let serviceCatalogAdd = useAddEditPage({})
        addRecordToList(record);
    } else if (showEdit.value) {
        updateRecordInList(productFormData);
    }
}

const productRules = {
    name: {required: required, maxLength: maxLength(75)},
};
const productValidate$ = useVuelidate(productRules, productFormData);

const serviceCatalogRules = {
    name: {required: required, maxLength: maxLength(75)},
};
const serviceCatalogValidate$ = useVuelidate(serviceCatalogRules, serviceCatalogFormData);

const page = useAddEditPage(options, productFormData, productValidate$, onFormSubmitted, beforeSubmit);

const {
    saving
} = toRefs(page.state);
const {showAdd, showEdit} = page.computedProps
const {
    submitForm,
    getFieldErrorsMsg,
    addRecordToList,
    updateRecordInList,
    load
} = page.methods;

useMeta(() => {
    return {
        title: $t("addNewSampletable")
    };
});

onMounted(() => {
    load();
});

defineExpose({
    page
});
</script>
