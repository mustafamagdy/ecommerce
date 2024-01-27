import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/models/response/product_calculate_response.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../../../models/models.dart';

part 'main_state.freezed.dart';

@freezed
class MainState with _$MainState {
  const factory MainState({
    @Default(false) bool isProductsLoading,
    @Default(false) bool isMoreProductsLoading,
    @Default(false) bool isShopsLoading,
    @Default(false) bool isBrandsLoading,
    @Default(false) bool isCategoriesLoading,
    @Default(true) bool hasMore,
    @Default(0) int selectIndex,
    @Default([]) List<ProductData> products,
    @Default([]) List<ShopData> shops,
    @Default([]) List<CategoryData> categories,
    @Default([]) List<BrandData> brands,
    @Default([]) List<DropDownItemData> dropDownShops,
    @Default([]) List<DropDownItemData> dropDownCategories,
    @Default([]) List<DropDownItemData> dropDownBrands,
    @Default('') String query,
    @Default('') String shopQuery,
    @Default('') String categoryQuery,
    @Default('') String brandQuery,
    ShopData? selectedShop,
    CategoryData? selectedCategory,
    BrandData? selectedBrand,
    OrderData? selectedOrder,
    PriceDate? priceDate,
  }) = _MainState;

  const MainState._();
}
