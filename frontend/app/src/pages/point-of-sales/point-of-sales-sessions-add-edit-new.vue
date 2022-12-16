<template>
    <q-card>
        <div class="row">
            <div class="q-ma-sm">
                <div class="text-h5 q-pa-md">{{ props.showAdd ? $t("Add new Point of sale session") : $t("Edit Session") }}</div>

                <div class="q-pa-md" style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()">
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("point_of_sale:") }}</span>
                            <q-select class="q-ma-sm" v-model="formData.name" :options="pointOfSales" />
                        </div>
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("employee :") }}</span>
                            <q-select class="q-ma-sm" v-model="formData.employeeName" :options="employees" />
                        </div>

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
    employeeName: "",
    status: "new",
    statusModifyingDate: {
        activationDate: "",
        closingDate: "",
        approvingDate: "",
    },
};
const accept = ref(false);
const pointOfSales = ["point1", "point2"];
const employees = ["emp1", "emp2", "emp3"];
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
    apiPath: serverApis.pointOfSalesSessions,
    storeModule: storeModules.pointOfSalesSessions,
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
