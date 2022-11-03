import { reactive, ref } from "vue";
import { deleteRecord } from "src/services/crud-apis";
import { useStore } from "vuex";
import { $t } from "src/services/i18n";
import { useQuasar } from "quasar";
import { useApp } from "src/composables/app";
import { useList } from "src/composables/useList";
import { useShowAddEdit } from "src/composables/useShowAddEdit";

export const useCRUDList = ({
    primaryKey = "id",
    apiPath = "",
    storeModule = "",
    listName = "records",
    recordName = "record",
    pageSize = 10,
}) => {
    const list = useList({
        primaryKey: primaryKey,
        apiPath: apiPath,
        storeModule: storeModule,
        listName: listName,
        pageSize: pageSize,
    });
    const $q = useQuasar();
    const app = useApp();
    const store = useStore();
    const { showAdd, showEdit, showAddOrEdit } = useShowAddEdit(
        storeModule,
        recordName
    );

    async function deleteItem(id) {
        if (Array.isArray(id)) {
            id = id.map((value) => value[primaryKey]);
        }
        if (id) {
            let title = $t("delete_confirmation_dialog_title");
            let msg = $t("delete_confirmation_dialog_message");
            $q.dialog({
                title: title,
                message: msg,
                cancel: true,
                persistent: true,
            })
                .onOk(async () => {
                    try {
                        const data = await deleteRecord(apiPath, id);
                        store.commit(`${storeModule}/deleteRecord`, id);
                        list.totalRecords.value--;
                        app.flashMsg($t("after_delete_message"));
                    } catch (e) {
                        app.showPageRequestError(e);
                    }
                })
                .onCancel(() => {
                    // console.log('>>>> Cancel')
                })
                .onDismiss(() => {
                    // console.log('I am triggered on both OK and Cancel')
                });
        }
    }

    return {
        loading: list.loading,
        currentPage: list.currentPage,
        totalRecords: list.totalRecords,
        totalPages: list.totalPages,
        records: list.records,
        showAdd,
        showEdit,
        showAddOrEdit,
        load: list.load,
        deleteItem,
    };
};
