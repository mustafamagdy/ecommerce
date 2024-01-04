import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'order_details_state.dart';
import 'order_details_notifier.dart';

final orderDetailsProvider =
    StateNotifierProvider<OrderDetailsNotifier, OrderDetailsState>(
  (ref) => OrderDetailsNotifier(ordersRepository, usersRepository),
);
