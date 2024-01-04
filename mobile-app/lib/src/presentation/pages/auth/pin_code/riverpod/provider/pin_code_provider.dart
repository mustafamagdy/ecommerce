import 'package:admin_desktop/src/presentation/pages/auth/pin_code/riverpod/notifier/pin_code_notifier.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../state/pin_code_state.dart';

final pinCodeProvider =
    StateNotifierProvider.autoDispose<PinCodeNotifier, PinCodeState>(
  (ref) => PinCodeNotifier(),
);
