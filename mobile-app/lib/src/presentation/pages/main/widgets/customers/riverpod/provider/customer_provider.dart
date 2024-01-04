import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/customers/riverpod/state/customer_state.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../notifier/customer_notifier.dart';

final customerProvider =
    StateNotifierProvider.autoDispose<CustomerNotifier, CustomerState>(
  (ref) => CustomerNotifier(
    usersRepository,
  ),
);
