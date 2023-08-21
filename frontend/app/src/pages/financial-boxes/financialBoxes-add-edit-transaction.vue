<template>
    <q-card>
        <div class="row">
            <div class="q-ma-sm">
                <div class="text-h5 q-pa-md">
                    {{ props.showAdd ? $t("Add new Transaction") : $t("Edit Transaction") }}
                </div>

                <div style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()">
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("Amount :") }}</span>
                            <q-input class="q-ma-sm" v-model.trim="formData.amount"></q-input>
                        </div>
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("From_box :") }}</span>
                            <q-select class="q-ma-sm" v-model="formData.fromBox" :options="boxOptions" />
                        </div>
                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("To_box :") }}</span>
                            <q-select class="q-ma-sm" v-model="formData.toBox" :options="boxOptions" />
                        </div>

                        <div class="column">
                            <span class="label q-mx-sm">{{ $t("Notices :") }}</span>
                            <q-input v-model.trim="formData.notices" class="q-ma-sm" type="textarea" />
                        </div>

                        <div class="row flex-center no-wrap">
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
    amount: "",
    fromBox: "",
    toBox: "",
    notices: "",
};
const boxOptions = ["box1", "box2"];
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
    amount: { required: required },
    fromBox: { required: required },
    toBox: { required: required },
};
const v$ = useVuelidate(rules, formData);

const page = useAddEditPage({
    apiPath: serverApis.financialBoxesTransactions,
    storeModule: storeModules.financialBoxesTransactions,
    formInputs,
    formData,
    v$,
    onFormSubmitted,
    beforeSubmit,
});
const pageState = reactive({ ...page.state });
const props = defineProps(["showAdd"]);

onMounted(() => {
    page.load();
});
</script>
<style></style>
