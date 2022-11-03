<template>
    <div>
        <div>box card</div>
        {{ page.records }}
    </div>
</template>

<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, reactive } from "vue-demi";

const page = reactive(
    useCRUDList({
        apiPath: serverApis.financialBoxes,
        storeModule: storeModules.financialBoxes,
    })
);

onMounted(() => {
    page.load();
});
</script>
