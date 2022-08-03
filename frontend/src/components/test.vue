<template>
    <div>
        <Cropper ref="cropper" class="cropper" areaClassname="area" :src="'https://loremflickr.com/300/300/abstract'"/>
    </div>
</template>
<script setup>
import {Cropper} from "vue-advanced-cropper";
import 'vue-advanced-cropper/dist/style.css';
</script>
