import {computed} from "vue";
import {useStore} from "vuex";

export const useShowAddEdit = (pageName) => {
    const store = useStore();
    const showAdd = computed({
        get() {
            return store.getters[`${pageName}/showAdd`];
        },
        set(value) {
            store.commit(`${pageName}/setShowAdd`, value);
        }
    });
    const showEdit = computed({
        get() {
            return store.getters[`${pageName}/showEdit`];
        },
        set(value) {
            store.commit(`${pageName}/setShowEdit`, value);
        },
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
    return {
        showAdd,
        showEdit,
        showAddOrEdit
    }
}
