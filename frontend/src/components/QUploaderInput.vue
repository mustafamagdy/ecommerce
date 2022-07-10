<template>

	<q-uploader ref="uploader" 
	:flat="flat"
	:max-files="fileLimit"
	:max-file-size="maxFileSizeInKB"
	:bordered="bordered" 
	:accept="accept" 
	:factory="factoryFn" 
	:label="getLabel" 
	auto-upload hide-upload-btn  
	:multiple="multiple"
	@uploaded="uploadComplete" 
	@removed="removeFile"
	@rejected="onRejected"
	>
    <template v-slot:list="scope">
        <q-list separator>
          <q-item v-for="file in scope.files" :key="file.__key">
            <q-item-section>
              <q-item-label class="full-width ellipsis">
                {{ file.name }}
              </q-item-label>

              <q-item-label caption>
                Status: {{ file.__status }}
              </q-item-label>

              <q-item-label caption>
                {{ file.__sizeLabel }} / {{ file.__progressLabel }}
              </q-item-label>
            </q-item-section>

            <q-item-section
              v-if="file.__img"
              thumbnail
              class="gt-xs"
            >
              <img style="max-width: 70px;max-height:70px" :src="file.__img.src" />
            </q-item-section>

            <q-item-section top side>
              <q-btn
                class="gt-xs"
                size="12px"
                color="red"
                flat
                dense
                round
                :icon="deleteIcon"
                @click="scope.removeFile(file)"
              />
            </q-item-section>
          </q-item>
        </q-list>
        <template v-if="showUploadedFiles">
            <div class="row">
                <template v-for="file in getUploadedFiles" :key="file.path">
                    <q-chip size="lg" square removable @remove="removeUploadedFile(file)" v-if="file.isImage">
                        <q-avatar><image-viewer :src="file.path" :image-size="file.size"></image-viewer></q-avatar>
                    </q-chip>
                    <q-chip square :icon="icon" :label="file.shortName" removable @remove="removeUploadedFile(file)" clickable @click="openFile(file)" v-else></q-chip>
                </template>
            </div>
        </template>
      </template>
    </q-uploader>

    
</template>
<script>
import { StorageService } from 'src/services/storage'
export default {
    props: {
        accept: {
            type:String,
            default:'.png,.gif,.jpg,.jpg'
        },
        extensions: {
            type:String, 
            default:''
        },
        maxFiles: {
            type:Number, 
            default: 5
        },
        maxFileSize: {
            type:Number, 
            default: 10
        },
        multiple:{
            type: Boolean, 
            default: true
        },
        flat:{
            type: Boolean, 
            default: true
        },
        bordered:{
            type: Boolean, 
            default: true
        },
        uploadPath: {
            type:String, 
            default:'fileuploader/upload/photo'
        },
        fieldName: {
            type:String, 
            default:'photo'
        },
        fileName:{
            type:String, 
            default:'file'
        },
        label:{
            type:String, 
            default:'Choose files or Drop files here'
        },
        deleteIcon:{
            type:String, 
            default:'deleteIcon'
        },
        modelValue: null,
        showUploadedFiles: {
            type:Boolean,  
            default: true
        },
    },
    data: function() {
        return {
            files: [],
            uploadedFilePaths:[],
        }
    },
    methods: {
       
        updateValue() {
            this.$emit('update:modelValue', this.uploadedFilePaths.toString());
        },
        factoryFn (file) {
             return new Promise((resolve, reject) => {
                // Retrieve JWT token from the localstore.
                const token = StorageService.getToken();
                //additional fields that is submited with file upload
                //let formFields = [{name: 'fieldname', value: 'fieldvalue'}];
				let uploadUrl = this.$utils.setApiPath(this.uploadPath)
                resolve({
                    fieldName: this.fileName,
                    url: uploadUrl,
                    method: 'POST',
                    //formFields: formFields,
                    headers: [
                        {name: 'Authorization', value: `Bearer ${token}`}
                    ]
                })
            })
        },
        uploadComplete: function(file) {
            let responseText = file.xhr.responseText;
            try{
                let response = JSON.parse(responseText);
                if(Array.isArray(response)){
                    this.uploadedFilePaths.push(...response);
                }
                else{
                    this.uploadedFilePaths.push(response);
                }
                this.updateValue();
            }
            catch(e){
                console.error(e);
            }
            this.$emit('uploadComplete', responseText);
        },
        removeFile: function(files) {
            files.forEach(file => {
				let fileUrl = JSON.parse(file.xhr.response);
				if(Array.isArray(fileUrl)){
					this.uploadedFilePaths = this.uploadedFilePaths.filter(item => !fileUrl.includes(item))
				}
				else{
					let index =  this.uploadedFilePaths.indexOf(fileUrl);
                    if(index !== -1){
                        this.uploadedFilePaths.splice(index, 1);
                    }
				}

                let url = "fileuploader/remove_temp_file";
                let postData = {
                    temp_file: fileUrl.toString()
                }
                this.$api.post(url, postData).then( (response) => {
                     this.loading = false;   
                },
                (response) => {
                    this.loading = false;
                });
                this.updateValue();
            });
		},
        removeUploadedFile: function(file) {
            let index =  this.uploadedFilePaths.indexOf(file.path);
            if(index !== -1){
                this.uploadedFilePaths.splice(index, 1);
                this.$refs.uploader.uploadedFileCount--;
                let url = "fileuploader/remove_temp_file";
                let formData = {
                    temp_file: file.path
                }
                this.$api.post(url, formData).then( (response) => {
                        
                },
                (response) => {
                    this.loading = false;
                });
            }
            this.updateValue();
		},
		onRejected: function(rejectedEntries) {
			let failedProp = rejectedEntries[0].failedPropValidation
			if(failedProp == "max-file-size"){
				this.$q.notify({
					type: 'negative',
					message: `Max file size of ${this.maxFileSize}MB is allowed`
				});
			}
			else if(failedProp == "max-files" || failedProp == "filter"){
				this.$q.notify({
					type: 'negative',
					message: `Max of ${this.maxFiles} files is allowed`
				});
			}
		},
    },
    computed: {
        maxFileSizeInKB:function () {
            return this.maxFileSize * 1024 * 1024;
        },
        fileLimit:function () {
            let numUpload = this.uploadedFilePaths.length;
            let limit = this.maxFiles;
            return (limit - numUpload) || 0;
        },
        getUploadedFiles(){
            let files = [];
            this.uploadedFilePaths.forEach(path => {
                let fileName = path.split('\\').pop().split('/').pop();
                let ext = fileName.split('.').pop().toLowerCase();
                let imgExt = ['jpg', 'png', 'gif', 'jpeg', 'bmp'];
                let isImage = false;
                if(imgExt.indexOf(ext) > -1){
                    isImage = true;
                }
                let size = "small"; //use resize image for the display
                if(path.indexOf("temp/") > -1){
                    size = "";  //if image is still in temp folder use the original image
                }
                let fileShortName = this.$utils.strEllipsis(fileName, 15);
                files.push({
                    name: fileName,
                    shortName: fileShortName,
                    isImage: isImage,
                    size: size,
                    path: path
                })
            });
            return files;
		},
        getLabel(){
            if(this.fileLimit){
                return this.label;
            }
            else{
                return "..."; //cannot upload more files
            }
        }
    },
    mounted:function(){
        
        if(this.modelValue){
            this.uploadedFilePaths = this.$utils.toArray(this.modelValue);
        }
        else{
            this.uploadedFilePaths = []
        }
    },
};
</script>
