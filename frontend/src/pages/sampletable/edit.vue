<template>
    <div class="main-page">
        <template v-if="showHeader">
            <q-card  :flat="isSubPage" class="page-section q-py-sm q-px-md q-mb-md nice-shadow-18" >
                <div class="container">
                    <div class="row justify-between q-col-gutter-md">
                        <div class="col-12 col-md-auto " >
                            <div class="" >
                                <div class="row  items-center q-col-gutter-sm q-px-sm">
                                    <div class="col">
                                        <div class="text-h6 text-primary">{{ $t('editSampletable') }}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </q-card>
        </template>
        <section class="page-section " >
            <div class="container">
                <div class="row q-col-gutter-x-md">
                    <div class="col-md-9 col-12 comp-grid" >
                        <q-card  :flat="isSubPage" class="q-pa-md nice-shadow-18">
                            <div >
                                <template v-if="!loading">
                                    <div class="row  q-col-gutter-x-md">
                                        <div class="col">
                                            <q-form ref="observer"  @submit.prevent="submitForm()">
                                            <!--[form-content-start]-->
                                            <div class="row q-col-gutter-x-md">
                                                <div class="col-12">
                                                    <div class="row">
                                                        <div class="col-sm-3 col-12">
                                                            {{ $t('id') }} *
                                                        </div>
                                                        <div class="col-sm-9 col-12">
                                                            <q-input outlined dense  ref="ctrlid" v-model.trim="formData.id"  :label="$t('id')" type="text" :placeholder="$t('enterId')"      
                                                            class="" :error="isFieldValid('id')" :error-message="getFieldError('id')">
                                                            </q-input>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-12">
                                                    <div class="row">
                                                        <div class="col-sm-3 col-12">
                                                            {{ $t('codefield') }} *
                                                        </div>
                                                        <div class="col-sm-9 col-12">
                                                            <q-input outlined dense  ref="ctrlcodefield" v-model.trim="formData.codefield"  :label="$t('codefield')" type="text" :placeholder="$t('enterCodefield')"      
                                                            class="" :error="isFieldValid('codefield')" :error-message="getFieldError('codefield')">
                                                            </q-input>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-12">
                                                    <div class="row">
                                                        <div class="col-sm-3 col-12">
                                                            {{ $t('namefield') }} *
                                                        </div>
                                                        <div class="col-sm-9 col-12">
                                                            <q-input outlined dense  ref="ctrlnamefield" v-model.trim="formData.namefield"  :label="$t('namefield')" type="text" :placeholder="$t('enterNamefield')"      
                                                            class="" :error="isFieldValid('namefield')" :error-message="getFieldError('namefield')">
                                                            </q-input>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-12">
                                                    <div class="row">
                                                        <div class="col-sm-3 col-12">
                                                            {{ $t('datefield') }} *
                                                        </div>
                                                        <div class="col-sm-9 col-12">
                                                            <q-input outlined dense  v-model.trim="formData.datefield"    :error="isFieldValid('datefield')" :error-message="getFieldError('datefield')">
                                                            <template v-slot:prepend>
                                                                <q-icon name="date_range" class="cursor-pointer">
                                                                <q-popup-proxy transition-show="scale" transition-hide="scale">
                                                                <q-date     mask="YYYY-MM-DD HH:mm" v-model="formData.datefield" />
                                                                </q-popup-proxy>
                                                                </q-icon>
                                                            </template>
                                                            <template v-slot:append>
                                                                <q-icon name="access_time" class="cursor-pointer">
                                                                <q-popup-proxy transition-show="scale" transition-hide="scale">
                                                                <q-time v-model="formData.datefield" mask="YYYY-MM-DD HH:mm" />
                                                                </q-popup-proxy>
                                                                </q-icon>
                                                            </template>
                                                            </q-input>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <!--[form-content-end]-->
                                            <div v-if="showSubmitButton" class="text-center q-my-md">
                                                <q-btn    :rounded="false"  color="primary"  no-caps  unelevated   type="submit" icon-right="send" :loading="saving">
                                                    {{ submitButtonLabel }}
                                                    <template v-slot:loading>
                                                        <q-spinner-oval />
                                                    </template>
                                                </q-btn>
                                            </div>
                                            </q-form>
                                            <slot :submit="submitForm" :saving="saving"></slot>
                                        </div>
                                    </div>
                                </template>
                                <template v-else>
                                    <div class="q-pa-sm text-center">
                                        <q-spinner :size="40" color="accent" indeterminate></q-spinner>
                                    </div>
                                </template>
                            </div>
                        </q-card>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>
<script setup>
	import {  computed, ref, reactive, toRefs } from 'vue';
	import useVuelidate from '@vuelidate/core';
	import { required } from '@vuelidate/validators';
	import { useMeta } from 'quasar';
	import { useApp } from 'src/composables/app.js';
	import { $t } from 'src/services/i18n';
	import { useEditPage } from 'src/composables/editpage.js';
	const props = defineProps({
		id: [String, Number],
		pageName: {
			type: String,
			default: 'sampletable',
		},
		routeName: {
			type: String,
			default: 'sampletableedit',
		},
		pagePath: {
			type : String,
			default : 'sampletable/edit',
		},
		apiPath: {
			type: String,
			default: 'sampletable/edit',
		},
		submitButtonLabel: {
			type: String,
			default: () => $t('update'),
		},
		msgAfterUpdate: {
			type: String,
			default: () => $t('recordUpdatedSuccessfully'),
		},
		showHeader: {
			type: Boolean,
			default: true,
		},
		showSubmitButton: {
			type: Boolean,
			default: true,
		},
		redirect: {
			type : Boolean,
			default : true,
		},
		isSubPage: {
			type : Boolean,
			default : false,
		},
		formInputs: {
			type: Object,
			default: function() {
				return {				id: "", 
					codefield: "", 
					namefield: "", 
					datefield: "", 
				} 
			}
		}
	});
	const app = useApp();
	const formData = reactive({...props.formInputs});
	function onFormSubmited(response) {
		app.flashMsg(props.msgAfterUpdate);
		if(props.redirect) app.navigateTo(`/sampletable`);
	}
	const rules = computed(() => {
		return {
			id: { required },
			codefield: { required },
			namefield: { required },
			datefield: { required }
		}
	});
	const v$ = useVuelidate(rules, formData); //form validation
	const page = useEditPage(props, formData, v$, onFormSubmited);
	//page state
	const {
		submitted, // form api submitted state - Boolean
		saving, // form api submiting state - Boolean
		loading, // form data loading state - Boolean
	} = toRefs(page.state);
	//page computed propties
	const {
		apiUrl, // page current api path
		currentRecord, // current page record  - Object
	} = page.computedProps;
	//page methods
	const { 
		submitForm, // submit form data to api
		isFieldValid, // check if field is validated - args(fieldname)
		getFieldError, //  get validation error message - args(fieldname)
		 // map api datasource  to Select options label-value
	} = page.methods;
	useMeta(() => {
		return {
			//set browser title
			title: $t('editSampletable')
		}
	});
</script>
<style scoped>
</style>
