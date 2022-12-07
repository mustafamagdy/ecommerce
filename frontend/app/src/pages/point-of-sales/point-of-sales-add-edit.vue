<template>
    <q-card>
        <div class="row">
            <div class="q-ma-sm">
                <div class="text-h5 q-pa-md">{{ props.showAdd ? $t("Add new Point of sale") : $t("Edit point of sale") }}</div>

                <div class="q-pa-md" style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()">
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("Code :") }}</span>
                            <q-input class="q-ma-sm" v-model.trim="formData.code"></q-input>
                        </div>
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("Name :") }}</span>
                            <q-input
                                class="q-ma-sm"
                                v-model.trim="formData.name"
                                :error="v$.name.$error"
                                :error-message="page.getFieldErrorsMsg(v$.name)"
                            ></q-input>
                        </div>
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("Default_customer :") }}</span>
                            <q-select class="q-ma-sm" v-model="formData.defaultCustomer" :options="defaultCustumerOptions" />
                        </div>
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("Branch :") }}</span>
                            <q-select class="q-ma-sm" v-model="formData.branch" :options="branchOptions" />
                        </div>

                        <q-card-section class="column">
                            <span class="label q-mx-sm">{{ $t("Payment_ways :") }}</span>
                            <div class="column">
                                <div class="row justify-around">
                                    <q-btn :label="$t('btn_add')" icon="add" type="submit" class="bg-color-primary">
                                        <template v-slot:loading>
                                            <q-spinner-oval />
                                        </template>
                                    </q-btn>
                                    <q-btn :label="$t('btn_add_all')" icon="add" type="submit" class="bg-color-primary">
                                        <template v-slot:loading>
                                            <q-spinner-oval />
                                        </template>
                                    </q-btn>
                                </div>
                                <div class="row flex flex-center">
                                    <q-input class="q-ma-sm" v-model="text" />
                                    <q-btn icon="close" class="text-primary" @click="removePaymentMethod()" />
                                </div>
                            </div>
                        </q-card-section>

                        <div class="row flex-center no-wrap">
                            <q-btn :label="$t('btn_save')" icon="add" type="submit" :loading="pageState.saving" class="bg-color-positive">
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
    name: "",
    branch: "",
    code: "",
    isActive: "",
    defaultCustomer: "",
    imageFile: {},
    imageUrl: "",
    paymentMethods: [],
};
const accept = ref(false);
const addMethod = false;
const branchOptions = ["branch1", "branch2"];
const boxOptions = ["box", "bank_account"];
const defaultCustumerOptions = ["cust1", "cust2"];
const app = useApp();
const formData = reactive({ ...formInputs });
const addPaymentMethod = (index) => {
    formData.paymentMethods.splice(index + 1);
};
const removePaymentMethod = (index) => {
    formData.paymentMethods.splice(index, 1);
};

const viewNextAddPaymentMethod = () => {
    addMethod = true;
};
function beforeSubmit() {
    delete formData.imageUrl;
    formData.imageFile.name = uid();
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

const rules = {
    name: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, formData);
const props = defineProps(["showAdd"]);

const page = useAddEditPage({
    apiPath: serverApis.pointOfSalesList,
    storeModule: storeModules.pointOfSalesList,
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
