import 'package:admin_desktop/src/models/data/count_of_notifications_data.dart';
import 'package:admin_desktop/src/models/data/notification_data.dart';
import 'package:admin_desktop/src/models/data/notification_transactions_data.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

 part 'notification_state.freezed.dart';

@freezed
class NotificationState with _$NotificationState {
  const factory NotificationState({
    @Default([]) List<TransactionModel> transaction,
    @Default(0) int totalCount,
    @Default(false) bool isTransactionsLoading,
    @Default(true) bool hasMoreTransactions,
    @Default([]) List<NotificationModel> notifications,
    @Default(null) CountNotificationModel? countOfNotifications,
    @Default(0) int totalCountNotification,
    @Default(false) bool isNotificationLoading,
    @Default(false) bool isMoreNotificationLoading,
    @Default(true) bool hasMoreNotification,
    @Default(false) bool isReadAllLoading,
    @Default(false) bool isShowUserLoading,
    @Default(false) bool isAllNotificationsLoading,
    @Default(false) bool isFirstTimeNotification,
    @Default(false) bool isFirstTransaction,
    @Default(0) int total,
  }) = _NotificationState;

  const NotificationState._();
}
