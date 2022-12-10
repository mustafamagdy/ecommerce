import { useCRUDList } from "./useCRUDList";

export const useStatusChange = ({ record = "" }) => {
    function changeStatusAndDate() {
        if (record.status === "new") {
            (record.status = "active"),
                (record.statusModifyingDate.activationDate = new Date());
        } else if (record.status === "active") {
            (record.status = "closed"),
                (record.statusModifyingDate.closingDate = new Date());
        } else if (record.status === "closed") {
            (record.status = "approved"),
                (record.statusModifyingDate.approvingDate = new Date());
        }
    }

    return {
        changeStatusAndDate,
    };
};
