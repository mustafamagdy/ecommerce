// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target

part of 'on_a_way_orders_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
    'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#custom-getters-and-methods');

/// @nodoc
mixin _$OnAWayOrdersState {
  bool get isLoading => throw _privateConstructorUsedError;
  bool get hasMore => throw _privateConstructorUsedError;
  List<OrderData> get orders => throw _privateConstructorUsedError;
  int get totalCount => throw _privateConstructorUsedError;
  String get query => throw _privateConstructorUsedError;

  @JsonKey(ignore: true)
  $OnAWayOrdersStateCopyWith<OnAWayOrdersState> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $OnAWayOrdersStateCopyWith<$Res> {
  factory $OnAWayOrdersStateCopyWith(
          OnAWayOrdersState value, $Res Function(OnAWayOrdersState) then) =
      _$OnAWayOrdersStateCopyWithImpl<$Res>;
  $Res call(
      {bool isLoading,
      bool hasMore,
      List<OrderData> orders,
      int totalCount,
      String query});
}

/// @nodoc
class _$OnAWayOrdersStateCopyWithImpl<$Res>
    implements $OnAWayOrdersStateCopyWith<$Res> {
  _$OnAWayOrdersStateCopyWithImpl(this._value, this._then);

  final OnAWayOrdersState _value;
  // ignore: unused_field
  final $Res Function(OnAWayOrdersState) _then;

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
abstract class _$$_OnAWayOrdersStateCopyWith<$Res>
    implements $OnAWayOrdersStateCopyWith<$Res> {
  factory _$$_OnAWayOrdersStateCopyWith(_$_OnAWayOrdersState value,
          $Res Function(_$_OnAWayOrdersState) then) =
      __$$_OnAWayOrdersStateCopyWithImpl<$Res>;
  @override
  $Res call(
      {bool isLoading,
      bool hasMore,
      List<OrderData> orders,
      int totalCount,
      String query});
}

/// @nodoc
class __$$_OnAWayOrdersStateCopyWithImpl<$Res>
    extends _$OnAWayOrdersStateCopyWithImpl<$Res>
    implements _$$_OnAWayOrdersStateCopyWith<$Res> {
  __$$_OnAWayOrdersStateCopyWithImpl(
      _$_OnAWayOrdersState _value, $Res Function(_$_OnAWayOrdersState) _then)
      : super(_value, (v) => _then(v as _$_OnAWayOrdersState));

  @override
  _$_OnAWayOrdersState get _value => super._value as _$_OnAWayOrdersState;

  @override
  $Res call({
    Object? isLoading = freezed,
    Object? hasMore = freezed,
    Object? orders = freezed,
    Object? totalCount = freezed,
    Object? query = freezed,
  }) {
    return _then(_$_OnAWayOrdersState(
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

class _$_OnAWayOrdersState extends _OnAWayOrdersState {
  const _$_OnAWayOrdersState(
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
    return 'OnAWayOrdersState(isLoading: $isLoading, hasMore: $hasMore, orders: $orders, totalCount: $totalCount, query: $query)';
  }

  @override
  bool operator ==(dynamic other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$_OnAWayOrdersState &&
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
  _$$_OnAWayOrdersStateCopyWith<_$_OnAWayOrdersState> get copyWith =>
      __$$_OnAWayOrdersStateCopyWithImpl<_$_OnAWayOrdersState>(
          this, _$identity);
}

abstract class _OnAWayOrdersState extends OnAWayOrdersState {
  const factory _OnAWayOrdersState(
      {final bool isLoading,
      final bool hasMore,
      final List<OrderData> orders,
      final int totalCount,
      final String query}) = _$_OnAWayOrdersState;
  const _OnAWayOrdersState._() : super._();

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
  _$$_OnAWayOrdersStateCopyWith<_$_OnAWayOrdersState> get copyWith =>
      throw _privateConstructorUsedError;
}
