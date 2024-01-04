import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'new_orders_state.dart';
import 'new_orders_notifier.dart';

final newOrdersProvider =
    StateNotifierProvider<NewOrdersNotifier, NewOrdersState>(
  (ref) => NewOrdersNotifier(ordersRepository),
);
