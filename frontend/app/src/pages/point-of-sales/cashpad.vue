<template>
    <div class="row cash-keypad">
        <q-btn push v-for="key in cashpadKeys" class="bg-color-light cash-keypad-key" @click="numPress(key)">
            <div class="column no-wrap flex-center cash-keypad-key-content">
                <q-icon name="mdi-cash" class="" size="30px" />
                <span class="text-body1">{{ key }}</span>
            </div>
        </q-btn>
    </div>
</template>
<script setup>
const props = defineProps({
    modelValue: {
        type: Number,
        default: 0,
        required: true
    }
});
const emit = defineEmits(["update:modelValue"]);
const cashpadKeys = ["1", "5", "10", "20", "50", "100", "200", "500"];
const numPress = (num) => {
    emit("update:modelValue", props.modelValue + parseFloat(num));
};
</script>
