import 'package:freezed_annotation/freezed_annotation.dart';

part 'login_state.freezed.dart';

@freezed
class LoginState with _$LoginState {
  const factory LoginState({
    @Default(false) bool isLoading,
    @Default(false) bool showPassword,
    @Default(false) bool isCurrenciesLoading,
    @Default(false) bool isLoginError,
    @Default(false) bool isEmailNotValid,
    @Default(false) bool isPasswordNotValid,
    @Default('') String email,
    @Default('') String password,
  }) = _LoginState;

  const LoginState._();
}