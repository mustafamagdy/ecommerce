import 'package:freezed_annotation/freezed_annotation.dart';

part 'pin_code_state.freezed.dart';

@freezed
class PinCodeState with _$PinCodeState {
  const factory PinCodeState({
    @Default(false) bool isPinCodeNotValid,
    @Default('') String pinCode,
  }) = _PinCodeState;

  const PinCodeState._();
}