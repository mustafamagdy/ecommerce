import '../core/handlers/handlers.dart';
import '../models/models.dart';

abstract class BrandsRepository {
  Future<ApiResult<BrandsPaginateResponse>> searchBrands(String? query);
}
