
<template>
	<div class="container">
		<div class="row justify-center">
			<div class="col-md-5 col-12">
				<div class="text-h5 text-bold q-my-md">Change Password</div>
				<ValidationObserver ref="observer" v-slot="{ invalid }" tag="form" @submit.prevent="startSaveRecord()">
					<ValidationProvider :rules="{required:true}" name="Old Password" v-slot="{ errors, invalid, validated }">
						<q-input 
							dense
							outlined
							label="Old Password" 
							placeholder="Your New Password" 
							v-model="formData.oldPassword" 
							name="password"
							type="password" 
							:error="invalid && validated" 
							:error-message="errors[0]" />
					</ValidationProvider>
				
					<ValidationProvider :rules="{required:true}" name="New Password" v-slot="{ errors, invalid, validated }">
						<q-input 
							dense
							outlined
							hint="Hints : Not Less Than 6 Characters" 
							label="New Password" 
							placeholder="Your New Password" 
							v-model="formData.password"
							name="password"
							type="password" 
							:error="invalid && validated" 
							:error-message="errors[0]" />
					</ValidationProvider>
					<ValidationProvider :rules="{required:true}" name="Password Confirm" v-slot="{ errors, invalid, validated }">
						<q-input 
							dense
							outlined
							label="Confirm new password" 
							placeholder="Confirm Password" 
							v-model="formData.confirmPassword" 
							name="cpassword"  
							type="password" 
							:error="invalid && validated" 
							:error-message="errors[0]" />
					</ValidationProvider>
					<div class="q-mt-md text-center">
						<q-btn unelevated type="submit" icon="send" color="primary" large :loading="saving">
							<q-spinner-oval slot="loading" /> Change Password
						</q-btn>
					</div>
				</ValidationObserver>
			</div>
		</div>
	</div>
</template>

<script>
export default {
	props:{
		apiUrl: 'account/change_password/',
	},
	data() {
		return {
			saving: false,
			formData: {
				oldPassword: '', 
				password: '', 
				confirmPassword: '', 
			},
		}
	},
	methods: {
		async startSaveRecord(){
			const isFormValid = await this.$refs.observer.validate();
			if(isFormValid){
				this.saving = true;
				let payload = this.formData;
				let url = this.apiUrl;
				this.$api.post(url, payload).then((response) => {
					this.saving = false;
				},
					(response) => {
					this.saving = false;
					this.$root.$emit('requestError' , response);
				});
			}
		},
	},
}
</script>
