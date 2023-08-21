<template>
    <div>
        <q-img v-bind="$attrs" ref="img" />
        <q-btn :label="$t('btn_edit')" @click="showEditImage=true" class="bg-color-info" />
        <q-dialog
            v-model="showEditImage"
            position="standard"
            maximized
            transition-show="slide-up"
            transition-hide="slide-down"
            persistent
        >
            <ImageCropper :initImg="$attrs.src" v-model:base64="_file.data" />
        </q-dialog>
    </div>
</template>

<script setup>
import ImageCropper from "src/components/image-cropper";
import { computed, reactive, ref, watch } from "vue";

const props = defineProps({
    file: Object
});

const emit = defineEmits(["update:file", "update:src"]);

const img = ref(null);
const showEditImage = ref(false);
const _file = reactive({ name: "", extension: "png", data: "" });

watch(_file, (val) => {
        emit("update:file", val);
        if (val.data && val.data !== "") {
            emit("update:src", val.data);
        }
    },
    { deep: true });
</script>
