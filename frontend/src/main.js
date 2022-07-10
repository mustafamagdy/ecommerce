
import { boot } from 'quasar/wrappers'
import AutoCompleteSelect from 'components/AutoCompleteSelect.vue';
import PageSearch from 'components/PageSearch.vue';
import QUploaderInput from 'components/QUploaderInput.vue';
import ApiDataSource from 'components/ApiDataSource.vue';
import InlineEdit from 'components/InlineEdit.vue';
import RecordCount from 'components/RecordCount.vue';
import CheckDuplicate from 'components/CheckDuplicate.vue';
import ImageViewer from 'components/ImageViewer.vue';
import FileViewer from 'components/FileViewer.vue';
import FullQEditor from 'components/FullQEditor.vue';
import ImportData from 'components/ImportData.vue';
import LangSwitcher from 'components/LangSwitcher.vue';

import { utils } from './utils';
import { ApiService } from './services/api';
import { StorageService } from './services/storage';
import { AppMenus } from './menus';

export default boot(async ({ app }) => {



	

	app.config.globalProperties.$appName = process.env.APP_NAME;

	const apiUrl = process.env.API_URL; //get the api base url
	app.config.globalProperties.$apiUrl = apiUrl;

	const apiPath = process.env.API_PATH; //get the the api path
	app.config.globalProperties.$apiPath = apiPath;

	//axio api service use for making api request
	app.config.globalProperties.$api = ApiService;

	//save data to localstorage
	app.config.globalProperties.$localStore = StorageService;

	//all application menu
	app.config.globalProperties.$menus = AppMenus;

	app.config.globalProperties.$utils = utils;
	
	app.component('AutoCompleteSelect', AutoCompleteSelect);
	app.component('PageSearch', PageSearch);
	app.component('InlineEdit', InlineEdit);
	app.component('ApiDataSource', ApiDataSource);
	app.component('QUploaderInput', QUploaderInput);
	app.component('RecordCount', RecordCount);
	app.component('CheckDuplicate', CheckDuplicate);
	app.component('FileViewer', FileViewer);
	app.component('ImageViewer', ImageViewer);
	app.component('FullQEditor', FullQEditor);
	app.component('ImportData', ImportData);
	app.component('LangSwitcher', LangSwitcher);

});