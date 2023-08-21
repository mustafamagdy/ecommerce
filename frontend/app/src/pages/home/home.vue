<template>
    <q-page class="main-page">
        <section class="page-section q-pa-md">
            <div class="container-fluid">
                <div class="row q-col-gutter-x-md">
                    <div class="col comp-grid">
                        <div class="">
                            <div class="row items-center q-col-gutter-sm q-px-sm">
                                <div class="col">
                                    <div class="text-h6 text-bold text-center">{{ $t("home") }}</div>
                                </div>
                            </div>
                        </div>
                        <q-separator class="q-my-sm"></q-separator>
                    </div>
                </div>
            </div>
        </section>
    </q-page>
</template>
<script setup>
import { computed, ref } from "vue";
import { useApp } from "src/composables/app.js";
import { $t } from "src/services/i18n";
const app = useApp();
</script>
