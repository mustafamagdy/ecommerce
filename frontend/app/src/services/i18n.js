import { StorageService } from 'src/services/storage';

const locale = StorageService.getLocale() || "en-US";
let messages = {};
try {
	messages = require(`src/i18n/${locale}`).default;
}
catch (err) {
	console.error(err);
}

const i18n = {
  locale,
  messages,
  t: function (key){
    return messages[key] || key;
  }
};

const $t = i18n.t;
export { i18n, $t }
