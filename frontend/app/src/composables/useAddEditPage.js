import { computed, reactive } from "vue";

import { useApp } from "src/composables/app";
import { useStore } from "vuex";
import { $t } from "src/services/i18n";
import { useShowAddEdit } from "src/composables/useShowAddEdit";
import { fetchRecord } from "src/services/crud-apis";

export function useAddEditPage(options, formData, v$, onFormSubmitted, beforeSubmit) {
    const {
        apiPath = "",
        storeModule = "",
        listName = "records",
        recordName = "record",
        formInputs = {}
    } = options;
    const upperRecordName = recordName.charAt(0).toUpperCase() + recordName.slice(1);
    const app = useApp();
    const store = useStore();
    const state = reactive({
        id: null,
        submitted: false,
        saving: false,
        loading: false
    });
    let currentRecord = computed({
        get() {
            return store.getters[`${storeModule}/current${upperRecordName}`];
        },
        set(value) {
            store.commit(`${storeModule}/setCurrent${upperRecordName}`, value);
        }
    });
    const addRecordToList = (record) => store.commit(`${storeModule}/add${upperRecordName}`, record);
    const updateRecordInList = (record) => store.commit(`${storeModule}/update${upperRecordName}`, record);
    const { showAddOrEdit, showAdd, showEdit, editId } = useShowAddEdit(storeModule, recordName);
    const validateForm = () => {
        state.submitted = true;
        v$.value.$validate();
        return !v$.value.$invalid;
    };
    const submitForm = async () => {

        if (beforeSubmit !== undefined) {
            if (!beforeSubmit()) {
                return;
            }
        }

        if (!validateForm()) {
            app.flashMsg($t("formIsInvalid"), $t("pleaseCompleteTheForm"), "negative");
            return;
        }

        state.saving = true;
        let url = apiPath;
        let data = { url, payload: formData };
        try {
            let response;
            if (showAdd.value) {
                response = await store.dispatch(`${storeModule}/saveRecord`, data);
                app.flashMsg($t("msg_after_add"));
            } else if (showEdit.value) {
                response = await store.dispatch(`${storeModule}/updateRecord`, data);
                app.flashMsg($t("msg_after_update"));
            } else {
                throw new Error($t("msg_default_error"));
            }
            onFormSubmitted(response.data);
            state.saving = false;
            state.submitted = false;
            showAddOrEdit.value = false;
        } catch (e) {
            state.saving = false;
            app.showPageRequestError(e);
        }
    };

    async function load() {
        if (!showEdit.value) return;
        state.loading = true;
        state.item = null;
        try {
            currentRecord.value = await fetchRecord(apiPath, editId.value);
            state.loading = false;
            Object.assign(formData, currentRecord.value);
        } catch (e) {
            console.error(e);
            state.loading = false;
            app.showPageRequestError(e);
        }
    }

    const resetForm = () => {
        if (Array.isArray(formData)) {
            formData = [{ ...formInputs }];  //reset form data
        } else {
            Object.assign(formData, formInputs); //reset form data
        }
    };
    const getFieldErrorsMsg = (field) => {
        let fieldErrors = null;
        fieldErrors = field.$silentErrors;
        if (fieldErrors.length) {
            return fieldErrors[0].$message; //return the first error
        }
        return null;
    };
    const computedProps = {
        showAdd,
        showEdit
    };
    const methods = {
        submitForm,
        getFieldErrorsMsg: getFieldErrorsMsg,
        addRecordToList,
        updateRecordInList,
        load
    };
    return {
        validateForm,
        resetForm,
        formData,
        state,
        computedProps,
        methods
    };
}
