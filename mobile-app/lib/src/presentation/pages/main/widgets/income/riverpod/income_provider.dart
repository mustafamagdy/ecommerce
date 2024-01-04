import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'income_notifier.dart';
import 'income_state.dart';

final incomeProvider =
    StateNotifierProvider<IncomeNotifier, IncomeState>(
  (ref) => IncomeNotifier(settingsRepository),
);
