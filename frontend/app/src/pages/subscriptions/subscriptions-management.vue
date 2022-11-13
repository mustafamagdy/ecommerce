<template>
    <div class="column justify-around q-mb-sm">
        <div class="column no-wrap">
            <div class="row justify-between items-center q-ma-sm">
                <q-avatar>
                    <img src="https://loremflickr.com/300/300/abstract" />
                </q-avatar>
                <q-input readonly :label="$t('subscriber-name')" class="q-mr-sm" />
                <q-input readonly :label="$t('subscription-mobile-no')" />
                <q-input readonly :label="$t('subscription-status')" />
                <q-input readonly :label="$t('subscription-date')" />
                <div class="row no-wrap">
                    <q-btn icon="mdi-cog-outline" padding="xs" class="bg-color-dark">
                        <q-menu auto-close>
                            <q-list separator dense>
                                <q-item clickable v-ripple>
                                    <q-item-section>
                                        <q-icon size="md" name="mdi-delete-outline" />
                                    </q-item-section>
                                    <q-item-section>
                                        {{ $t("btn_delete") }}
                                    </q-item-section>
                                </q-item>
                                <q-item clickable v-ripple>
                                    <q-item-section>
                                        <q-icon size="md" name="mdi-pause-circle-outline"></q-icon>
                                    </q-item-section>
                                    <q-item-section> {{ $t("deactivate") }} </q-item-section>
                                </q-item>
                                <q-item clickable v-ripple>
                                    <q-item-section>
                                        <q-icon size="md" name="login" />
                                    </q-item-section>
                                    <q-item-section>
                                        {{ $t("manager") }}
                                    </q-item-section>
                                </q-item>

                                <q-item clickable v-ripple>
                                    <q-item-section>
                                        <q-icon size="md" name="highlight_off" />
                                    </q-item-section>
                                    <q-item-section>
                                        {{ $t("close") }}
                                    </q-item-section>
                                </q-item>
                            </q-list>
                        </q-menu>
                    </q-btn>
                </div>
            </div>
        </div>
        <div class="column col-grow">
            <q-card class="column col-grow">
                <q-tabs
                    v-model="tab"
                    dense
                    class="text-grey column"
                    active-color="primary"
                    indicator-color="primary"
                    align="justify"
                    narrow-indicator
                >
                    <q-tab name="data" :label="$t('Data')" />
                    <q-tab name="PackageManagementAndSupport" :label="$t('Package management and support')" />
                    <q-tab name="AccountStatement" :label="$t('Account statement')" />
                </q-tabs>

                <q-separator />
                <q-tab-panels v-model="tab" animated class="column col-grow panel">
                    <q-tab-panel name="data">
                        <q-scroll-area class="col-grow panel" visible>
                            <subscriberDataManagement />
                        </q-scroll-area>
                    </q-tab-panel>

                    <q-tab-panel name="PackageManagementAndSupport">
                        <subscriberPackageManagement />
                    </q-tab-panel>

                    <q-tab-panel name="AccountStatement">
                        <subscriptionAccountStatementManagement />
                    </q-tab-panel>
                </q-tab-panels>
            </q-card>
        </div>

        <q-dialog v-model="page.showAddOrEdit" persistent> </q-dialog>
    </div>
</template>
<script setup>
import { useCRUDList } from "src/composables/useCRUDList";
import { serverApis, storeModules } from "src/enums";
import { onMounted, ref, reactive } from "vue-demi";
import subscriberDataManagement from "./subscriber-data-management";
import subscriberPackageManagement from "./subscriber-package-management.vue";
import subscriptionAccountStatementManagement from "./subscriber-account-statement-management.vue";
const tab = ref("data");

const page = reactive(
    useCRUDList({
        apiPath: serverApis.subscriptions,
        storeModule: storeModules.subscriptions,
    })
);
onMounted(() => {
    page.load();
});
</script>
