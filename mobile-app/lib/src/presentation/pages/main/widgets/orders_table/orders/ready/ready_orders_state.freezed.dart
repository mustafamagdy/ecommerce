// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target

part of 'ready_orders_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
    'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#custom-getters-and-methods');

/// @nodoc
mixin _$ReadyOrdersState {
  bool get isLoading => throw _privateConstructorUsedError;
  bool get hasMore => throw _privateConstructorUsedError;
  List<OrderData> get orders => throw _privateConstructorUsedError;
  int get totalCount => throw _privateConstructorUsedError;
  String get query => throw _privateConstructorUsedError;

  @JsonKey(ignore: true)
  $ReadyOrdersStateCopyWith<ReadyOrdersState> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ReadyOrdersStateCopyWith<$Res> {
  factory $ReadyOrdersStateCopyWith(
          ReadyOrdersState value, $Res Function(ReadyOrdersState) then) =
      _$ReadyOrdersStateCopyWithImpl<$Res>;
  $Res call(
      {bool isLoading,
      bool hasMore,
      List<OrderData> orders,
      int totalCount,
      String query});
}

/// @nodoc
class _$ReadyOrdersStateCopyWithImpl<$Res>
    implements $ReadyOrdersStateCopyWith<$Res> {
  _$ReadyOrdersStateCopyWithImpl(this._value, this._then);

  final ReadyOrdersState _value;
  // ignore: unused_field
  final $Res Function(ReadyOrdersState) _then;

  @override
  $Res call({
    Object? isLoading = freezed,
    Object? hasMore = freezed,
    Object? orders = freezed,
    Object? totalCount = freezed,
    Object? query = freezed,
  }) {
    return _then(_value.copyWith(
      isLoading: isLoading == freezed
          ? _value.isLoading
          : isLoading // ignore: cast_nullable_to_non_nullable
              as bool,
      hasMore: hasMore == freezed
          ? _value.hasMore
          : hasMore // ignore: cast_nullable_to_non_nullable
              as bool,
      orders: orders == freezed
          ? _value.orders
          : orders // ignore: cast_nullable_to_non_nullable
              as List<OrderData>,
      totalCount: totalCount == freezed
          ? _value.totalCount
          : totalCount // ignore: cast_nullable_to_non_nullable
              as int,
      query: query == freezed
          ? _value.query
          : query // ignore: cast_nullable_to_non_nullable
              as String,
    ));
  }
}

/// @nodoc
abstract class _$$_ReadyOrdersStateCopyWith<$Res>
    implements $ReadyOrdersStateCopyWith<$Res> {
  factory _$$_ReadyOrdersStateCopyWith(
          _$_ReadyOrdersState value, $Res Function(_$_ReadyOrdersState) then) =
      __$$_ReadyOrdersStateCopyWithImpl<$Res>;
  @override
  $Res call(
      {bool isLoading,
      bool hasMore,
      List<OrderData> orders,
      int totalCount,
      String query});
}

/// @nodoc
class __$$_ReadyOrdersStateCopyWithImpl<$Res>
    extends _$ReadyOrdersStateCopyWithImpl<$Res>
    implements _$$_ReadyOrdersStateCopyWith<$Res> {
  __$$_ReadyOrdersStateCopyWithImpl(
      _$_ReadyOrdersState _value, $Res Function(_$_ReadyOrdersState) _then)
      : super(_value, (v) => _then(v as _$_ReadyOrdersState));

  @override
  _$_ReadyOrdersState get _value => super._value as _$_ReadyOrdersState;

  @override
  $Res call({
    Object? isLoading = freezed,
    Object? hasMore = freezed,
    Object? orders = freezed,
    Object? totalCount = freezed,
    Object? query = freezed,
  }) {
    return _then(_$_ReadyOrdersState(
      isLoading: isLoading == freezed
          ? _value.isLoading
          : isLoading // ignore: cast_nullable_to_non_nullable
              as bool,
      hasMore: hasMore == freezed
          ? _value.hasMore
          : hasMore // ignore: cast_nullable_to_non_nullable
              as bool,
      orders: orders == freezed
          ? _value._orders
          : orders // ignore: cast_nullable_to_non_nullable
              as List<OrderData>,
      totalCount: totalCount == freezed
          ? _value.totalCount
          : totalCount // ignore: cast_nullable_to_non_nullable
              as int,
      query: query == freezed
          ? _value.query
          : query // ignore: cast_nullable_to_non_nullable
              as String,
    ));
  }
}

/// @nodoc

class _$_ReadyOrdersState extends _ReadyOrdersState {
  const _$_ReadyOrdersState(
      {this.isLoading = false,
      this.hasMore = true,
      final List<OrderData> orders = const [],
      this.totalCount = 0,
      this.query = ''})
      : _orders = orders,
        super._();

  @override
  @JsonKey()
  final bool isLoading;
  @override
  @JsonKey()
  final bool hasMore;
  final List<OrderData> _orders;
  @override
  @JsonKey()
  List<OrderData> get orders {
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_orders);
  }

  @override
  @JsonKey()
  final int totalCount;
  @override
  @JsonKey()
  final String query;

  @override
  String toString() {
    return 'ReadyOrdersState(isLoading: $isLoading, hasMore: $hasMore, orders: $orders, totalCount: $totalCount, query: $query)';
  }

  @override
  bool operator ==(dynamic other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$_ReadyOrdersState &&
            const DeepCollectionEquality().equals(other.isLoading, isLoading) &&
            const DeepCollectionEquality().equals(other.hasMore, hasMore) &&
            const DeepCollectionEquality().equals(other._orders, _orders) &&
            const DeepCollectionEquality()
                .equals(other.totalCount, totalCount) &&
            const DeepCollectionEquality().equals(other.query, query));
  }

  @override
  int get hashCode => Object.hash(
      runtimeType,
      const DeepCollectionEquality().hash(isLoading),
      const DeepCollectionEquality().hash(hasMore),
      const DeepCollectionEquality().hash(_orders),
      const DeepCollectionEquality().hash(totalCount),
      const DeepCollectionEquality().hash(query));

  @JsonKey(ignore: true)
  @override
  _$$_ReadyOrdersStateCopyWith<_$_ReadyOrdersState> get copyWith =>
      __$$_ReadyOrdersStateCopyWithImpl<_$_ReadyOrdersState>(this, _$identity);
}

abstract class _ReadyOrdersState extends ReadyOrdersState {
  const factory _ReadyOrdersState(
      {final bool isLoading,
      final bool hasMore,
      final List<OrderData> orders,
      final int totalCount,
      final String query}) = _$_ReadyOrdersState;
  const _ReadyOrdersState._() : super._();

  @override
  bool get isLoading;
  @override
  bool get hasMore;
  @override
  List<OrderData> get orders;
  @override
  int get totalCount;
  @override
  String get query;
  @override
  @JsonKey(ignore: true)
  _$$_ReadyOrdersStateCopyWith<_$_ReadyOrdersState> get copyWith =>
      throw _privateConstructorUsedError;
}
