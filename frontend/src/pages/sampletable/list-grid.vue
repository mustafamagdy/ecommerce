<template>
    <div class="main-page">
        <template v-if="showHeader">
            <q-card  :flat="isSubPage" class="page-section q-py-sm q-px-md q-mb-md nice-shadow-18" >
                <div class="container-fluid">
                    <div class="row justify-between q-col-gutter-md">
                        <div class="col-12 col-md-auto " >
                            <div class="" >
                                <div class="row  items-center q-col-gutter-sm q-px-sm">
                                    <div class="col">
                                        <div class="text-h6 text-primary">{{ $t('sampletable') }}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-auto col-12 " >
                            <q-btn       :rounded="false"  size=""  color="primary" no-caps  unelevated   :to="`/sampletable/add`" class="full-width" >
                                <q-icon name="add"></q-icon>                                
                                {{ $t('addNewSampletable') }} 
                            </q-btn>
                        </div>
                        <div class="col-md-auto col-12 " >
                            <q-input debounce="1000" outlined dense  :placeholder="$t('search')" v-model="searchText" >
                            <template v-slot:append>
                                <q-icon name="search"></q-icon>
                            </template>
                            </q-input>
                        </div>
                    </div>
                </div>
            </q-card>
        </template>
        <section class="page-section " >
            <div class="container-fluid">
                <div class="row q-col-gutter-x-md">
                    <div class="col comp-grid" >
                        <div>
                            <q-chip v-if="searchText" icon="search" removable @remove="clearSearch()">
                            {{ $t('search') }}: <strong class="q-ml-sm"> {{ searchText }} </strong>
                            </q-chip>
                        </div>
                        <div class="">
                            <div >
                                <template v-if="showBreadcrumbs && $route.query.tag">
                                    <q-breadcrumbs class="q-pa-md">
                                        <q-breadcrumbs-el icon="arrow_back" class="text-capitalize" :label="$route.query.tag" to="/sampletable"></q-breadcrumbs-el>
                                        <q-breadcrumbs-el :label="$route.query.label"></q-breadcrumbs-el>
                                    </q-breadcrumbs>
                                </template>
                                <div class="relative-position">
                                    <div class="row q-col-gutter-md">
                                        <div class="col">
                                            <!-- Master Page Start -->
                                            <!-- page records template -->
                                            <div >
                                                <q-table 
                                                grid
                                                table-header-class="text-h4"
                                                card-container-class="q-col-gutter-md justify-start"
                                                :bordered="false"
                                                :columns="$menus.SampletableTableHeaderItems" 
                                                :rows="records"
                                                binary-state-sort
                                                v-model:selected="selectedItems"
                                                selection="multiple"
                                                row-key="id" 
                                                v-model:pagination="pagination"
                                                hide-bottom
                                                @request="setPagination"
                                                :loading="loading">
                                                <template v-slot:item="props">
                                                    <div class="col-sm-6 col-md-3 col-12">
                                                        <div :class="{selected: isCurrentRecord(props.row)}" class="grid-style-transition" :style="props.selected ? 'transform: scale(0.95);' : ''">
                                                            <q-card  class="nice-shadow-18 nice-shadow-18">
                                                                <q-list >
                                                                    <q-item>
                                                                        <q-item-section  class="">
                                                                            <q-item-label caption>
                                                                                {{ $t('id') }}
                                                                            </q-item-label>
                                                                            <q-item-label class="text-bold">
                                                                                <q-btn padding="xs"   :rounded="false"  color="primary"  no-caps  unelevated   flat :to="`/sampletable/view/${props.row.id}`">{{ props.row.id }}</q-btn>
                                                                            </q-item-label>
                                                                        </q-item-section>
                                                                    </q-item>
                                                                    <q-separator inset></q-separator>
                                                                    <q-item>
                                                                        <q-item-section  class="">
                                                                            <q-item-label caption>
                                                                                {{ $t('codefield') }}
                                                                            </q-item-label>
                                                                            <q-item-label class="text-bold">
                                                                                {{ props.row.codefield }}
                                                                            </q-item-label>
                                                                        </q-item-section>
                                                                    </q-item>
                                                                    <q-separator inset></q-separator>
                                                                    <q-item>
                                                                        <q-item-section  class="">
                                                                            <q-item-label caption>
                                                                                {{ $t('namefield') }}
                                                                            </q-item-label>
                                                                            <q-item-label class="text-bold">
                                                                                {{ props.row.namefield }}
                                                                            </q-item-label>
                                                                        </q-item-section>
                                                                    </q-item>
                                                                    <q-separator inset></q-separator>
                                                                    <q-item>
                                                                        <q-item-section  class="">
                                                                            <q-item-label caption>
                                                                                {{ $t('datefield') }}
                                                                            </q-item-label>
                                                                            <q-item-label class="text-bold">
                                                                                <q-chip v-if="props.row.datefield" dense size="13px" :label="$utils.relativeDate(props.row.datefield)">
                                                                                <q-tooltip content-class="bg-accent" transition-show="scale" transition-hide="scale">
                                                                                {{ $utils.humanDatetime(props.row.datefield) }}
                                                                                </q-tooltip>
                                                                                </q-chip>
                                                                            </q-item-label>
                                                                        </q-item-section>
                                                                    </q-item>
                                                                    <q-separator inset></q-separator>
                                                                </q-list>
                                                                <q-separator></q-separator>
                                                                <div class="row justify-between">
                                                                    <div class="q-pa-sm"><q-checkbox dense v-model="props.selected"></q-checkbox></div>
                                                                    <q-card-actions class="row q-col-gutter-xs justify-end">
                                                                        <q-btn icon="menu" padding="xs" round flat color="grey">
                                                                            <q-menu auto-close transition-show="flip-right"  transition-hide="flip-left" self="center middle" anchor="center middle">
                                                                                <q-list dense rounded nav>
                                                                                    <q-item link clickable v-ripple :to="`/sampletable/view/${props.row.id}`">
                                                                                        <q-item-section>
                                                                                            <q-icon color="primary"  size="sm" name="visibility"></q-icon>
                                                                                        </q-item-section>
                                                                                        <q-item-section>{{ $t('view') }}</q-item-section>
                                                                                    </q-item>
                                                                                    <q-item link clickable v-ripple :to="`/sampletable/edit/${props.row.id}`">
                                                                                        <q-item-section>
                                                                                            <q-icon color="positive"  size="sm" name="edit"></q-icon>
                                                                                        </q-item-section>
                                                                                        <q-item-section>{{ $t('edit') }}</q-item-section>
                                                                                    </q-item>
                                                                                    <q-item link clickable v-ripple @click="deleteItem(props.row.id)">
                                                                                        <q-item-section>
                                                                                            <q-icon color="negative"  size="sm" name="clear"></q-icon>
                                                                                        </q-item-section>
                                                                                        <q-item-section>{{ $t('delete') }}</q-item-section>
                                                                                    </q-item>
                                                                                </q-list>
                                                                            </q-menu>
                                                                        </q-btn>
                                                                    </q-card-actions>
                                                                </div>
                                                            </q-card>
                                                        </div>
                                                    </div>
                                                </template>
                                                </q-table>
                                                <div class="row justify-center">
                                                    <q-td></q-td>
                                                </div>
                                            </div>
                                            <!-- page loading indicator template -->
                                            <template v-if="loading">
                                                <q-inner-loading :showing="loading">
                                                    <q-spinner color="primary" size="30px"> 
                                                    </q-spinner>
                                                </q-inner-loading>
                                            </template>
                                            <!-- page empty record template -->
                                            <template v-if="!loading && !records.length">
                                                <q-card :flat="$q.screen.gt.md">
                                                    <q-card-section>
                                                        <div class="text-grey text-h6 text-center">
                                                            {{ $t('noRecordFound') }}
                                                        </div>
                                                    </q-card-section>
                                                </q-card>
                                            </template>
                                            <!-- page footer template -->
                                            <div v-if="showFooter" class="">
                                                <div class="q-pa-sm" v-show="!loading">
                                                    <div class="row justify-between">
                                                        <div class="row q-col-gutter-md">
                                                            <div>
                                                                <q-btn round flat   no-caps  unelevated   color="negative" @click="deleteItem(selectedItems)" v-if="selectedItems.length" icon="delete_sweep" class="q-my-xs" :title="$t('deleteSelected')"></q-btn>
                                                            </div>
                                                        </div>
                                                        <div v-if="paginate && totalRecords > 0" class="row q-col-gutter-md justify-center">
                                                            <div class="col-auto">
                                                                <q-chip>{{ $t('records') }} {{recordsPosition}} {{ $t('of') }} {{totalRecords}}</q-chip>
                                                            </div>
                                                            <div v-if="totalPages > 1">
                                                                <q-pagination  color="primary" flat glossy  input v-model="pagination.page" :direction-links="true" :boundary-links="true" :max-pages="5" :boundary-numbers="true" :max="totalPages"></q-pagination>
                                                            </div>
                                                        </div>
                                                    </div>  
                                                </div>
                                            </div>  
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>
<script setup>
	import {  computed, ref, reactive, toRefs, onMounted } from 'vue';
	import { useMeta } from 'quasar';
	import { useApp } from 'src/composables/app.js';
	import { utils } from 'src/utils';
	import { $t } from 'src/services/i18n';
	import { useListPage } from 'src/composables/listpage.js';
	const props = defineProps({
		primaryKey : {
			type : String,
			default : 'id',
		},
		pageName : {
			type : String,
			default : 'sampletable',
		},
		routeName : {
			type : String,
			default : 'sampletablelist',
		},
		apiPath : {
			type : String,
			default : 'sampletable/index',
		},
		paginate: {
			type: Boolean,
			default: true,
		},
		isSubPage: {
			type: Boolean,
			default: false,
		},
		showHeader: {
			type: Boolean,
			default: true,
		},
		showFooter: {
			type: Boolean,
			default: true,
		},
		showBreadcrumbs: {
			type: Boolean,
			default: true,
		},
		exportButton: {
			type: Boolean,
			default: true,
		},
		importButton: {
			type: Boolean,
			default: false,
		},
		listSequence: {
			type: Boolean,
			default: true,
		},
		multiCheckbox: {
			type: Boolean,
			default: true,
		},
		emptyRecordMsg: {
			type: String,
			default: () => $t('noRecordFound'),
		},
		msgBeforeDelete: {
			type: String,
			default: () => $t('promptDeleteRecord'),
		},
		msgAfterDelete: {
			type: String,
			default: () => $t('recordDeletedSuccessfully'),
		},
		page: {
			type: Number,
			default: 1,
		},
		limit: {
			type: Number,
			default: 10,
		},
		search: {
			type: String,
			default: '',
		},
		fieldName: null,
		fieldValue: null,
		sortBy: {
			type: String,
			default: '',
		},
		sortType: {
			type: String,
			default: '', //desc or asc
		},
	});
	const app = useApp();
			const filters = reactive({});
	//init list page hook
	const page = useListPage(props, filters);
	//page state
	const { 
		totalRecords, // total records from api - Number
		recordCount, // current record count - Number
		loading, // Api loading state - Boolean
		selectedItems, // Data table selected items -Array
		pagination, //Pagination object - Object
		searchText // search text - String
	} = toRefs(page.state);
	//page computed propties
	const { 
		records, // page record from store - Array
		apiUrl, // current api path - URL
		currentRecord, // master detail selected record - Object
		pageBreadCrumb, // get page navigation paths - Object
		canLoadMore, // if api has more data for loading - Boolean
		finishedLoading, // if api has finished loading - Boolean
		totalPages, // total number of pages from api - Number
		recordsPosition // current record position from api - Number
	} = page.computedProps;
	//page methods
	const {
		load, // load data from api
		reload, // reset pagination and load data
		exportPage, // export page records - args = (exportFormats, apiUrl, pageName)
		clearSearch, // clear input search
		onSort, // reset page number before sort from api
		setPagination, // update pagination with data from api
		deleteItem, // delete item by id or selected items - args = (id) or (selectedItems)
		setCurrentRecord, // master detail set current record
		isCurrentRecord, // master detail 
		removeFilter, // remove page filter item - args = (filter.propertyname)
		getFilterLabel, // get filter item display label - args = (filter.propertyname)
		filterHasValue, //check if filter item has value - args = (filter.propertyname)
		importComplete // reload page after data import
	} = page.methods;
	const pageTitle = computed({
		get: function () {
			return $t('sampletable')
		}
	});
	useMeta(() => {
		return {
			title: pageTitle.value //set browser title
		}
	});
	onMounted(()=>{ 
		load();
	});
</script>
<style scoped>
</style>
