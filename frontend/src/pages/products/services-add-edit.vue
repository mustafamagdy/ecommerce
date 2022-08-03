<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="text-h6 q-mb-md">{{ $t("addNewSampletable") }}</div>
        <q-form ref="observer" @submit.prevent="submitForm()">
            <div>
                <ImagePicker v-model:src="formData.imageUrl" v-model:file="formData.imageFile"/>
            </div>
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
import {computed, ref, reactive, toRefs, onMounted} from "vue";
import useVuelidate from "@vuelidate/core";
import {required, maxLength} from "@vuelidate/validators";
import {uid, useMeta} from "quasar";
import {useApp} from "src/composables/app.js";
import {$t} from "src/services/i18n";
import ImagePicker from "src/components/image-picker";
import {useAddEditPage} from "src/composables/addEditPage.js";

const options = reactive({
    apiPath: "services",
    pageName: "services",
    formInputs: {
        id: "",
        name: "",
        description: "",
        imageFile: {},
        imageUrl: ""
    },
});
const app = useApp();
const formData = reactive({...options.formInputs});

function beforeSubmit() {
    delete formData.imageUrl;
    formData.imageFile.name = uid();
    if (showAdd.value) delete formData.id;
    return true
}

const onFormSubmitted = (data) => {
    if (showAdd.value) {
        let record = {id: data, ...formData};
        addRecordToList(record);
    } else if (showEdit.value) {
        updateRecordInList(formData);
    }
}

const rules = {
    name: {required: required, maxLength: maxLength(75)},
};
const v$ = useVuelidate(rules, formData); // form validation

const page = useAddEditPage(options, formData, v$, onFormSubmitted, beforeSubmit);

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
