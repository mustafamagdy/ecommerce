<template>
    <q-card>
        <div class="row">
            <div class="q-ma-sm">
                <div class="text-h5 q-pa-md">{{ props.showAdd ? $t("Add new Point of sale session") : $t("Edit Session") }}</div>
                <div class="q-pa-md" style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()">
                        <div class="column">
                            <span class="label q-ma-sm">{{ $t("point_of_sale:") }} {{ formData.name }}</span>
                        </div>
                        <div class="column">
                            <span class="label q-ma-sm">{{ $t("employee :") }} {{ formData.employeeName }}</span>
                        </div>
                        <div class="column">
                            <div class="row">
                                <span class="label q-ma-sm">{{ $t("status :") }} </span>
                                <span class="label q-ma-sm"> {{ formData.status }} </span>
                                <q-btn
                                    @click="changeStatusAndDate(formData)"
                                    v-model="formData.status"
                                    :icon="
                                        formData.status === 'active' ? 'cancel' : formData.status === 'closed' ? 'check_box' : 'recommend'
                                    "
                                    padding="xs"
                                    class="bg-color-primary"
                                >
                                    {{ formData.status === "active" ? $t("close") : formData.status === "closed" ? "approve" : "approved" }}
                                </q-btn>
                            </div>
                        </div>

                        <div class="column">
                            <div class="column">
                                <span class="label q-ma-sm">{{ $t("Procedures :") }} </span>

                                <span class="label q-ma-sm"> activated on : {{ formData.statusModifyingDate.activationDate }} </span>
                                <span class="row label q-ma-sm" v-if="formData.statusModifyingDate.closingDate != ''">
                                    Closed on : {{ formData.statusModifyingDate.closingDate }}
                                </span>
                                <span class="row label q-ma-sm" v-if="formData.statusModifyingDate.approvingDate != ''">
                                    Approved on : {{ formData.statusModifyingDate.approvingDate }}
                                </span>
                            </div>
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
import { useStatusChange } from "src/composables/useStatusChange";
import { computed, ref, reactive, toRefs, onMounted } from "vue";
import useVuelidate from "@vuelidate/core";
import { required, maxLength } from "@vuelidate/validators";
import { uid, useMeta } from "quasar";
import { useApp } from "src/composables/app.js";
import { $t } from "src/services/i18n";
import ImagePicker from "src/components/image-picker";
import { useAddEditPage } from "src/composables/useAddEditPage.js";
import { serverApis, storeModules } from "src/enums";
const { changeStatusAndDate } = useStatusChange();

const formInputs = {
    id: "",
    name: "",
    employeeName: "",
    status: "",
    statusModifyingDate: {
        activationDate: "",
        closingDate: "",
        approvingDate: "",
    },
};

const app = useApp();
const formData = reactive({ ...formInputs });

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
const addPaymentMethod = (index) => {
    formData.paymentMethods.splice(index + 1);
};
const removePaymentMethod = (index) => {
    formData.paymentMethods.splice(index, 1);
};
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
