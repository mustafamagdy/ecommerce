// ignore_for_file: unrelated_type_equality_checks

import 'dart:ui';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../../../core/utils/utils.dart';
import '../state/pin_code_state.dart';

class PinCodeNotifier extends StateNotifier<PinCodeState> {
  PinCodeNotifier() : super(const PinCodeState());

  void setPinCode({
    required String code,
    required VoidCallback onSuccess,
  }) {
    if (state.pinCode.length < 4) {
      state = state.copyWith(
          pinCode: state.pinCode + code, isPinCodeNotValid: false);
    }
    checkCode(onSuccess: onSuccess, isNotSet: state.pinCode.length == 4);
  }

  void setNewPinCode({
    required String code,
    required VoidCallback onSuccess,
  }) {
    if (state.pinCode.length < 4) {
      state = state.copyWith(
          pinCode: state.pinCode + code, isPinCodeNotValid: false);
    }
  }

  checkCode({
    required VoidCallback onSuccess,
    bool isNotSet = true,
  }) {
    if (state.pinCode.length == 4) {
      String pinCode = LocalStorage.instance.getPinCode();
      if (pinCode == state.pinCode) {
        state = state.copyWith(isPinCodeNotValid: false);
        onSuccess();
      } else {
        state = state.copyWith(isPinCodeNotValid: true);
      }
    } else {
      if (isNotSet) {
        state = state.copyWith(isPinCodeNotValid: true);
      }
    }
  }

  checkNewCode({
    required VoidCallback onSuccess,
  }) {
    if (state.pinCode.length == 4) {
      LocalStorage.instance.setPinCode(state.pinCode);
      onSuccess();
    } else {
      state = state.copyWith(isPinCodeNotValid: true);
    }
  }

  void removePinCode() {
    if (state.isPinCodeNotValid) {
      state = state.copyWith(isPinCodeNotValid: false);
    }
    if (state.pinCode.isNotEmpty) {
      state = state.copyWith(
          pinCode: state.pinCode.substring(0, state.pinCode.length - 1));
    }
  }

  void clearPinCode() {
    state = state.copyWith(pinCode: "", isPinCodeNotValid: false);
  }
}
