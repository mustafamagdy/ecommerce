import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'on_a_way_orders_state.dart';
import 'on_a_way_orders_notifier.dart';

final onAWayOrdersProvider =
    StateNotifierProvider<OnAWayOrdersNotifier, OnAWayOrdersState>(
  (ref) => OnAWayOrdersNotifier(ordersRepository),
);
