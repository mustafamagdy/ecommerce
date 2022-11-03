<template>
    <q-page class="q-pa-md flex flex-center">
        <div class="row">
            <div class="q-ma-xl text-center">
                <div class="text-h5 q-pa-md">تعديل صندوق</div>

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

                        <div>
                            <q-btn label=" تعديل الصندوق" @click="editBox()" type="submit" color="primary" />
                            <q-btn label="تراجع " class="q-ma-md" color="secondary" to="/view-boxes" />

                            <q-btn label="Reset" type="reset" color="primary" flat class="q-ml-sm" />
                        </div>
                    </q-form>
                </div>
            </div>
        </div>
    </q-page>
</template>
<script setup>
import { reactive, ref, onMounted } from "vue";
import { useStore } from "vuex";
import { useRouter } from "vue-router";

const store = useStore();
const options = ["صندوق", "حساب بنكي"];
const managerOptions = ["فهد بن عبدالله القحطاني", " محمد بن عبدالله العتيبي"];
const branchOptions = ["وسط الرياض", "شمال الرياض "];
const formBoxData = reactive({
    id: "",
    type: "",
    boxName: "",
    primary: "",
    managerName: "",
    branch: "",
});
const NameOfBoxToEdit = ref("");
function clearFormBoxData() {
    formBoxData.type = "";
    formBoxData.boxName = "";
    formBoxData.primary = "";
    formBoxData.branch = "";
    formBoxData.managerName = "";
}
const router = useRouter();

function editBox() {
    store.dispatch("moduleExample/editBox", formBoxData);
    router.push("/view-boxes");
    clearFormBoxData();
    store.commit("moduleExample/clearCurrentBoxData");
}

onMounted(() => {
    formBoxData.type = store.getters["moduleExample/getCurrentBoxData"].type === "box" ? "صندوق" : "حساب بنكي";
    formBoxData.id = store.getters["moduleExample/getCurrentBoxData"].id;
    formBoxData.boxName = store.getters["moduleExample/getCurrentBoxData"].boxName;
    formBoxData.managerName = store.getters["moduleExample/getCurrentBoxData"].managerName;
    formBoxData.branch = store.getters["moduleExample/getCurrentBoxData"].branch;
    NameOfBoxToEdit.value = store.getters["moduleExample/getNameOfBoxToEdit"];
});

function onReset() {
    clearFormBoxData();
}
</script>
