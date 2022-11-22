<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="column">
            <div class="q-ma-sm text-center">
                <div class="text-h5 q-pa-md">{{ $t(" Add new User") }}</div>

                <div class="column q-pa-sm" style="min-width: 400px">
                    <q-form ref="observer" @submit.prevent="page.submitForm()" class="q-gutter-md text-right">
                        <q-input
                            class="q-ma-sm"
                            filled
                            :label="$t('name')"
                            v-model.trim="formData.name"
                            :error="v$.name.$error"
                            :error-message="page.getFieldErrorsMsg(v$.name)"
                        ></q-input>

                        <q-input
                            class="q-ma-sm"
                            filled
                            :label="$t('User-name')"
                            v-model.trim="formData.username"
                            :error="v$.username.$error"
                            :error-message="page.getFieldErrorsMsg(v$.username)"
                        ></q-input>
                        <q-input
                            class="q-ma-sm"
                            filled
                            v-model="formData.Password"
                            :label="$t('password')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />

                        <q-input
                            class="q-ma-sm"
                            filled
                            v-model="formData.Tel"
                            :label="$t('phoneNumber')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />
                        <q-input
                            class="q-ma-sm"
                            filled
                            v-model="formData.email"
                            :label="$t('E-mail')"
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />
                        <div class="row flex-center no-wrap q-my-sm">
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
                        </div>
                        <q-field readonly class="q-my-sm">
                            <template v-slot:control>
                                <div class="self-center full-width no-outline">Or choose recent user</div>
                            </template>
                        </q-field>
                        <q-select filled v-model="formData.nameame" :options="userOptions" :label="$t('choose user')" />
                        <div class="row flex-center q-my-sm">
                            <q-btn :label="$t('Add-user')" icon="add" type="submit" :loading="saving" class="bg-color-positive">
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
        page.updateRecordInList(formData);
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

onMounted(() => {
    page.load();
});
</script>
<style></style>
