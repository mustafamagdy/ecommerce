<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="row">
            <div class="q-ma-sm text-center">
                <div class="text-h5 q-pa-md">{{ props.showAdd ? $t("Add new Box") : $t("Edit Box") }}</div>

                <div class="q-pa-md" style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()" class="q-gutter-md text-right">
                        <q-select
                            filled
                            v-model="formData.typeOfBox"
                            :options="boxOptions"
                            :hint="$t('box | bank-account')"
                            :label="$t('type')"
                        />
                        <q-input
                            filled
                            v-model.trim="formData.boxName"
                            :error="v$.boxName.$error"
                            :error-message="page.getFieldErrorsMsg(v$.boxName)"
                            :label="$t('box name')"
                        ></q-input>

                        <q-select filled v-model="formData.managerName" :options="managerOptions" :label="$t('manager')" />
                        <q-select filled v-model="formData.branch" :options="branchOptions" :label="$t('branch')" />
                        <div>
                            <q-checkbox v-model="formData.primary" :label="$t('primary')" color="primary" />
                            <q-toggle v-model="accept" :label="$t('I accept the license and terms')" />
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
    typeOfBox: "",
    boxName: "",
    isprimary: "",
    managerName: "",
    branch: "",
};
const accept = ref(false);

const branchOptions = ["branch1", "branch2"];
const boxOptions = ["box", "bank_account"];
const managerOptions = ["manager1", "manager2"];
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
    boxName: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, formData);
const props = defineProps(["showAdd"]);

const page = useAddEditPage({
    apiPath: serverApis.financialBoxes,
    storeModule: storeModules.financialBoxes,
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
