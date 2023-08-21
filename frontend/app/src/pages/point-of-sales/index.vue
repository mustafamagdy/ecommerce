<template>
    <div class="column col-grow">
        <div class="row justify-between q-mb-sm">
            <div class="row items-center">
                <q-input readonly :label="$t('pos-invoice-date')" class="q-mr-sm" />
                <q-input readonly :label="$t('pos-invoice-no')" />
                <q-btn icon="mdi-magnify" class="bg-color-dark" />
            </div>

            <div class="row items-center justify-between">
                <!--<q-toggle v-model="order.isForward" keep-color color="red" :label="$t('pos-is-forward')" />-->

                <div>
                    <q-input readonly :label="$t('pos-client-name')" />
                </div>

                <div>
                    <q-btn icon="mdi-dots-horizontal" padding="xs" class="bg-color-dark">
                        <q-menu persistent style="width: 50%">
                            <search-add-customer />
                        </q-menu>
                    </q-btn>
                </div>
            </div>
        </div>
        <div class="col-grow row">
            <q-stepper v-model="step" ref="stepper" color="primary" animated class="col-12" header-class="hidden" flat>
                <q-step :name="1" class="row col-grow no-wrap" title="">
                    <div class="row col-grow">
                        <div class="col-grow column col-3">
                            <q-scroll-area class="col-grow panel q-mr-sm" visible>
                                <div class="row">
                                    <div v-for="service in servicesList.records" :key="service.id" class="col-6">
                                        <q-btn class="bg-color-dark">
                                            <div class="column flex-center">
                                                <q-img :src="service.imageUrl" alt="" />
                                                <span class="text-body1">{{ service.name }}</span>
                                            </div>
                                        </q-btn>
                                    </div>
                                </div>
                            </q-scroll-area>
                        </div>
                        <div class="col-grow column col-5">
                            <q-scroll-area class="col-grow panel q-mr-sm" visible>
                                <div class="row">
                                    <div v-for="product in productsList.records" :key="product.id" class="column col-4">
                                        <q-btn class="bg-color-dark" @click="addItem(product)">
                                            <div class="column flex-center">
                                                <q-img :src="product.imagePath" alt="" />
                                                <span class="text-body1">{{ product.name }}</span>
                                            </div>
                                            <q-badge class="text-body1 bg-color-info q-my-sm" floating>{{ product.price }} </q-badge>
                                        </q-btn>
                                    </div>
                                </div>
                            </q-scroll-area>
                        </div>
                        <div class="col-grow column col-4 panel">
                            <div class="row justify-between">
                                <q-tabs
                                    v-model="affectiveDraft"
                                    dense
                                    align="left"
                                    class="bg-primary text-white shadow-2 q-mr-sm"
                                    :breakpoint="0"
                                >
                                    <q-tab
                                        icon="pause_presentation"
                                        v-for="(order, index) in tempState.draftOrders"
                                        :key="order.itemId"
                                        :name="order.itemId"
                                        @click="bringDraftToScreen(order)"
                                    >
                                        <q-badge color="orange" text-color="white" floating>{{ index + 1 }}</q-badge>
                                    </q-tab>
                                </q-tabs>
                                <q-btn
                                    icon="mdi-plus"
                                    padding="xs"
                                    align="left"
                                    class="bg-color-primary"
                                    @click="addNewTempOrder()"
                                    :disable="isemptyOrder === true"
                                />
                            </div>
                            <div class="row">
                                <span class="col-6 q-pa-xs">{{ $t("pos-order-dtl-item") }}</span>
                                <span class="col-3 q-pa-xs">{{ $t("pos-order-dtl-quantity") }}</span>
                                <span class="col-3 q-pa-xs">{{ $t("pos-order-dtl-price") }}</span>
                            </div>
                            <q-separator />
                            <q-scroll-area class="col-grow" visible>
                                <div class="col-grow">
                                    <div class="row items-center" v-for="(item, index) in order.items" :key="item.itemId">
                                        <span class="col-6 q-pa-xs">{{ item.name }}</span>
                                        <span class="col-2 q-pa-xs">{{ item.qty }}</span>
                                        <span class="col-2 q-pa-xs">{{ item.itemTotal }}</span>
                                        <div class="col-2 q-pa-xs">
                                            <q-btn class="bg-color-negative" icon="mdi-minus-circle-outline" @click="removeItem(index)" />
                                        </div>
                                    </div>
                                </div>
                            </q-scroll-area>
                            <q-separator />
                            <div class="row">
                                <span class="col-6 q-pa-xs">{{ $t("pos-order-total") }}</span>
                                <span class="col-3 q-pa-xs">{{ totalQty }}</span>
                                <span class="col-3 q-pa-xs">{{ totalPrice }}</span>
                            </div>
                            <q-btn class="bg-color-primary" @click="step = 2" v-if="step === 1" :disable="totalPrice === 0">
                                <div class="row items-center q-py-sm">
                                    <q-icon name="mdi-cash-100" class="q-mr-sm" size="50px" />
                                    <span>{{ $t("pos-order-pay") }}</span>
                                </div>
                            </q-btn>
                        </div>
                    </div>
                </q-step>
                <q-step :name="2" title="">
                    <div class="col-grow row">
                        <div class="col-8 column">
                            <payment-kit v-model="paymentMethods.records" class="col-grow panel q-mr-sm" />
                        </div>
                        <div class="col-4 column panel">
                            <div class="column col-grow items-center pos-totals text-body1">
                                <span>{{ $t("pos-total-due") }}</span>
                                <span class="pos-totals-item bg-color-info text-h4 text-center">{{ totalPrice }}</span>
                                <q-separator class="self-stretch" />
                                <span class="pos-totals-item">{{ $t("pos-paid") }}</span>
                                <div class="column items-center self-stretch">
                                    <q-separator class="self-stretch" />
                                    <div v-for="pm in paymentMethods.records" :key="pm.id" class="row no-wrap self-stretch text-h6">
                                        <span class="col-6 text-right q-pr-md">{{ pm.label }}</span>
                                        <span class="col-6">{{ pm.paid }}</span>
                                    </div>
                                    <q-separator class="self-stretch" />
                                    <span>{{ $t("pos-paid-total") }}</span>
                                    <span class="pos-totals-item bg-color-positive text-h4 text-center">{{ totalPaid }}</span>
                                </div>
                                <q-separator class="self-stretch" />
                                <span>{{ $t("pos-remain") }}</span>
                                <span
                                    class="pos-totals-item text-h4"
                                    :class="remain < 0 ? 'bg-color-negative' : remain > 0 ? 'bg-color-primary' : ''"
                                    >{{ remain }}</span
                                >
                            </div>
                            <div class="row" v-if="step === 2">
                                <q-btn class="bg-color-light" @click="step = 1">
                                    <div class="row items-center q-py-sm">
                                        <q-icon name="mdi-arrow-left" class="q-mr-sm" size="50px" />
                                    </div>
                                </q-btn>
                                <q-btn class="bg-color-positive col-grow">
                                    <div class="row items-center q-py-sm">
                                        <q-icon name="mdi-content-save-outline" class="q-mr-sm" size="50px" />
                                        <span>{{ $t("pos-order-save") }}</span>
                                    </div>
                                </q-btn>
                            </div>
                        </div>
                    </div>
                </q-step>
            </q-stepper>
        </div>
    </div>
</template>

<script setup>
import { computed, watch, onMounted, reactive, ref, toRaw } from "vue";
import PaymentKit from "src/pages/point-of-sales/payment-kit";
import SearchAddCustomer from "pages/stakeholders/search-add-stakeholder";
import { useList } from "src/composables/useList";
import { pages, serverApis, storeModules } from "src/enums";
import { useAddEditPage } from "src/composables/useAddEditPage";
import { ApiService } from "src/services/api-service";
import { date } from "quasar";
const affectiveDraft = ref("draftOrder");

const servicesList = reactive(
    useList({
        apiPath: serverApis.services,
        storeModule: storeModules.pointOfSales,
        listName: pages.services,
        pageSize: 0,
    })
);
const order = reactive({
    isForward: false,
    items: [],
    payments: [],
});
const isClickTab = ref(false);
const isemptyOrder = ref(true);
const tempState = reactive({
    draftOrders: [],
    currentIndex: 0,
});
watch(
    () => order.items.length,
    (newVal) => {
        isemptyOrder.value = newVal === 0 ? true : false;
    }
);

const addNewTempOrder = () => {
    if (isClickTab.value === false) {
        tempState.draftOrders[tempState.currentIndex] = order.items;
        tempState.currentIndex += 1;
        order.items = [];
    } else {
        order.items = [];
        isClickTab.value = false;
    }
};

const bringDraftToScreen = (draftOrder) => {
    order.items = draftOrder;
    isClickTab.value = true;
};
//     // if not empty order in temp orders --> add new , inex= last index  // or order = tempOrders[currentIndex]
//     // else --> index = the index of the empty draft order
// )

// function save(
//     //save the current temp order
//     // delete it fron draft orders
// )

const productsList = {
    records: [
        {
            id: "8529eab0-df42-48d0-b381-fb34dfc88d38",
            name: "اسم الخدمة 341",
            imagePath: "https://loremflickr.com/300/300/abstract",
            price: 2.5,
        },
        {
            id: "8529eab0-df42-48d0-b381-fb34dfc84d88",
            name: "اسم الخدمة 341",
            imagePath: "https://loremflickr.com/300/300/abstract",
            price: 2.5,
        },
        {
            id: "8529eab0-df42-48d0-b381-fb36dfc88d88",
            name: "اسم الخدمة 341",
            imagePath: "https://loremflickr.com/300/300/abstract",
            price: 2.5,
        },
        {
            id: "8529eab0-df42-48d6-b381-fb34dfc88d88",
            name: "اسم الخدمة 341",
            imagePath: "https://loremflickr.com/300/300/abstract",
            price: 2.5,
        },
        {
            id: "8529eab0-df42-48d1-b381-fb34dfc88d88",
            name: "اسم الخدمة 341",
            imagePath: "https://loremflickr.com/300/300/abstract",
            price: 2.5,
        },
        {
            id: "8529eab0-df42-48d0-b381-fb34dfc84d88",
            name: "اسم الخدمة 341",
            imagePath: "https://loremflickr.com/300/300/abstract",
            price: 2.5,
        },
        {
            id: "8529eab0-df42-48d0-b381-fb34dfc89d88",
            name: "اسم الخدمة 341",
            imagePath: "https://loremflickr.com/300/300/abstract",
            price: 2.5,
        },
    ],
};
const paymentMethods = reactive({
    records: [
        {
            id: "1",
            key: "cash",
            label: "Cash",
            type: "cash",
            icon: "mdi-cash-100",
            paid: 0,
        },
        {
            id: "2",
            key: "atm",
            label: "ATM",
            type: "atm",
            icon: "mdi-credit-card-outline",
            paid: 0,
        },
    ],
});

const step = ref(2);

const totalQty = computed(() => order.items.reduce((sum, i) => sum + i.qty, 0));
const totalPrice = computed(() => order.items.reduce((sum, i) => sum + i.itemTotal, 0));
const totalPaid = computed(() => order.payments.reduce((sum, i) => sum + i.amount, 0));
const remain = computed(() => totalPrice.value - totalPaid.value);

watch(
    () => paymentMethods.records,
    (newVal) => {
        let payments = [];
        newVal.forEach((p) => {
            payments.push({
                paymentMethodId: p.id,
                amount: p.paid,
            });
        });
        order.payments = payments;
    },
    { deep: true }
);

const addItem = (product) => {
    const itemIndex = order.items.findIndex((i) => i.itemId === product.id);
    if (itemIndex === -1) {
        order.items.push({
            itemId: product.id,
            name: product.name,
            qty: 1,
            unitPrice: product.price,
        });
    } else {
        order.items[itemIndex].qty += 1;
        order.items[itemIndex].unitPrice = product.price;
        order.items[itemIndex].price = order.items[itemIndex].qty * product.price;
    }
};

const removeItem = (index) => {
    let item = order.items[index];

    if (item.qty > 1) {
        item.qty -= 1;
        item.price = item.qty * item.unitPrice;
    } else {
        order.items.splice(index, 1);
    }
};

onMounted(async () => {
    await servicesList.load();
});
</script>
