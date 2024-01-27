import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../notifier/add_product_notifier.dart';
import '../riverpod/add_product_state.dart';

final addProductProvider =
    StateNotifierProvider<AddProductNotifier, AddProductState>(
  (ref) => AddProductNotifier(),
);
