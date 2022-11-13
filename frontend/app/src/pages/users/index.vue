<template>
    <div class="col-grow row items-stretch justify-start">
        <div class="col column q-pa-sm">
            <usersRuleList />
        </div>
        <div class="column col-grow justify-between">
            <div class="row col-grow">
                <div class="column">
                    <q-input readonly :label="$t('Selected-role')" class="q-mr-sm" />
                    <div class="q-pa-md flex flex-center">
                        <q-option-group :options="options" type="checkbox" v-model="screens" />
                    </div>
                </div>
                <q-separator vertical inset />

                <div class="column">
                    <q-input readonly :label="$t('Selected-Screen')" class="q-mr-sm" />
                    <div class="q-pa-md">
                        <q-option-group :options="options" type="checkbox" v-model="screens" />
                    </div>
                    <div class="row flex flex-center">
                        <q-btn icon="done" padding="xs" class="bg-color-primary" />
                    </div>
                </div>
                <q-separator vertical inset />

                <div class="col column col-grow">
                    <usersList class="col-grow" />
                </div>
            </div>
        </div>
    </div>
</template>
<script setup>
import { ref } from "vue";
import usersRuleList from "./users-roles-list.vue";
import usersList from "./users-list.vue";
const screens = ref([]);
const options = [
    { label: "Screen1", value: "Screen1" },
    { label: "Screen2 ", value: "Screen2" },
    { label: "Screen3 ", value: "Screen3" },
    { label: "Screen4 ", value: "Screen4" },
    { label: "Screen5 ", value: "Screen5" },
];
</script>
