import {QInput, QSelect, QBtn, QPagination} from "quasar";

const defaults = {
    outlined: true,
    hideBottomSpace: true,
    noErrorIcon: true,
    dense: true,
};
const QBtnDefaults = {
    unelevated: true,
};
const QSelectDefaults = {
    mapOptions: true,
    emitValue: true,
};
const QPaginationDefaults = {}
const propsDefaults = {
    dense: {type: Boolean, default: false},
    outlined: {type: Boolean, default: false},
    hideBottomSpace: {type: Boolean, default: false},
    noErrorIcon: {type: Boolean, default: false},
    rounded: {type: Boolean, default: false},
    unelevated: {type: Boolean, default: false},
    outline: {type: Boolean, default: false},
    mapOptions: {type: Boolean, default: false},
    emitValue: {type: Boolean, default: false},
}
const setDefaults = (components, propsDefaults, defaults) => {
    for (let i = 0; i < components.length; i++) {
        for (const key in defaults) {
            components[i].props[key] = propsDefaults[key];
            components[i].props[key].default = defaults[key];
        }
    }
};

setDefaults([QInput, QSelect], propsDefaults, defaults);
setDefaults([QBtn], propsDefaults, QBtnDefaults);
setDefaults([QSelect], propsDefaults, QSelectDefaults);
setDefaults([QPagination], propsDefaults, QPaginationDefaults);
