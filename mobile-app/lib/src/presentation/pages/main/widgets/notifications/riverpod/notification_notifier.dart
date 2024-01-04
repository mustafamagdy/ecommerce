import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:admin_desktop/src/core/utils/utils.dart';
import 'package:admin_desktop/src/models/data/notification_data.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/notifications/riverpod/notification_state.dart';
import 'package:admin_desktop/src/repository/notification_repository.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../../../models/data/notification_transactions_data.dart';

class NotificationNotifier extends StateNotifier<NotificationState> {
  final NotificationRepository _notificationRepository;

  int _page = 0;
  int _notificationPage = 0;

  NotificationNotifier(this._notificationRepository)
      : super(const NotificationState());

  Future<void> fetchAllTransactions({
    bool isRefresh = false,
    Function(int)? updateTotal,
  }) async {
    if (isRefresh) {
      _page = 0;
      state = state.copyWith(hasMoreTransactions: true);
    }
    if (!state.hasMoreTransactions) {
      return;
    }
    state = state.copyWith(
        isTransactionsLoading: state.transaction.isEmpty ? true : false);
    final response =
        await _notificationRepository.getTransactions(page: ++_page);
    response.when(
      success: (data) {
        List<TransactionModel> transactions =
            isRefresh ? [] : List.from(state.transaction);
        final List<TransactionModel> newTransactions = data.data ?? [];
        transactions.addAll(newTransactions);
        state =
            state.copyWith(hasMoreTransactions: newTransactions.length >= 4);
        if (_page == 1 && !isRefresh) {
          state = state.copyWith(
            isTransactionsLoading: false,
            transaction: transactions,
          );
        } else {
          state = state.copyWith(
            isTransactionsLoading: false,
            transaction: transactions,
          );
        }
      },
      failure: (failure) {
        _page--;
        if (_page == 0) {
          state = state.copyWith(isTransactionsLoading: false);
        }
      },
    );
  }

  changeFirst() {
    state =
        state.copyWith(isFirstTimeNotification: true, isFirstTransaction: true);
  }

  Future<void> fetchAllNotifications(BuildContext context) async {
    state = state.copyWith(isNotificationLoading: true);

    final response = await _notificationRepository.getAllNotifications();
    response.when(
      success: (data) {
        state = state.copyWith(
            isNotificationLoading: false, notifications: data.data ?? []);
      },
      failure: (failure) {
        AppHelpers.showFlashBar(context, failure.toString());
      },
    );
  }

  Future<void> fetchNotificationsPaginate({
    VoidCallback? checkYourNetwork,
  }) async {
    if (!state.hasMoreNotification) {
      return;
    }
    final connected = await AppConnectivity.connectivity();
    if (connected) {
      if (_notificationPage == 0) {
        state = state.copyWith(isNotificationLoading: true, notifications: []);

        final response = await notificationRepository.getNotifications(
          page: ++_notificationPage,
        );
        response.when(
          success: (data) {
            state = state.copyWith(
              notifications: data.data ?? [],
              isNotificationLoading: false,
            );
            if ((data.data?.length ?? 0) < 5) {
              state = state.copyWith(hasMoreNotification: false);
            }
          },
          failure: (failure) {
            state = state.copyWith(isNotificationLoading: false);
            debugPrint('==> get products failure: $failure');
          },
        );
      } else {
        state = state.copyWith(isMoreNotificationLoading: true);
        final response = await notificationRepository.getNotifications(
          page: ++_notificationPage,
        );
        response.when(
          success: (data) async {
            final List<NotificationModel> newList =
                List.from(state.notifications);
            newList.addAll(data.data ?? []);
            state = state.copyWith(
              notifications: newList,
              isMoreNotificationLoading: false,
            );
            if ((data.data?.length ?? 0) < 5) {
              state = state.copyWith(hasMoreNotification: false);
            }
          },
          failure: (failure) {
            state = state.copyWith(isMoreNotificationLoading: false);
            debugPrint('==> get notifications more failure: $failure');
          },
        );
      }
    } else {
      checkYourNetwork?.call();
    }
  }

  Future<void> readAll(BuildContext context) async {
    List<NotificationModel> notif = List.from(state.notifications);
    for (var i = 0; i < notif.length; i++) {
      if (notif[i].readAt == null) {
        notif[i] = notif[i].copyWith(readAt: DateTime.now());
      }
    }
    state = state.copyWith(
      notifications: notif,
      countOfNotifications:
          state.countOfNotifications?.copyWith(notification: 0),
    );
    updateTotal();

    final response = await _notificationRepository.readAll();
    response.when(
      success: (data) {
        debugPrint('Read all success');
      },
      failure: (failure) {
        AppHelpers.showFlashBar(context, failure.toString());
      },
    );
  }

  Future<void> readOne(BuildContext context,
      {int? id, required int index}) async {
    List<NotificationModel> notif = List.from(state.notifications);
    notif[index] = notif[index].copyWith(
      readAt: DateTime.now(),
    );
    final notification = state.countOfNotifications?.copyWith(
        notification: (state.countOfNotifications?.notification ?? 0) - 1);
    state = state.copyWith(
        notifications: notif, countOfNotifications: notification);
    updateTotal();
    final response = await _notificationRepository.readOne(id: id);
    response.when(
      success: (data) {
        debugPrint('Success read one');
      },
      failure: (failure) {
        AppHelpers.showFlashBar(context, failure.toString());
      },
    );
  }

  Future<void> fetchCount(BuildContext context) async {
    final response = await _notificationRepository.getCount();
    response.when(
      success: (data) {
        state = state.copyWith(countOfNotifications: data);
        state = state.copyWith(
            totalCount: (data.notification ?? 0) + (data.transaction ?? 0));

        debugPrint('Success count');
      },
      failure: (failure) {
        AppHelpers.showFlashBar(context, failure.toString());
      },
    );
  }

  updateTotal() {
    state = state.copyWith(
        totalCount: (state.countOfNotifications?.notification ?? 0) +
            (state.countOfNotifications?.transaction ?? 0));
  }
}
