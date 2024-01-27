import 'dart:async';
import 'package:admin_desktop/src/core/routes/app_router.gr.dart';
import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/models/response/product_calculate_response.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../../core/constants/constants.dart';
import '../../../../../core/utils/utils.dart';
import '../../../../../models/models.dart';
import '../../../../../repository/repository.dart';
import '../state/main_state.dart';

class MainNotifier extends StateNotifier<MainState> {
  final ProductsRepository _productsRepository;
  final CategoriesRepository _categoriesRepository;
  final BrandsRepository _brandsRepository;
  final UsersRepository _usersRepository;
 
  Timer? _searchProductsTimer;
  Timer? _searchCategoriesTimer;
  Timer? _searchBrandsTimer;
  int _page = 0;

  MainNotifier(
    this._productsRepository,
    this._categoriesRepository,
    this._brandsRepository,
    this._usersRepository,
   
  ) : super(const MainState());

  changeIndex(int index) {
    state = state.copyWith(selectIndex: index);
  }

  setOrder(OrderData? order) {
    state = state.copyWith(selectedOrder: order);
  }

  setPriceDate(PriceDate? priceDate) {
    state = state.copyWith(priceDate: priceDate);
  }

  Future<void> fetchUserDetail({VoidCallback? checkYourNetwork}) async {
    final response = await _usersRepository.getProfileDetails();
    response.when(
      success: (data) async {
        LocalStorage.instance.setUser(data.data);
      },
      failure: (failure) {
        debugPrint('==> get user detail failure: $failure');
      },
    );
  }

  Future<void> fetchProducts({
    VoidCallback? checkYourNetwork,
  }) async {
    if (!state.hasMore) {
      return;
    }
    final connected = await AppConnectivity.connectivity();
    if (connected) {
      if (_page == 0) {
        state = state.copyWith(isProductsLoading: true, products: []);

        final response = await _productsRepository.getProductsPaginate(
          page: ++_page,
          query: state.query.isEmpty ? null : state.query,
          shopId: state.selectedShop == null ? null : state.selectedShop!.id,
          categoryId: state.selectedCategory == null
              ? null
              : state.selectedCategory!.id,
          brandId: state.selectedBrand == null ? null : state.selectedBrand!.id,
        );
        response.when(
          success: (data) {
            state = state.copyWith(
              products: data.data ?? [],
              isProductsLoading: false,
            );
            if ((data.data?.length ?? 0) < 12) {
              state = state.copyWith(hasMore: false);
            }
          },
          failure: (failure) {
            state = state.copyWith(isProductsLoading: false);
            debugPrint('==> get products failure: $failure');
          },
        );
      } else {
        state = state.copyWith(isMoreProductsLoading: true);
        final response = await _productsRepository.getProductsPaginate(
          page: ++_page,
          query: state.query.isEmpty ? null : state.query,
          shopId: state.selectedShop == null ? null : state.selectedShop!.id,
          categoryId: state.selectedCategory == null
              ? null
              : state.selectedCategory!.id,
          brandId: state.selectedBrand == null ? null : state.selectedBrand!.id,
        );
        response.when(
          success: (data) async {
            final List<ProductData> newList = List.from(state.products);
            newList.addAll(data.data ?? []);
            state = state.copyWith(
              products: newList,
              isMoreProductsLoading: false,
            );
            if ((data.data?.length ?? 0) < 12) {
              state = state.copyWith(hasMore: false);
            }
          },
          failure: (failure) {
            state = state.copyWith(isMoreProductsLoading: false);
            debugPrint('==> get products more failure: $failure');
          },
        );
      }
    } else {
      checkYourNetwork?.call();
    }
  }

  void setProductsQuery(BuildContext context, String query) {
    if (state.query == query) {
      return;
    }
    state = state.copyWith(query: query.trim());
    if (state.query.isNotEmpty) {
      if (_searchProductsTimer?.isActive ?? false) {
        _searchProductsTimer?.cancel();
      }
      _searchProductsTimer = Timer(
        const Duration(milliseconds: 500),
        () {
          state = state.copyWith(hasMore: true, products: []);
          _page = 0;
          fetchProducts(
            checkYourNetwork: () {
              AppHelpers.showSnackBar(
                context,
                AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
              );
            },
          );
        },
      );
    } else {
      if (_searchProductsTimer?.isActive ?? false) {
        _searchProductsTimer?.cancel();
      }
      _searchProductsTimer = Timer(
        const Duration(milliseconds: 500),
        () {
          state = state.copyWith(hasMore: true, products: []);
          _page = 0;
          fetchProducts(
            checkYourNetwork: () {
              AppHelpers.showSnackBar(
                context,
                AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
              );
            },
          );
        },
      );
    }
  }

  Future<void> fetchCategories({required BuildContext context,VoidCallback? checkYourNetwork}) async {
    final connected = await AppConnectivity.connectivity();
    if (connected) {
      state = state.copyWith(
        isCategoriesLoading: true,
        dropDownCategories: [],
        categories: [],
      );
      final response = await _categoriesRepository.searchCategories(
          state.categoryQuery.isEmpty ? null : state.categoryQuery);
      response.when(
        success: (data) async {
          final List<CategoryData> categories = data.data ?? [];
          state = state.copyWith(
            isCategoriesLoading: false,
            categories: categories,
          );
        },
        failure: (failure) {
          state = state.copyWith(isCategoriesLoading: false);
          failure.mapOrNull(unauthorisedRequest:(s){
            context.replaceRoute(const LoginRoute());
            LocalStorage.instance.clearStore();
          } );

        },
      );
    } else {
      checkYourNetwork?.call();
    }
  }

  void setCategoriesQuery(BuildContext context, String query) {
    debugPrint('===> set categories query: $query');
    if (state.categoryQuery == query) {
      return;
    }
    state = state.copyWith(categoryQuery: query.trim());
    if (_searchCategoriesTimer?.isActive ?? false) {
      _searchCategoriesTimer?.cancel();
    }
    _searchCategoriesTimer = Timer(
      const Duration(milliseconds: 500),
      () {
        state = state.copyWith(categories: [], dropDownCategories: []);
        fetchCategories(context: context,
          checkYourNetwork: () {
            AppHelpers.showSnackBar(
              context,
              AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
            );
          },
        );
      },
    );
  }

  void setSelectedCategory(BuildContext context, int index) {
    if (index == -1) {
      state = state.copyWith(selectedCategory: null, hasMore: true);
    } else {
      final category = state.categories[index];
      if (category.id != state.selectedCategory?.id) {
        state = state.copyWith(selectedCategory: category, hasMore: true);
      } else {
        state = state.copyWith(selectedCategory: null, hasMore: true);
      }
    }

    _page = 0;
    fetchProducts(
      checkYourNetwork: () {
        AppHelpers.showSnackBar(
          context,
          AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
        );
      },
    );
    setCategoriesQuery(context, '');
  }

  void removeSelectedCategory(BuildContext context) {
    state = state.copyWith(selectedCategory: null, hasMore: true);
    _page = 0;
    fetchProducts(
      checkYourNetwork: () {
        AppHelpers.showSnackBar(
          context,
          AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
        );
      },
    );
  }

  Future<void> fetchBrands({VoidCallback? checkYourNetwork}) async {
    final connected = await AppConnectivity.connectivity();
    if (connected) {
      state = state.copyWith(
        isBrandsLoading: true,
        dropDownBrands: [],
        brands: [],
      );
      final response = await _brandsRepository
          .searchBrands(state.brandQuery.isEmpty ? null : state.brandQuery);
      response.when(
        success: (data) async {
          final List<BrandData> brands = data.data ?? [];
          List<DropDownItemData> dropdownBrands = [];
          for (int i = 0; i < brands.length; i++) {
            dropdownBrands.add(
              DropDownItemData(
                index: i,
                title: brands[i].title ?? 'No category title',
              ),
            );
          }
          state = state.copyWith(
            isBrandsLoading: false,
            brands: brands,
            dropDownBrands: dropdownBrands,
          );
        },
        failure: (failure) {
          state = state.copyWith(isBrandsLoading: false);
          debugPrint('==> get brands failure: $failure');
        },
      );
    } else {
      checkYourNetwork?.call();
    }
  }

  void setBrandsQuery(BuildContext context, String query) {
    if (state.brandQuery == query) {
      return;
    }
    state = state.copyWith(brandQuery: query.trim());
    if (_searchBrandsTimer?.isActive ?? false) {
      _searchBrandsTimer?.cancel();
    }
    _searchBrandsTimer = Timer(
      const Duration(milliseconds: 500),
      () {
        state = state.copyWith(brands: [], dropDownBrands: []);
        fetchBrands(
          checkYourNetwork: () {
            AppHelpers.showSnackBar(
              context,
              AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
            );
          },
        );
      },
    );
  }

  void setSelectedBrand(BuildContext context, int index) {
    final brand = state.brands[index];
    state = state.copyWith(selectedBrand: brand, hasMore: true);
    _page = 0;
    fetchProducts(
      checkYourNetwork: () {
        AppHelpers.showSnackBar(
          context,
          AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
        );
      },
    );
    setBrandsQuery(context, '');
  }

  void removeSelectedBrand(BuildContext context) {
    state = state.copyWith(selectedBrand: null, hasMore: true);
    _page = 0;
    fetchProducts(
      checkYourNetwork: () {
        AppHelpers.showSnackBar(
          context,
          AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
        );
      },
    );
  }

  }
