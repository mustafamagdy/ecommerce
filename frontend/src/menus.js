
import { $t } from 'src/services/i18n';

export const AppMenus = {
    
	navbarTopRightItems: [],
	navbarTopLeftItems: [],
	navbarSideLeftItems: [
  {
    "path": "/home",
    "label": $t('home'),
    "icon": "extension",
    "iconcolor": "",
    "target": "",
    "submenu": []
  },
  {
    "path": "/products",
    "label": $t('products'),
    "icon": "extension",
    "iconcolor": "",
    "target": "",
    "submenu": []
  }
],
	SampletableTableHeaderItems: [
  {
    "label": $t('id'),
    "align": "left",
    "sortable": false,
    "name": "id",
    "field": "id"
  },
  {
    "label": $t('codefield'),
    "align": "left",
    "sortable": false,
    "name": "codefield",
    "field": "codefield"
  },
  {
    "label": $t('namefield'),
    "align": "left",
    "sortable": false,
    "name": "namefield",
    "field": "namefield"
  },
  {
    "label": $t('datefield'),
    "align": "left",
    "sortable": false,
    "name": "datefield",
    "field": "datefield"
  },
  {
    "label": $t('action'),
    "align": "right",
    "sortable": false,
    "name": "btnactions",
    "field": ""
  }
],

    exportFormats: {
        print: {
			label: 'Print',
			color: 'blue',
            icon: 'print',
            id: 'print',
            ext: '',
        },
        pdf: {
			label: 'Pdf',
			color: 'red',
            icon: 'picture_as_pdf',
            id: 'pdf',
            ext: 'pdf',
        },
        excel: {
			label: 'Excel',
			color: 'green',
            icon: 'grid_on',
            id: 'excel',
            ext: 'xlsx',
        },
        csv: {
			label: 'Csv',
			color: 'teal',
            icon: 'grid_on',
            id: 'csv',
            ext: 'csv',
        },
    },
    locales: {
  "Italian-it": "Italian-it",
  "fr": "French",
  "zh-CN": "Chinese",
  "en-US": "English",
  "CN": "Chinese-zh",
  "ar": "Arabic",
  "ru": "Russian",
  "PT": "Portuguese-pt",
  "Russian-ru": "Russian-ru",
  "Hindi-hi": "Hindi-hi",
  "Arabic-ar": "Arabic-ar",
  "pt": "Portuguese",
  "Spanish-es": "Spanish-es",
  "it": "Italian",
  "German-de": "German-de",
  "hi": "Hindi",
  "English-en": "English-en",
  "es": "Spanish",
  "de": "German",
  "French-fr": "French-fr"
}
}