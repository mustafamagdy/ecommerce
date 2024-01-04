import 'package:admin_desktop/src/models/data/notification_data.dart';
import 'package:admin_desktop/src/models/data/notification_transactions_data.dart';

import '../core/handlers/handlers.dart';
import '../models/data/count_of_notifications_data.dart';
import '../models/data/read_one_notification_data.dart';

abstract class NotificationRepository {
  Future<ApiResult<TransactionListResponse>> getTransactions({
    int? page,
  });

  Future<ApiResult<NotificationResponse>> getNotifications({
    int? page,
  });

  Future<ApiResult<NotificationResponse>> getAllNotifications();

  Future<ApiResult<ReadOneNotificationResponse>> readOne({
    int? id,
  });

  Future<ApiResult<NotificationResponse>> readAll();

  Future<ApiResult<NotificationResponse>> showSingleUser({
    int? id,
  });

  Future<ApiResult<CountNotificationModel>> getCount();
}
