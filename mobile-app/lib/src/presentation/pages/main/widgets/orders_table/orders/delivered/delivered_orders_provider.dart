import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'delivered_orders_state.dart';
import 'delivered_orders_notifier.dart';

final deliveredOrdersProvider =
    StateNotifierProvider<DeliveredOrdersNotifier, DeliveredOrdersState>(
  (ref) => DeliveredOrdersNotifier(ordersRepository),
);
