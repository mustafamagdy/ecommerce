import {reactive, computed, watch} from "vue";

import {useApp} from 'src/composables/app';
import {useStore} from 'vuex';
import {$t} from 'src/services/i18n';

export function useAddPage(options, formData, v$, onFormSubmitted, beforeSubmit) {
    const {
        apiPath = "",
        pageName = "",
        formInputs = {}
    } = options;
    const app = useApp();
    const store = useStore();
    const state = reactive({
        id: null,
        submitted: false,
        saving: false,
        errorMsg: '',
        isPwd: true,
        isCPwd: true,
    });

    const addRecordToList = (record) => store.commit(`${pageName}/addRecord`, record);

    const validateForm = () => {
        state.submitted = true;
        v$.value.$validate();
        return !v$.value.$invalid;
    }

    const submitForm = async () => {

        if (beforeSubmit !== undefined) {
            if (!beforeSubmit()) {
                return;
            }
        }

        if (!validateForm()) {
            app.flashMsg($t('formIsInvalid'), $t('pleaseCompleteTheForm'), 'negative');
            return;
        }

        state.saving = true;
        let url = apiPath;
        let data = {url, payload: formData}
        try {
            let response = await store.dispatch(`${pageName}/saveRecord`, data);
            state.saving = false;
            state.submitted = false;
            app.flashMsg($t("msg_after_add"));
            onFormSubmitted(response.data);
            //Object.assign(formData, formInputs);
            store.commit(`${pageName}/setShowAdd`, false);
        } catch (e) {
            state.saving = false;
            app.showPageRequestError(e);
        }
    }

    const resetForm = () => {
        if (Array.isArray(formData)) {
            formData = [{...formInputs}];  //reset form data
        } else {
            Object.assign(formData, formInputs); //reset form data
        }
    }

    const getFieldErrorsMsg = (field) => {
        let fieldErrors = null;
        fieldErrors = field.$silentErrors;
        if (fieldErrors.length) {
            return fieldErrors[0].$message; //return the first error
        }
        return null
    }

    // watch(() => props.formInputs, (current) => {
    // 	Object.assign(formData, current);
    //   },
    //   { deep: true, immediate: true }
    // );
    const methods = {
        submitForm,
        getFieldErrorsMsg: getFieldErrorsMsg,
        addRecordToList
    }
    return {
        validateForm,
        resetForm,
        formData,
        state,
        methods
    }
}
