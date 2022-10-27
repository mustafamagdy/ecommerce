export const pageComponents = {
	namespaced: true,
	state: {
		leftDrawer: __leftdrawermode,
		leftDrawerMini: __leftdrawerminimode,
		rightDrawer: false,
		rightDrawerWidth: 400,
		pageErrors: [],
		pageComponent: '',
		pageUrl: '',
		pageData: null,
		dialogProps: {
			seamless: false,
			position: 'standard',
			persistent: false,
			maximized: false,
			closeBtn: false,
			width: '700px',
			//width: '95vw',
		},
		pageDialog: false,
		imageViewerDialog: false,
		imageViewerCurrentSlide: 1,
		imageViewerImages: [],
		modelBind: {},
	},
	getters: {
		isDialogOpen(state) {
			if(state.pageDialog || state.rightDrawer){
				return true;
			}
			return false
		},
	},
	mutations: {
		setDialogProps(state, dialog) {
			if(dialog){
				state.dialogProps.seamless = dialog.seamless || false;
				state.dialogProps.persistent = dialog.persistent || false;
				state.dialogProps.position = dialog.position || 'standard';
				state.dialogProps.maximized = dialog.maximized || false;
				state.dialogProps.closeBtn = dialog.closeBtn || false;
				if (dialog.maximized){
					state.dialogProps.width = dialog.width || '';
					state.dialogProps.maxWidth = '';
				}
				else{
					state.dialogProps.width = dialog.width || '700px';
					state.dialogProps.maxWidth = '95vw';
				}
			}
		},
		setDrawerProps(state, drawer) {
			if(drawer){
				state.rightDrawerWidth = drawer.width || 400;
			}
		},
		setImageViewerDialog(state, value) {
			state.imageViewerDialog = value;
		},
		setImageViewerCurrentSlide(state, value) {
			state.imageViewerCurrentSlide = value;
		},
		setImageViewerImages(state, value) {
			state.imageViewerImages = value;
		},
		setLeftDrawer(state, value) {
			state.leftDrawer = value;
		},
		setLeftDrawerMini(state, value) {
			state.leftDrawerMini = value;
		},
		setRightDrawer(state, value) {
			state.rightDrawer = value;
		},
		setPageDialog(state, value) {
			state.pageDialog = value;
		},
		setPageComponent(state, value) {
			state.pageComponent = value;
		},
		setPageModelBind(state, value) {
			state.modelBind = value;
		},
		setPageUrl(state, value) {
			state.pageUrl = value;
		},
		setPageData(state, value) {
			state.pageData = value;
		},
		setPageErrors(state, errors) {
			state.pageErrors = errors;
		},
	},
	actions: {
		openPageDrawer: ({ commit }, payload) => {
			let page = payload.page || null;
			let url = payload.url || null;
			let pageData = payload.pageData ?? null;
			let modelBind = payload.formdata ?? {};
			commit("setPageModelBind", modelBind);
			commit("setPageUrl", url);
			commit("setPageComponent", page);
			commit("setPageData", pageData);
			commit("setRightDrawer", true);
		},
		openPageDialog: ({ commit }, payload) => {
			let page = payload.page || null;
			let url = payload.url || null;
			let pageData = payload.pageData ?? null;
			let modelBind = payload.formdata ?? {};
			commit("setPageModelBind", modelBind);
			commit("setPageUrl", url);
			commit("setPageComponent", page);
			commit("setPageData", pageData);
			commit("setPageDialog", true);
		},
		openImageViewDialog: ({ commit }, payload) => {
			const {images, currentSlide} = payload;
			commit("setImageViewerDialog", true);
			commit("setImageViewerCurrentSlide", currentSlide);
			commit("setImageViewerImages", images);
		},
		showPageErrors: ({ commit }, errors) => {
			commit("setPageErrors", errors);
		},
	},
}
