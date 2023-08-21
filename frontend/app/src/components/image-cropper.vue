<template>
    <div class="column items-center justify-center img-popup">
        <q-inner-loading :showing="loading">
            <q-spinner-ios size="50px" color="primary"/>
        </q-inner-loading>
        <Cropper ref="cropper" class="cropper" areaClassname="area" :src="imgUrl"/>

        <div class="column items-center tools">
            <div class="row no-wrap items-center self-stretch">

            </div>
            <div class="q-pa-xs row no-wrap justify-center">
                <q-file
                    v-if="$q.platform.is.mobile"
                    v-model="imgFile"
                    accept="image/*"
                    input-style="max-width:0px;display:none;"
                    style="max-width: 100%"
                    capture="user"
                    ref="mobFile"
                >
                </q-file>
                <q-file
                    v-model="imgFile"
                    accept="image/*"
                    input-style="max-width:0px;display:none;"
                    style="max-width: 100%"
                    ref="winFile"
                >
                </q-file>
                <q-btn icon="mdi-folder-open" :label="$t('btn_browse')" no-wrap
                       @click=" winFile.pickFiles()" class="action"/>
                <q-btn v-if="$q.platform.is.mobile" icon="mdi-camera" :label="$t('btn_capture')" no-wrap
                       @click="mobFile.pickFiles()" class="action"/>
                <q-btn
                    icon="mdi-delete-forever"
                    :label="$t('btn_delete')"
                    class="delete"
                    @click.stop="reset"
                    no-wrap
                />
                <q-btn
                    icon="mdi-check"
                    :label="$t('btn_save')"
                    @click="acceptImg"
                    class="no-flip-x save"
                    v-close-popup
                    no-wrap
                />
                <q-btn
                    :label="$t('btn_close')"
                    icon="mdi-close"
                    class="close"
                    v-close-popup
                    no-wrap
                />
            </div>
        </div>
    </div>
</template>

<script setup>
import {Cropper} from "vue-advanced-cropper";
import 'vue-advanced-cropper/dist/style.css';
import {onMounted, onUnmounted, ref, watch} from "vue";

const props = defineProps({
    initImg: [Blob, String],
    base64: String
});

const emit = defineEmits(["update:base64"]);

const cropper = ref(null);
const winFile = ref(null);
const mobFile = ref(null);
const imgFile = ref(null);
const imgUrl = ref("");
const initUrl = ref("");
const loading = ref(false);

watch(imgFile, (val) => {
    loadImg(val, false);
});
watch(() => props.initImg, (val) => {
    if ("string" === typeof val) {
        imgUrl.value = val;
        return;
    }
    loadImg(val, true);
});

function loadImg(file, isInit) {
    if (typeof file == "string") {
        imgUrl.value = file;
        if (isInit) initUrl.value = imgUrl.value;
        return;
    }
    if (file) {
        let reader = new FileReader();
        reader.onload = (e) => {
            imgUrl.value = e.target.result;
            if (isInit) initUrl.value = imgUrl.value;
        };
        reader.readAsDataURL(file);
    }
}

function getResultDataURL() {
    const data = getDataURL();
    const isChanged = data.split(",")[1] !== initUrl.value.split(",")[1];
    if (isChanged) {
        return data;
    }
    return "";
}

function getDataURL() {
    const {canvas} = cropper.value.getResult();
    if (canvas) {
        return canvas.toDataURL();
    }
    return "";
}

function reset() {
    imgFile.value = null;
    imgUrl.value = "";
}

function acceptImg() {
    emit("update:base64", getResultDataURL());
}

onMounted(() => {
    loadImg(props.initImg, true);
});

onUnmounted(() => {
    //URL.revokeObjectURL(props.imgUrl);
})

</script>

<style lang="scss" scoped>
.img-popup {
    background-color: rgba(0, 0, 0, 0.9);

    .q-field__native {
        display: none !important;
    }

    .cropper {
        border: solid 1px rgb(223, 223, 223);
        max-width: 100%;
        max-height: 100%;
    }

    .tools {
        max-width: 100%;
        position: absolute;
        bottom: 20px;
        left: auto;
        right: auto;
        background-color: rgb(255 255 255 / 90%);
        border-radius: 10px;
        box-shadow: 0 0 10px rgb(0 0 0 / 22%);
    }
}
</style>
