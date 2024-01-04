import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'accepted_orders_state.dart';
import 'accepted_orders_notifier.dart';

final acceptedOrdersProvider =
    StateNotifierProvider<AcceptedOrdersNotifier, AcceptedOrdersState>(
  (ref) => AcceptedOrdersNotifier(ordersRepository),
);
