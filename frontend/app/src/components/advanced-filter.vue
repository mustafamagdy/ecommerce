<template>
    <div>
        <q-btn :icon="edit?'mdi-close':'mdi-playlist-edit'" :class="edit?'close':'edit'" @click="edit=!edit"/>
        <div class="row items-center q-gutter-sm q-mt-xs" v-for="(filter,index) in data.filters">
            <span v-if="index!==0">{{ data.logic }}</span>
            <span>
                <q-select :readonly="!edit" v-model="filter.field" :options="fields" option-value="name"
                          option-label="name"/>
            </span>
            <span>
                <q-select :readonly="!edit" v-model="filter.operator" :options="operators(field(filter.field).type)"/>
            </span>
            <span>
                <component :readonly="!edit" :is="valueComponent(field(filter.field).type)" v-model="filter.value"/>
            </span>
            <q-btn v-if="edit" icon="mdi-delete" class="delete" @click="removeFilter(index)"/>
            <q-btn v-if="edit" icon="mdi-plus" class="add" @click="addFilterAfter(index)"/>
        </div>
        {{ data }}
    </div>
</template>

<script setup>
import {reactive, ref} from "vue";

const valueTypeEnum = Object.freeze({
    text: "text",
    number: "number",
    boolean: "boolean",
    date: "date",
    time: "time",
    lookup: "lookup"
});
const operatorEnum = {
    eq: {
        value: "equal",
        label: "="
    },
    gt: {
        value: "greaterThan",
        label: ">"
    },
    gtEq: {
        value: "greaterThanOrEqual",
        label: ">"
    },
    lt: {
        value: "lessThan",
        label: "<"
    },
    ltEq: {
        value: "lessThanOrEqual",
        label: "<="
    },
    s: {
        value: "startsWith",
        label: "Starts With"
    },
    e: {
        value: "endsWith",
        label: "Ends With"
    },
    c: {
        value: "contains",
        label: "Contains"
    },
    bet: {
        value: "between",
        label: "in range"
    },
    in: {
        value: "in",
        label: "in"
    },
}
const data = reactive({
    logic: "or",
    filters: [
        {
            field: "name",
            operator: "contains",
            value: "خدمة 1",
            fixed: true
        },
        {
            field: "name",
            operator: "contains",
            value: "خدمة 2"
        }
    ]
})
const fields = [
    {
        name: "name",
        type: "text"
    },
    {
        name: "isActive",
        type: "boolean"
    }
]
const edit = ref(false);
const field = (fieldName) => {
    return fields.find(field => field.name === fieldName);
}
const operatorByValue = (val) => {
    return Object.values(operatorEnum).find((op) => op.value === val);
}
const operators = (fieldType) => {
    switch (fieldType) {
        case valueTypeEnum.text:
            return [operatorEnum.eq, operatorEnum.c, operatorEnum.s, operatorEnum.e]
        case valueTypeEnum.number:
            return [operatorEnum.eq, operatorEnum.gt, operatorEnum.lt, operatorEnum.gtEq, operatorEnum.ltEq, operatorEnum.bet]
        case valueTypeEnum.boolean:
            return [operatorEnum.eq]
        case valueTypeEnum.date:
            return [operatorEnum.eq, operatorEnum.gt, operatorEnum.lt, operatorEnum.gtEq, operatorEnum.ltEq, operatorEnum.bet]
        case valueTypeEnum.time:
            return [operatorEnum.eq, operatorEnum.gt, operatorEnum.lt, operatorEnum.gtEq, operatorEnum.ltEq, operatorEnum.bet]
        default:
            return [operatorEnum.eq]
    }
}
const valueComponent = (fieldType) => {
    switch (fieldType) {
        case valueTypeEnum.text:
            return "q-input"
        case valueTypeEnum.number:
            return "q-input"
        case valueTypeEnum.boolean:
            return "q-checkbox"
        case valueTypeEnum.date:
            return
        case valueTypeEnum.time:
            return
        default:
            return "q-input"
    }
}
const addFilterAfter = (index) => {
    data.filters.splice(index + 1, 0, {
        field: "name",
        operator: "",
        value: ""
    })
}
const removeFilter = (index) => {
    data.filters.splice(index, 1);
}
</script>
