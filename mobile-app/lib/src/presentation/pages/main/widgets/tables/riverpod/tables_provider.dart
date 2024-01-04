
import 'package:admin_desktop/src/presentation/pages/main/widgets/tables/riverpod/tables_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/tables/riverpod/tables_state.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final tablesProvider = StateNotifierProvider<TablesNotifier, TablesState>(
  (ref) => TablesNotifier(),

);
