import 'package:flutter/material.dart';

import '../../core/di/injection.dart';
import '../../core/handlers/handlers.dart';
import '../../models/models.dart';
import '../repository.dart';

class BrandsRepositoryImpl extends BrandsRepository {
  @override
  Future<ApiResult<BrandsPaginateResponse>> searchBrands(String? query) async {
    final data = {'search': query};
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/rest/brands/paginate',
        queryParameters: data,
      );
      return ApiResult.success(
        data: BrandsPaginateResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> search brands failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }
}
