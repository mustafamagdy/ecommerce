import { ApiService } from "src/services/api-service";
import { useStore } from "vuex";

export const useStatusChange = () => {
    const store = useStore();

    function changeStatusAndDate(record) {
        return new Promise((resolve, reject) => {
            let url = `http://localhost:3000/api/v1/pointOfSalesSessions/${record.id}`;
            let date = new Date();
            let datestring =
                date.getDate() +
                "-" +
                (date.getMonth() + 1) +
                "-" +
                date.getFullYear() +
                " " +
                date.getHours() +
                ":" +
                date.getMinutes();
            if (record.status === "new") {
                (record.status = "active"),
                    (record.statusModifyingDate.activationDate = datestring);
            } else if (record.status === "active") {
                (record.status = "closed"),
                    (record.statusModifyingDate.closingDate = datestring);
            } else if (record.status === "closed") {
                (record.status = "approved"),
                    (record.statusModifyingDate.approvingDate = datestring);
            }
            ApiService.put(url, record)
                .then((resp) => {
                    resolve(resp);
                    store.commit(`pointOfSalesSessions/updateRecord`, record);
                })
                .catch((err) => {
                    reject(err);
                });
        });
    }

    return {
        changeStatusAndDate,
    };
};
