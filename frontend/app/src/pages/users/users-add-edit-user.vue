<template>
    <q-card>
        <div class="column">
            <div class="q-ma-sm">
                <div class="text-h5 q-pa-md">
                    {{ props.showAdd ? $t("Add new User") : $t("Edit User") }}
                </div>

                <div class="column q-pa-sm" style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()">
                        <div class="col">
                            <span class="label q-mx-sm">{{ $t("Name :") }}</span>
                            <q-input
                                class="q-ma-sm"
                                v-model.trim="formData.name"
                                :error="v$.name.$error"
                                :error-message="page.getFieldErrorsMsg(v$.name)"
                            ></q-input>
                        </div>
                        <div class="col">
                            <span class="label q-mx-sm">{{ $t("User-name :") }}</span>
                            <q-input
                                class="q-ma-sm"
                                v-model.trim="formData.username"
                                :error="v$.username.$error"
                                :error-message="page.getFieldErrorsMsg(v$.username)"
                            ></q-input>
                        </div>
                        <div class="col">
                            <span class="label q-mx-sm">{{ $t("Password :") }}</span>
                            <q-input
                                class="q-ma-sm"
                                v-model="formData.Password"
                                lazy-rules
                                :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                            />
                        </div>
                        <div class="col">
                            <span class="label q-mx-sm">{{ $t("Phone_number :") }}</span>
                            <q-input
                                class="q-ma-sm"
                                v-model="formData.Tel"
                                lazy-rules
                                :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                            />
                        </div>
                        <div class="col">
                            <span class="label q-mx-sm">{{ $t("Email :") }}</span>
                            <q-input
                                class="q-ma-sm"
                                v-model="formData.email"
                                lazy-rules
                                :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                            />
                        </div>

                        <q-field readonly class="q-my-sm">
                            <template v-slot:control>
                                <div class="self-center full-width no-outline">Or choose recent user</div>
                            </template>
                        </q-field>
                        <div class="col">
                            <span class="label q-mx-sm">{{ $t("Choose_user :") }}</span>
                            <q-select class="q-ma-sm" v-model="formData.nameame" :options="userOptions" />
                        </div>
                        <div class="row flex-center">
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
                            <q-btn :label="$t('close')" icon="mdi-close" class="bg-color-light" v-close-popup />
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
    username: "",
    Password: "",
    email: "",
    Tel: "",
    isActive: "",
    imageUrl: "",
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
    username: { required: required, maxLength: maxLength(75) },
};
const v$ = useVuelidate(rules, formData);

const page = useAddEditPage({
    apiPath: serverApis.users,
    storeModule: storeModules.users,
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
