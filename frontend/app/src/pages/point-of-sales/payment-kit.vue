<template>
    <div class="row">
        <div class="col-3">
            <q-tabs
                v-model="tab"
                vertical
            >
                <q-tab v-for="pm in modelValue" :name="pm.key"
                       class="bg-color-primary q-mb-md text-light-blue-10">
                    <div class="row items-center">
                        <div class="column items-center q-py-sm text-white">
                            <q-icon :name="pm.icon" size="50px" />
                            <span>{{ pm.label }}</span>
                        </div>
                        <q-icon v-if="pm.key===tab" name="mdi-arrow-right" size="50px" />
                    </div>
                    <q-separator v-if="pm.paid>0" class="self-stretch" />
                    <q-badge v-if="pm.paid>0" class="bg-color-positive q-my-sm">{{ pm.paid }}</q-badge>
                </q-tab>
            </q-tabs>
        </div>
        <div class="col-9">
            <q-tab-panels
                v-model="tab"
                animated
                swipeable
                vertical
                transition-prev="jump-up"
                transition-next="jump-up"
            >
                <q-tab-panel v-for="(pm,index) in modelValue" :name="pm.key">
                    <span class="self-stretch bg-color-primary q-pa-sm q-ma-sm text-body1">{{ pm.label }}</span>
                    <div class="q-mb-md">
                        <q-input v-model="pm.paid" input-class="text-h1 text-center" :dense="false" filled />
                    </div>
                    <div class="row flex-center no-wrap">
                        <numpad class="" v-model="pm.paid" />
                        <cashpad v-model="pm.paid" v-if="pm.type==='cash'" />
                    </div>
                </q-tab-panel>
            </q-tab-panels>
        </div>
    </div>
</template>

<script setup>
import { ref, watch } from "vue";
import Numpad from "src/pages/point-of-sales/numpad";
import Cashpad from "pages/point-of-sales/cashpad";

const props = defineProps({
    modelValue: {
        type: Object,
        required: true
    }
});
const emit = defineEmits(["update:modelValue"]);

const tab = ref(props.modelValue[0].key);
watch(
    () => props.modelValue,
    (newVal) => {
        emit("update:modelValue", newVal);
    },
    { deep: true }
);
</script>
