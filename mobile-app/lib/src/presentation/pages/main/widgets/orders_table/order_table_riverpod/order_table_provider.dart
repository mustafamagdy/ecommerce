import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'order_table_notifier.dart';
import 'order_table_state.dart';


final orderTableProvider =
    StateNotifierProvider<OrderTableNotifier, OrderTableState>(
  (ref) => OrderTableNotifier(),
);
