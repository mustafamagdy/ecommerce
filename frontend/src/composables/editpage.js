
import { reactive, computed, onMounted, watch } from "vue";
import { utils } from "../utils";
import { useApp } from "./app";
import { useStore } from 'vuex';
import { $t } from 'src/services/i18n';

export function useEditPage(props, formData, v$, onFormSubmited) {

	const app = useApp();
	const store = useStore();
	const state = reactive({
		id: null,
		loading:false,
		submitted: false,
		saving: false,
		errorMsg: ''
	});

	const apiUrl = computed(() => {
		if(props.id){
			return props.apiPath + '/' + encodeURIComponent(props.id);
		}
		return props.apiPath;
	});

	const currentRecord = computed( {
		get: function () {
			return store.getters[`${props.pageName}/currentRecord`];
		},
		set: function (value) {
			store.commit(`${props.pageName}/setCurrentRecord`, value);
		},
	});

	async function submitForm(){
		state.submitted = true;
		const isFormValid = !v$.value.$invalid;
		if(!isFormValid){
			app.flashMsg($t('formIsInvalid'), $t('pleaseCompleteTheForm'), 'negative');
			return;
		}
		state.saving = true;
		let url = apiUrl.value;
		let id = props.id;
		let data = {id,  url, payload: formData }
		try{
			let response = await store.dispatch(`${props.pageName}/updateRecord`, data);
			state.saving = false;
			state.submitted = false;
			app.closeDialogs();// close page dialog that if opened
			app.flashMsg(props.msgAfterSave);
			onFormSubmited(response);
		}
		catch(e) {
			state.saving = false;
			app.showPageRequestError(e);
		}
	}

	// async function load() {
	// 	state.loading = true;
	// 	state.item = null;
	// 	let url = apiUrl.value;
	// 	try{
	// 		await store.dispatch(`${props.pageName}/fetchRecord`, url);
	// 		state.loading = false;
	// 		Object.assign(formData, currentRecord.value); //update form data
	// 	}
	// 	catch(e){
	// 		console.error(e);
	// 		state.loading = false;
	// 		app.showPageRequestError(e);
	// 	}
	// }

	function mapOptionField (options, fieldname){
		if(formData){
			let currentValue = formData[fieldname] ?? null;
			if(currentValue){
				if(Array.isArray(currentValue)){
					let selectedOptions = [];
					let selectedOptionsValue = [];
					currentValue.forEach( val =>{
						let option = options.find(v => v.value == val);
						selectedOptions.push(option);
						selectedOptionsValue.push(option.value)
					});
					formData[fieldname] = mapSelectedOptions; // update the select label
					formData[fieldname] = mapSelectedOptionsValue; // this will emit the value.
				}
				else{
					let selectedOption = options.find(v => v.value == currentValue);
					formData[fieldname] = selectedOption; // update the select label
					formData[fieldname] = selectedOption.value;  // this will emit the value.
				}
			}
		}
	}

	function isFieldValid(field){
		return v$.value[field].$invalid && state.submitted;
	}

	function getFieldError(field){
		const fieldErrors = v$.value[field].$silentErrors;
		if(fieldErrors.length){
			return fieldErrors[0].$message; //return the first error
		}
		return null;
	}

	watch(() => props.formInputs, (current) => {
		Object.assign(formData, current);
	  },
	  { deep: true, immediate: true }
  	);

	onMounted(()=>{
		load();
	});

	const computedProps = {
		apiUrl,
		currentRecord
	}

	const methods = {
		load,
		submitForm,
		isFieldValid,
		getFieldError,
		mapOptionField
	}

	return {
		state,
		computedProps,
		methods
	}
}
