import '../core/handlers/handlers.dart';
import '../models/models.dart';

abstract class ShopsRepository {
  Future<ApiResult<ShopsPaginateResponse>> searchShops(String? query);

  Future<ApiResult<ShopsPaginateResponse>> getShopsByIds(
    List<int> shopIds,
  );

  Future<ApiResult<ShopDeliveriesResponse>> getOnlyDeliveries();
}
