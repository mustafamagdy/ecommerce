import 'package:admin_desktop/src/models/data/count_of_notifications_data.dart';
import 'package:admin_desktop/src/models/data/notification_data.dart';
import 'package:admin_desktop/src/models/data/notification_transactions_data.dart';
import 'package:admin_desktop/src/models/data/read_one_notification_data.dart';
import 'package:admin_desktop/src/repository/notification_repository.dart';
import 'package:flutter/material.dart';
import '../../core/di/injection.dart';
import '../../core/handlers/handlers.dart';
import '../../core/utils/local_storage.dart';

class NotificationRepositoryImpl extends NotificationRepository {
  @override
  Future<ApiResult<TransactionListResponse>> getTransactions(
      {int? page}) async {
    final data = {
      if (page != null) 'page': page,
      'perPage': 4,
      'lang': LocalStorage.instance.getActiveLocale(),
      'model': 'orders'
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/transactions/paginate',
        queryParameters: data,
      );
      return ApiResult.success(
        data: TransactionListResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get transaction failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<NotificationResponse>> getNotifications({
    int? page,
  }) async {
    final data = {
      if (page != null) 'page': page,
      'column': 'created_at',
      'sort': 'desc',
      'perPage': 5,
      'lang': LocalStorage.instance.getActiveLocale(),
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/notifications',
        queryParameters: data,
      );
      return ApiResult.success(
        data: NotificationResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get notification failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<NotificationResponse>> readAll() async {
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.post(
        '/api/v1/dashboard/notifications/read-all',
      );
      return ApiResult.success(
        data: NotificationResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get notification failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<ReadOneNotificationResponse>> readOne({int? id}) async {
    final data = {
      if (id != null) '$id': id,
      // 'lang': LocalStorage.instance.getActiveLocale(),
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.post(
        '/api/v1/dashboard/notifications/$id/read-at',
        queryParameters: data,
      );
      return ApiResult.success(
        data: ReadOneNotificationResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get notification failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<NotificationResponse>> showSingleUser({int? id}) async {
    final data = {
      if (id != null) '$id': id,
      'lang': LocalStorage.instance.getActiveLocale(),
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.post(
        '/api/v1/dashboard/notifications/$id',
        queryParameters: data,
      );
      return ApiResult.success(
        data: NotificationResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get notification failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<NotificationResponse>> getAllNotifications() async {
    final data = {
      'lang': LocalStorage.instance.getActiveLocale(),
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/notifications',
        queryParameters: data,
      );
      return ApiResult.success(
        data: NotificationResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get notification failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<CountNotificationModel>> getCount() async {
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/user/profile/notifications-statistic',
      );
      return ApiResult.success(
        data: CountNotificationModel.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get notification failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }
}
