import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/models/response/product_calculate_response.dart';
import 'package:flutter/material.dart';

import '../../core/di/injection.dart';
import '../../core/handlers/handlers.dart';
import '../../core/utils/utils.dart';
import '../../models/models.dart';
import '../repository.dart';

class ProductsRepositoryImpl extends ProductsRepository {
  @override
  Future<ApiResult<ProductsPaginateResponse>> getProductsPaginate({
    String? query,
    int? categoryId,
    int? brandId,
    int? shopId,
    required int page,
  }) async {
    final data = {
      if (brandId != null) 'brand_id': brandId,
      if (categoryId != null) 'category_id': categoryId,
      if (shopId != null) 'shop_id': shopId,
      if (query != null) 'search': query,
      'perPage': 12,
      'page': page,
      'lang': LocalStorage.instance.getActiveLocale(),
      "status": "published",
      "addon_status": "published"
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/products/paginate',
        queryParameters: data,
      );
      return ApiResult.success(
        data: ProductsPaginateResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get products failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<ProductCalculateResponse>> getAllCalculations(
      List<BagProductData> bagProducts, String type,
      {String? coupon}) async {
    final data = {
      'currency_id': LocalStorage.instance.getSelectedCurrency().id,
      'shop_id': LocalStorage.instance.getUser()?.shop?.id ?? 0,
      'type': type.isEmpty ? TrKeys.pickup : type,
      if (coupon != null) "coupon": coupon,
      'address[latitude]': LocalStorage.instance
              .getBags()
              .first
              .selectedAddress
              ?.location
              ?.latitude ??
          0,
      'address[longitude]': LocalStorage.instance
              .getBags()
              .first
              .selectedAddress
              ?.location
              ?.longitude ??
          0
    };
    for (int i = 0; i < (bagProducts.length); i++) {
      data['products[$i][stock_id]'] = bagProducts[i].stockId;
      data['products[$i][quantity]'] = bagProducts[i].quantity;
      for (int j = 0; j < (bagProducts[i].carts?.length ?? 0); j++) {
        data['products[$i$j][stock_id]'] = bagProducts[i].carts?[j].stockId;
        data['products[$i$j][quantity]'] = bagProducts[i].carts?[j].quantity;
        data['products[$i$j][parent_id]'] = bagProducts[i].carts?[j].parentId;
      }
    }

    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/rest/order/products/calculate',
        queryParameters: data,
      );
      return ApiResult.success(
        data: ProductCalculateResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get all calculations failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }
}
