import 'package:flutter/material.dart';

import '../../core/di/injection.dart';
import '../../core/handlers/handlers.dart';
import '../../models/models.dart';
import '../repository.dart';

class CurrenciesRepositoryImpl extends CurrenciesRepository {
  @override
  Future<ApiResult<CurrenciesResponse>> getCurrencies() async {
    try {
      final client = inject<HttpService>().client(requireAuth: false);
      final response = await client.get('/api/v1/rest/currencies');
      return ApiResult.success(
        data: CurrenciesResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get currencies failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }
}
