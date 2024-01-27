import 'dart:convert';

import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/models/response/income_chart_response.dart';
import 'package:admin_desktop/src/models/response/income_statistic_response.dart';
import 'package:admin_desktop/src/models/response/sale_cart_response.dart';
import 'package:flutter/material.dart';

import '../../core/di/injection.dart';
import '../../core/handlers/handlers.dart';
import '../../core/utils/utils.dart';
import '../../models/models.dart';
import '../../models/response/income_cart_response.dart';
import '../../models/response/sale_history_response.dart';
import '../repository.dart';

class SettingsSettingsRepositoryImpl extends SettingsRepository {
  @override
  Future<ApiResult<GlobalSettingsResponse>> getGlobalSettings() async {
    try {
      final client = inject<HttpService>().client(requireAuth: false);
      final response = await client.get('/api/v1/rest/settings');
      debugPrint('==> get global settings response: $response');
      return ApiResult.success(
        data: GlobalSettingsResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get settings failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<TranslationsResponse>> getTranslations() async {
    final data = {
      'lang': LocalStorage.instance.getActiveLocale().isEmpty
          ? 'en'
          : LocalStorage.instance.getActiveLocale()
    };
    try {
      final client = inject<HttpService>().client(requireAuth: false);
      final response = await client.get(
        '/api/v1/rest/translations/paginate',
        queryParameters: data,
      );
      return ApiResult.success(
        data: TranslationsResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get translations failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<SaleHistoryResponse>> getSaleHistory(
      int type, int page) async {
    final data = {
      'type': type == 0
          ? "deliveryman"
          : type == 1
              ? "today"
              : "history",
      "perPage": 10,
      "page": page,
      "sort": "desc",
      "column": "created_at"
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/sales-history',
        queryParameters: data,
      );
      return ApiResult.success(
        data: SaleHistoryResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get sale history failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<SaleCartResponse>> getSaleCart() async {
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/sales-cards',
      );
      return ApiResult.success(
        data: SaleCartResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get sale cart failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }


  @override
  Future<ApiResult<IncomeStatisticResponse>> getIncomeStatistic(
      {required String type,
        required DateTime? from,
        required DateTime? to}) async {
    try {
      final data = {
        "type": type,
        "date_from": from.toString().substring(0, from.toString().indexOf(" ")),
        "date_to": to.toString().substring(0, to.toString().indexOf(" "))
      };
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
          '/api/v1/dashboard/seller/sales-statistic',
          queryParameters: data);
      return ApiResult.success(
        data: IncomeStatisticResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get sale statistic failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<IncomeCartResponse>> getIncomeCart(
      {required String type,
      required DateTime? from,
      required DateTime? to}) async {
    try {
      final data = {
        "type": type,
        "date_from": from.toString().substring(0, from.toString().indexOf(" ")),
        "date_to": to.toString().substring(0, to.toString().indexOf(" "))
      };
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
          '/api/v1/dashboard/seller/sales-main-cards',
          queryParameters: data);
      return ApiResult.success(
        data: IncomeCartResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get sale cart failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<List<IncomeChartResponse>>> getIncomeChart(
      {required String type,
      required DateTime? from,
      required DateTime? to}) async {
    try {
      final data = {
        "type": type == TrKeys.week ? TrKeys.month : type,
        "date_from": from.toString().substring(0, from.toString().indexOf(" ")),
        "date_to": to.toString().substring(0, to.toString().indexOf(" "))
      };
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get('/api/v1/dashboard/seller/sales-chart',
          queryParameters: data);
      return ApiResult.success(
        data: incomeChartResponseFromJson(jsonEncode(response.data)),
      );
    } catch (e) {
      debugPrint('==> get sale cart failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

// @override
// Future<ApiResult<LanguagesResponse>> getLanguages() async {
//   try {
//     final client = inject<HttpService>().client(requireAuth: false);
//     final response = await client.get('/api/v1/rest/languages/active');
//     return ApiResult.success(
//       data: LanguagesResponse.fromJson(response.data),
//     );
//   } catch (e) {
//     debugPrint('==> get languages failure: $e');
//     return ApiResult.failure(error: NetworkExceptions.getDioException(e));
//   }
// }
}
