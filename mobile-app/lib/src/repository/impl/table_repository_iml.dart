import 'package:admin_desktop/src/core/di/injection.dart';
import 'package:admin_desktop/src/models/response/table_status_response.dart';
import 'package:admin_desktop/src/repository/table_repository.dart';
import 'package:flutter/material.dart';

import '../../core/handlers/handlers.dart';
import '../../core/utils/utils.dart';

class TableRepositoryIml extends TableRepository{
  @override
  Future<ApiResult<TableResponse>> getTableInfo({int? page}) async {
    final data = {
      if (page != null) 'page': page,
      'perPage': 10,
      'lang': LocalStorage.instance.getActiveLocale(),
      "shop_section_id":1
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/tables',
        queryParameters: data,
      );
      return ApiResult.success(
        data: TableResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get transaction failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }
}