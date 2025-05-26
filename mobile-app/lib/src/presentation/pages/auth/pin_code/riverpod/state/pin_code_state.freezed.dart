// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target

part of 'pin_code_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
    'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#custom-getters-and-methods');

/// @nodoc
mixin _$PinCodeState {
  bool get isPinCodeNotValid => throw _privateConstructorUsedError;
  String get pinCode => throw _privateConstructorUsedError;

  @JsonKey(ignore: true)
  $PinCodeStateCopyWith<PinCodeState> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $PinCodeStateCopyWith<$Res> {
  factory $PinCodeStateCopyWith(
          PinCodeState value, $Res Function(PinCodeState) then) =
      _$PinCodeStateCopyWithImpl<$Res>;
  $Res call({bool isPinCodeNotValid, String pinCode});
}

/// @nodoc
class _$PinCodeStateCopyWithImpl<$Res> implements $PinCodeStateCopyWith<$Res> {
  _$PinCodeStateCopyWithImpl(this._value, this._then);

  final PinCodeState _value;
  // ignore: unused_field
  final $Res Function(PinCodeState) _then;

  @override
  $Res call({
    Object? isPinCodeNotValid = freezed,
    Object? pinCode = freezed,
  }) {
    return _then(_value.copyWith(
      isPinCodeNotValid: isPinCodeNotValid == freezed
          ? _value.isPinCodeNotValid
          : isPinCodeNotValid // ignore: cast_nullable_to_non_nullable
              as bool,
      pinCode: pinCode == freezed
          ? _value.pinCode
          : pinCode // ignore: cast_nullable_to_non_nullable
              as String,
    ));
  }
}

/// @nodoc
abstract class _$$_PinCodeStateCopyWith<$Res>
    implements $PinCodeStateCopyWith<$Res> {
  factory _$$_PinCodeStateCopyWith(
          _$_PinCodeState value, $Res Function(_$_PinCodeState) then) =
      __$$_PinCodeStateCopyWithImpl<$Res>;
  @override
  $Res call({bool isPinCodeNotValid, String pinCode});
}

/// @nodoc
class __$$_PinCodeStateCopyWithImpl<$Res>
    extends _$PinCodeStateCopyWithImpl<$Res>
    implements _$$_PinCodeStateCopyWith<$Res> {
  __$$_PinCodeStateCopyWithImpl(
      _$_PinCodeState _value, $Res Function(_$_PinCodeState) _then)
      : super(_value, (v) => _then(v as _$_PinCodeState));

  @override
  _$_PinCodeState get _value => super._value as _$_PinCodeState;

  @override
  $Res call({
    Object? isPinCodeNotValid = freezed,
    Object? pinCode = freezed,
  }) {
    return _then(_$_PinCodeState(
      isPinCodeNotValid: isPinCodeNotValid == freezed
          ? _value.isPinCodeNotValid
          : isPinCodeNotValid // ignore: cast_nullable_to_non_nullable
              as bool,
      pinCode: pinCode == freezed
          ? _value.pinCode
          : pinCode // ignore: cast_nullable_to_non_nullable
              as String,
    ));
  }
}

/// @nodoc

class _$_PinCodeState extends _PinCodeState {
  const _$_PinCodeState({this.isPinCodeNotValid = false, this.pinCode = ''})
      : super._();

  @override
  @JsonKey()
  final bool isPinCodeNotValid;
  @override
  @JsonKey()
  final String pinCode;

  @override
  String toString() {
    return 'PinCodeState(isPinCodeNotValid: $isPinCodeNotValid, pinCode: $pinCode)';
  }

  @override
  bool operator ==(dynamic other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$_PinCodeState &&
            const DeepCollectionEquality()
                .equals(other.isPinCodeNotValid, isPinCodeNotValid) &&
            const DeepCollectionEquality().equals(other.pinCode, pinCode));
  }

  @override
  int get hashCode => Object.hash(
      runtimeType,
      const DeepCollectionEquality().hash(isPinCodeNotValid),
      const DeepCollectionEquality().hash(pinCode));

  @JsonKey(ignore: true)
  @override
  _$$_PinCodeStateCopyWith<_$_PinCodeState> get copyWith =>
      __$$_PinCodeStateCopyWithImpl<_$_PinCodeState>(this, _$identity);
}

abstract class _PinCodeState extends PinCodeState {
  const factory _PinCodeState(
      {final bool isPinCodeNotValid, final String pinCode}) = _$_PinCodeState;
  const _PinCodeState._() : super._();

  @override
  bool get isPinCodeNotValid;
  @override
  String get pinCode;
  @override
  @JsonKey(ignore: true)
  _$$_PinCodeStateCopyWith<_$_PinCodeState> get copyWith =>
      throw _privateConstructorUsedError;
}
