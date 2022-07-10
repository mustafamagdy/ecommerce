import { reactive, computed, watch } from "vue";

import { useApp } from 'src/composables/app';
import { useStore } from 'vuex';
import { $t } from 'src/services/i18n';
export function useAddPage({props, formData, v$, onFormSubmited, beforeSubmit }) {
	const app = useApp();
	const store = useStore();
	const state = reactive({
		id: null,
		submitted: false,
		saving: false,
		errorMsg: '',
		isPwd: true,
		isCPwd: true,
	});

	const apiUrl = computed(() => {
		return props.apiPath
	});

	function validateForm(){
		state.submitted = true;
		v$.value.$validate();
		if(v$.value.$invalid){
			return false;
		}
		return true;
	}

	async function submitForm(){

		if(beforeSubmit !== undefined){
			if(!beforeSubmit()){ return; }
		}

		if(!validateForm()){
			app.flashMsg($t('formIsInvalid'), $t('pleaseCompleteTheForm'), 'negative');
			return;
		}
		
		state.saving = true;
		let url = apiUrl.value;
		let data = { url, payload: formData  }
		try{
			let response = await store.dispatch(`${props.pageName}/saveRecord`, data);
			state.saving = false;
			state.submitted = false;
			app.closeDialogs();// close page dialog that if opened
			onFormSubmited(response);
		}
		catch(e) {
			state.saving = false;
			app.showPageRequestError(e);
		}
	}

	function resetForm(){
		if(Array.isArray(formData)){
			formData = [{...props.formInputs}];  //reset form data
		}
		else{
			Object.assign(formData, props.formInputs); //reset form data
		}
	}

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

	function isFieldValid(field, index){
		if(index===undefined){
			return v$.value[field].$invalid && state.submitted;	
		}
		else if(v$.value.$each.$response.$errors[index][field].length && state.submitted){
			return true;
		}
		return false;
	}

	function getFieldError(field, index){
		let fieldErrors = null;
		if(index===undefined){
			fieldErrors = v$.value[field].$silentErrors;
			if(fieldErrors.length){
				return fieldErrors[0].$message; //return the first error
			}
		}
		else{
			fieldErrors = v$.value.$each.$response.$errors[index][field];
			if(fieldErrors.length){
				return fieldErrors[0].$message; //return the first error
			}
		}
		return null
	}
	
	// watch(() => props.formInputs, (current) => {
	// 	Object.assign(formData, current);
	//   },
	//   { deep: true, immediate: true }
  	// );

	const computedProps = {
		apiUrl,
	}

	const methods = {
		submitForm,
		isFieldValid,
		getFieldError,
		mapOptionField
	}
	
	return {
		validateForm,
		resetForm,
		formData,
		state,
		computedProps,
		methods
	}
}