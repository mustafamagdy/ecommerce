import { boot } from 'quasar/wrappers'
import { Quasar } from 'quasar';
import { i18n, $t } from 'src/services/i18n';
import { StorageService } from '../services/storage';


export default boot(async ({ app }) => {
	const locale = StorageService.getLocale() || 'en';

	try {
		await import(`quasar/lang/${locale}`).then(lang => {
			Quasar.lang.set(lang.default)
		});
	}
	catch (err) {
		console.error(err);
	}

	app.config.globalProperties.$i18n = i18n;
	app.config.globalProperties.$t = $t;
	
});