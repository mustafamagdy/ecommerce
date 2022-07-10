<template>  
    <q-menu @before-show="beforeShow" @before-hide="beforeHide" ref="menu" fit anchor="center middle" self="center middle">
        <div v-if="title" class="q-pa-sm text-bold">{{title}}</div>
        <div class="q-ma-sm">
            <slot></slot>
            <small class="text-negative" v-if="invalid">{{ errors[0] }}</small>
            <small class="text-negative" v-if="requestError">{{ requestError }}</small>
        </div>
        <div class="q-ma-sm row justify-center no-wrap">
            <q-btn flat dense color="negative" :icon="cancelIcon" @click.stop="close"  />
            <q-btn :loading="loading" flat dense color="positive" :icon="saveIcon" @click.stop="updateField" />
        </div>
    </q-menu>         
</template>
<script>
    export default {
        props: {
            rules: {
				default: null,
			},
            fieldLabel: {
				default: ''
			},
            fieldName:{
				default: ''
			},
            modelValue: {
				default: ''
			},
            title: {
				default: ''
			},
            buttons: {
				default: true
			},
            url: {
				default: ''
			},
            saveIcon: {
				default: 'check_circle'
			},
            cancelIcon: {
				default: 'close'
			}
        },
        data: function() {
            return {
                loading: false,
                initialValue: '',
                validated: false,
                requestError: '',
            };
        },
        methods: {
            onKeyEnter(e){
                if(e.keyCode === 13){
                    this.updateField()
                }
            },
            hasChanged () {
                return !(this.modelValue === this.initialValue)
            },
            beforeShow(e){
                this.validated = false;
                this.initialValue = JSON.parse(JSON.stringify(this.modelValue))
                this.requestError = '';
            },
            close () {
                this.$refs.menu.hide()
            },
            beforeHide(e){
                if(!this.validated && this.hasChanged()){
                    //this.$emit('update:modelValue', this.initialValue)
                }
            },
           async updateField () {
                let isValid = true;
                if(isValid && this.hasChanged()){
                    this.loading = true;
                    let payload = {}
                    payload[this.fieldName] =  this.modelValue
                    this.$api.post(this.url, payload).then((response) => {
                        this.loading = false;
                        this.requestError = '';
                        this.validated = true;
                        this.$emit('update:modelValue', this.modelValue)
                        this.$refs.menu.hide();
					}).catch(err => {
                        this.loading = false;
                        this.validated = false;
                        this.requestError = err.response.data;
					});
                }
                else{
                    this.validated = false;
                }
            },
        },
    }
</script>
