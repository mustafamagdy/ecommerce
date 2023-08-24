import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'canceled_orders_state.dart';
import 'canceled_orders_notifier.dart';

final canceledOrdersProvider =
    StateNotifierProvider<CanceledOrdersNotifier, CanceledOrdersState>(
  (ref) => CanceledOrdersNotifier(ordersRepository),
);
