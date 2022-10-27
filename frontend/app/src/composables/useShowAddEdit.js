import { computed } from "vue";
import { useStore } from "vuex";

export const useShowAddEdit = (moduleName, recordName = "record") => {
    const upperRecordName = recordName.charAt(0).toUpperCase() + recordName.slice(1);
    const store = useStore();
    const showAdd = computed({
        get() {
            return store.getters[`${moduleName}/showAdd${upperRecordName}`];
        },
        set(value) {
            store.commit(`${moduleName}/setShowAdd${upperRecordName}`, value);
        }
    });
    const showEdit = computed({
        get() {
            return store.getters[`${moduleName}/showEdit${upperRecordName}`];
        },
        set(value) {
            store.commit(`${moduleName}/setShowEdit${upperRecordName}`, value);
        }
    });
    const showAddOrEdit = computed({
        get() {
            return showAdd.value || showEdit.value;
        },
        set(value) {
            if (value === false) {
                showAdd.value = false;
                showEdit.value = false;
            }
        }
    });
    const editId = computed({
        get() {
            return store.getters[`${moduleName}/edit${upperRecordName}Id`];
        },
        set(value) {
            store.commit(`${moduleName}/setEdit${upperRecordName}Id`, value);
        }
    });
    return {
        showAdd,
        showEdit,
        editId,
        showAddOrEdit
    };
};
