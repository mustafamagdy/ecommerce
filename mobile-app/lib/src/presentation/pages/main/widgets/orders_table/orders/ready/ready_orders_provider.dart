import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'ready_orders_state.dart';
import 'ready_orders_notifier.dart';

final readyOrdersProvider =
    StateNotifierProvider<ReadyOrdersNotifier, ReadyOrdersState>(
  (ref) => ReadyOrdersNotifier(ordersRepository),
);
