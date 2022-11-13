<template>
    <div class="col-grow row items-stretch justify-start">
        <div class="col-9 column col-grow panel justify-between">
            <div class="row col-grow">
                <div class="col-3 column col-grow">
                    <employeeList class="col-grow" />
                </div>
                <q-separator vertical inset />

                <div class="col-3 column col-grow q-ma-sm">
                    <div class="column">
                        <q-input readonly :label="$t('Selected-job')" class="q-mr-sm" />
                        <div class="q-pa-md flex flex-center">
                            <q-option-group :options="options" type="checkbox" v-model="screens" />
                        </div>
                    </div>
                </div>
                <q-separator vertical inset />

                <div class="col-3 column q-ma-sm justify-between">
                    <div class="column">
                        <q-input readonly :label="$t('Selected-Screen')" class="q-mr-sm" />
                        <div class="q-pa-md">
                            <q-option-group :options="options" type="checkbox" v-model="screens" />
                        </div>
                    </div>
                    <div class="row flex flex-center">
                        <q-btn icon="done" padding="xs" class="bg-color-primary" />
                    </div>
                </div>
            </div>
        </div>
        <div class="col-3 column col-grow q-pa-sm">
            <employeesJobsList class="col-grow" />
        </div>
    </div>
</template>
<script setup>
import { ref } from "vue";
import employeesJobsList from "./employees-jobs-list.vue";
import employeeList from "./employee-list.vue";
const screens = ref([]);
const options = [
    { label: "Screen1", value: "Screen1" },
    { label: "Screen2 ", value: "Screen2" },
    { label: "Screen3 ", value: "Screen3" },
    { label: "Screen4 ", value: "Screen4" },
    { label: "Screen5 ", value: "Screen5" },
];
</script>
