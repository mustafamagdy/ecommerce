<template>
    <div class="col-grow row items-stretch justify-around">
        <div class="column q-pa-sm">
            <usersRuleList />
        </div>
        <div class="column col col-grow items-stretch">
            <div class="row col-grow justify-around">
                <div class="column">
                    <span>{{ $t("Selected-role") }}</span>

                    <div class="q-pa-md flex flex-center">
                        <q-option-group :options="options" type="checkbox" v-model="screens" />
                    </div>
                </div>
                <q-separator vertical inset />

                <div class="column">
                    <span>{{ $t("Selected-Screen") }} </span>

                    <div class="q-pa-md">
                        <q-option-group :options="options" type="checkbox" v-model="screens" />
                    </div>
                    <div class="row flex flex-center">
                        <q-btn icon="done" padding="xs" class="bg-color-primary" />
                    </div>
                </div>
                <q-separator vertical inset />

                <div class="column">
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
