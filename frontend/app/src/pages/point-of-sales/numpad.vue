<template>
    <div class="row num-keypad">
        <q-btn v-for="key in numpadKeys" class="num-keypad-key" push
               :class="(key==='.' && isDotPressed)?'bg-color-positive':'bg-color-dark'" @click="numPress(key)">
            <div class="row flex-center num-keypad-key-content">
                <span class="text-h5">{{ key }}</span>
            </div>
        </q-btn>
    </div>
</template>
<script setup>
import { ref } from "vue";

const props = defineProps({
    modelValue: {
        type: Number,
        default: 0,
        required: true
    }
});
const emit = defineEmits(["update:modelValue"]);
const numpadKeys = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "0", ".", "x"];
const isDotPressed = ref(false);
const numPress = (num) => {
    switch (num) {
        case "x":
            emit("update:modelValue", 0);
            isDotPressed.value = false;
            break;
        case ".":
            isDotPressed.value = props.modelValue % 1 === 0 && !isDotPressed.value;
            break;
        default:
            const val = props.modelValue;
            const strVal = val === 0 || val === 0.0 ?
                isDotPressed.value ? "0." : ""
                : isDotPressed.value ? val.toString() + "." : val.toString();
            emit("update:modelValue", parseFloat(strVal + num));
            isDotPressed.value = false;
    }
};
</script>
