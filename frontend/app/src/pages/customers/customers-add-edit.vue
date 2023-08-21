<template>
    <q-card>
        <div class="column">
            <div class="q-ma-sm">
                <div class="text-h5 q-pa-md">{{ props.showAdd ? $t("Add new Customer") : $t("Edit Customer") }}</div>

                <div class="column q-pa-sm" style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()">
                        <div class="row justify-between no-wrap">
                            <div class="col">
                                <span class="label q-mx-sm">{{ $t("First_name :") }}</span>
                                <q-input class="q-ma-sm" v-model.trim="formData.firstName"></q-input>
                            </div>
                            <div class="col">
                                <span class="label q-mx-sm">{{ $t("Last_name :") }}</span>
                                <q-input class="q-ma-sm" v-model.trim="formData.lastName"></q-input>
                            </div>
                        </div>
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("Email :") }}</span>
                            <q-input class="q-ma-sm" v-model.trim="formData.email"></q-input>
                        </div>
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("Phone_number :") }}</span>
                            <q-input class="q-ma-sm" v-model.trim="formData.phoneNumber"></q-input>
                        </div>

                        <div class="row flex-center no-wrap q-my-md">
                            <q-btn
                                :label="$t('btn_save')"
                                icon="mdi-content-save-outline"
                                type="submit"
                                :loading="pageState.saving"
                                class="bg-color-positive"
                            >
                                <template v-slot:loading>
                                    <q-spinner-oval />
                                </template>
                            </q-btn>
                            <q-btn :label="$t('btn_close')" icon="mdi-close" class="bg-color-light" v-close-popup />
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

const formInputs = {
    id: "",
    userName: "",
    firstName: "",
    lastName: "",
    email: "",
    isActive: "",
    emailConfirmed: "",
    phoneNumber: "",
    imageUrl: "",
};
const app = useApp();
const formData = reactive({ ...formInputs });

function beforeSubmit() {
    delete formData.imageUrl;
    // formData.imageFile.name = uid();
    if (page.showAdd.value) delete formData.id;
    return true;
}

const onFormSubmitted = (data) => {
    if (page.showAdd.value) {
        let record = { id: data, ...formData };
        page.addRecordToList(record);
    } else if (page.showEdit.value) {
        page.updateRecordInList({ ...formData });
    }
};
const props = defineProps(["showAdd"]);

const rules = {
    firstName: { required: required, maxLength: maxLength(75) },
    lastName: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, formData);

const page = useAddEditPage({
    apiPath: serverApis.customers,
    storeModule: storeModules.customers,
    formInputs,
    formData,
    v$,
    onFormSubmitted,
    beforeSubmit,
});
const pageState = reactive({ ...page.state });

onMounted(() => {
    page.load();
});
</script>
<style></style>
