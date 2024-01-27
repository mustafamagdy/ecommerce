import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../../../../core/di/dependency_manager.dart';
import '../notifier/right_side_notifier.dart';
import '../state/right_side_state.dart';

final rightSideProvider =
    StateNotifierProvider<RightSideNotifier, RightSideState>(
  (ref) => RightSideNotifier(usersRepository, currenciesRepository,
      paymentsRepository, productsRepository, ordersRepository),
);
