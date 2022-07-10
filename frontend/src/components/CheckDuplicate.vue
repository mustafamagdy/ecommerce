<template>
    <api-data-source :api-path="apiPath" v-slot="apiProp">
		<slot :loading="apiProp.loading" :check="check" :errorMsg="msg" :alreadyExist="checkExits(apiProp.response)"></slot>
	</api-data-source>                 
</template>
<script>
    export default {
        props: {
            errorMsg: {
                type: String,
                default: 'Value already exist'
            },
            checkPath: '',
            modelValue: '',
        },
        data: function() {
            return {
                msg: null,
				loading: false,
				apiPath: '',
            };
		},
        methods: {
            check(){
				let val = this.modelValue;
				if(val){
					this.apiPath = this.checkPath  + this.modelValue;
				}
            },
            checkExits(val){
                // if the record already exist the backend should return true
                if(val){
                    val = val.toString().toLowerCase(); 
                    if(val === "true"){ // record already exit at the backend
                        this.msg = this.errorMsg;
                        return true;
                    }
                }
                this.msg = null;
                return false;
            },
        },
    }
</script>
