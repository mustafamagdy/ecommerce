import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'kitchen_notifier.dart';
import 'kitchen_state.dart';

final kitchenProvider =
    StateNotifierProvider<KitchenNotifier, KitchenState>(
  (ref) => KitchenNotifier(ordersRepository),
);
