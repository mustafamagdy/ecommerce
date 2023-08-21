
import { boot } from 'quasar/wrappers'

import { utils } from './utils';
import { ApiService } from './services/api-service';
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


});
