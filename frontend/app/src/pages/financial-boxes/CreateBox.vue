<template>
    <q-card class="page-section q-py-sm q-px-md">
        <div class="row">
            <div class="q-ma-xl text-center">
                <div class="text-h5 q-pa-md">إضافة صندوق</div>

                <div class="q-pa-md" style="max-width: 400px">
                    <q-form @submit="onSubmit" @reset="onReset" class="q-gutter-md text-right">
                        <q-select filled v-model="formBoxData.type" :options="options" hint="صندوق | حساب بنكي  " label="النوع" />

                        <q-input
                            filled
                            v-model="formBoxData.boxName"
                            label="اسم الصندوق "
                            lazy-rules
                            :rules="[(val) => (val && val.length > 0) || 'Please type something']"
                        />

                        <q-select filled v-model="formBoxData.managerName" :options="managerOptions" label="المدير المسؤول" />
                        <q-select filled v-model="formBoxData.branch" :options="branchOptions" label="الفرع" />
                        <div>
                            <q-checkbox v-model="formBoxData.primary" label="صندوق رئيسي " color="primary" />
                        </div>
                        <q-toggle v-model="accept" label="I accept the license and terms" />

                        <div>
                            <q-btn label="اضافة الصندوق" type="submit" color="primary" />
                            <q-btn label="تراجع " class="q-ma-md" color="secondary" to="/view-boxes" />

                            <q-btn label="Reset" type="reset" color="primary" flat class="q-ml-sm" />
                        </div>
                    </q-form>
                </div>
            </div>
        </div>
    </q-card>
</template>
<script setup>
import { useQuasar } from "quasar";
import { ref, reactive } from "vue";
import { useRouter } from "vue-router";
import { useStore } from "vuex";

const router = useRouter();
const $q = useQuasar();

console.log($q);
const formBoxData = reactive({
    // name: "",
    type: "",
    boxName: "",
    primary: "",
    managerName: "",
    branch: "",
});
const options = ["صندوق", "حساب بنكي"];
const managerOptions = ["فهد بن عبدالله القحطاني", " محمد بن عبدالله العتيبي"];
const branchOptions = ["وسط الرياض", "شمال الرياض "];

// const $q = useQuasar();

const accept = ref(false);

function onSubmit() {
    if (accept.value !== true) {
        // alert("accept terms first");
        $q.notify({
            color: "red-5",
            textColor: "white",
            icon: "warning",
            message: "You need to accept the license and terms first",
        });
    } else {
        addBox();
        $q.notify({
            color: "green-4",
            textColor: "white",
            icon: "cloud_done",
            message: "done",
        });
    }
}
const store = useStore();
function clearFormBoxData() {
    formBoxData.type = "";
    formBoxData.boxName = "";
    formBoxData.primary = "";
    formBoxData.branch = "";
    formBoxData.managerName = "";
}
function addBox() {
    store.dispatch("moduleExample/addBox", formBoxData);
    clearFormBoxData();
    //router.go("/view-boxes");
    router.push("/view-boxes");
}
function onReset() {
    clearFormBoxData();
}
</script>
<style>
q-form {
    text-align: right !important;
}
</style>
