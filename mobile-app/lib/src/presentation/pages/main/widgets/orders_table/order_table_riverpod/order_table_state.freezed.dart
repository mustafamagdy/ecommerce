// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target

part of 'order_table_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
    'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#custom-getters-and-methods');

/// @nodoc
mixin _$OrderTableState {
  bool get isListView => throw _privateConstructorUsedError;
  int get selectTabIndex => throw _privateConstructorUsedError;
  bool get showFilter => throw _privateConstructorUsedError;
  List<dynamic> get selectOrders => throw _privateConstructorUsedError;
  bool get isAllSelect => throw _privateConstructorUsedError;
  Set<Marker> get setOfMarker =>
      throw _privateConstructorUsedError; // @Default('') String usersQuery,
  DateTime? get start => throw _privateConstructorUsedError;
  DateTime? get end => throw _privateConstructorUsedError;

  @JsonKey(ignore: true)
  $OrderTableStateCopyWith<OrderTableState> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $OrderTableStateCopyWith<$Res> {
  factory $OrderTableStateCopyWith(
          OrderTableState value, $Res Function(OrderTableState) then) =
      _$OrderTableStateCopyWithImpl<$Res>;
  $Res call(
      {bool isListView,
      int selectTabIndex,
      bool showFilter,
      List<dynamic> selectOrders,
      bool isAllSelect,
      Set<Marker> setOfMarker,
      DateTime? start,
      DateTime? end});
}

/// @nodoc
class _$OrderTableStateCopyWithImpl<$Res>
    implements $OrderTableStateCopyWith<$Res> {
  _$OrderTableStateCopyWithImpl(this._value, this._then);

  final OrderTableState _value;
  // ignore: unused_field
  final $Res Function(OrderTableState) _then;

  @override
  $Res call({
    Object? isListView = freezed,
    Object? selectTabIndex = freezed,
    Object? showFilter = freezed,
    Object? selectOrders = freezed,
    Object? isAllSelect = freezed,
    Object? setOfMarker = freezed,
    Object? start = freezed,
    Object? end = freezed,
  }) {
    return _then(_value.copyWith(
      isListView: isListView == freezed
          ? _value.isListView
          : isListView // ignore: cast_nullable_to_non_nullable
              as bool,
      selectTabIndex: selectTabIndex == freezed
          ? _value.selectTabIndex
          : selectTabIndex // ignore: cast_nullable_to_non_nullable
              as int,
      showFilter: showFilter == freezed
          ? _value.showFilter
          : showFilter // ignore: cast_nullable_to_non_nullable
              as bool,
      selectOrders: selectOrders == freezed
          ? _value.selectOrders
          : selectOrders // ignore: cast_nullable_to_non_nullable
              as List<dynamic>,
      isAllSelect: isAllSelect == freezed
          ? _value.isAllSelect
          : isAllSelect // ignore: cast_nullable_to_non_nullable
              as bool,
      setOfMarker: setOfMarker == freezed
          ? _value.setOfMarker
          : setOfMarker // ignore: cast_nullable_to_non_nullable
              as Set<Marker>,
      start: start == freezed
          ? _value.start
          : start // ignore: cast_nullable_to_non_nullable
              as DateTime?,
      end: end == freezed
          ? _value.end
          : end // ignore: cast_nullable_to_non_nullable
              as DateTime?,
    ));
  }
}

/// @nodoc
abstract class _$$_OrderTableStateCopyWith<$Res>
    implements $OrderTableStateCopyWith<$Res> {
  factory _$$_OrderTableStateCopyWith(
          _$_OrderTableState value, $Res Function(_$_OrderTableState) then) =
      __$$_OrderTableStateCopyWithImpl<$Res>;
  @override
  $Res call(
      {bool isListView,
      int selectTabIndex,
      bool showFilter,
      List<dynamic> selectOrders,
      bool isAllSelect,
      Set<Marker> setOfMarker,
      DateTime? start,
      DateTime? end});
}

/// @nodoc
class __$$_OrderTableStateCopyWithImpl<$Res>
    extends _$OrderTableStateCopyWithImpl<$Res>
    implements _$$_OrderTableStateCopyWith<$Res> {
  __$$_OrderTableStateCopyWithImpl(
      _$_OrderTableState _value, $Res Function(_$_OrderTableState) _then)
      : super(_value, (v) => _then(v as _$_OrderTableState));

  @override
  _$_OrderTableState get _value => super._value as _$_OrderTableState;

  @override
  $Res call({
    Object? isListView = freezed,
    Object? selectTabIndex = freezed,
    Object? showFilter = freezed,
    Object? selectOrders = freezed,
    Object? isAllSelect = freezed,
    Object? setOfMarker = freezed,
    Object? start = freezed,
    Object? end = freezed,
  }) {
    return _then(_$_OrderTableState(
      isListView: isListView == freezed
          ? _value.isListView
          : isListView // ignore: cast_nullable_to_non_nullable
              as bool,
      selectTabIndex: selectTabIndex == freezed
          ? _value.selectTabIndex
          : selectTabIndex // ignore: cast_nullable_to_non_nullable
              as int,
      showFilter: showFilter == freezed
          ? _value.showFilter
          : showFilter // ignore: cast_nullable_to_non_nullable
              as bool,
      selectOrders: selectOrders == freezed
          ? _value._selectOrders
          : selectOrders // ignore: cast_nullable_to_non_nullable
              as List<dynamic>,
      isAllSelect: isAllSelect == freezed
          ? _value.isAllSelect
          : isAllSelect // ignore: cast_nullable_to_non_nullable
              as bool,
      setOfMarker: setOfMarker == freezed
          ? _value._setOfMarker
          : setOfMarker // ignore: cast_nullable_to_non_nullable
              as Set<Marker>,
      start: start == freezed
          ? _value.start
          : start // ignore: cast_nullable_to_non_nullable
              as DateTime?,
      end: end == freezed
          ? _value.end
          : end // ignore: cast_nullable_to_non_nullable
              as DateTime?,
    ));
  }
}

/// @nodoc

class _$_OrderTableState extends _OrderTableState {
  const _$_OrderTableState(
      {this.isListView = false,
      this.selectTabIndex = 0,
      this.showFilter = false,
      final List<dynamic> selectOrders = const [],
      this.isAllSelect = false,
      final Set<Marker> setOfMarker = const {},
      this.start = null,
      this.end = null})
      : _selectOrders = selectOrders,
        _setOfMarker = setOfMarker,
        super._();

  @override
  @JsonKey()
  final bool isListView;
  @override
  @JsonKey()
  final int selectTabIndex;
  @override
  @JsonKey()
  final bool showFilter;
  final List<dynamic> _selectOrders;
  @override
  @JsonKey()
  List<dynamic> get selectOrders {
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_selectOrders);
  }

  @override
  @JsonKey()
  final bool isAllSelect;
  final Set<Marker> _setOfMarker;
  @override
  @JsonKey()
  Set<Marker> get setOfMarker {
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableSetView(_setOfMarker);
  }

// @Default('') String usersQuery,
  @override
  @JsonKey()
  final DateTime? start;
  @override
  @JsonKey()
  final DateTime? end;

  @override
  String toString() {
    return 'OrderTableState(isListView: $isListView, selectTabIndex: $selectTabIndex, showFilter: $showFilter, selectOrders: $selectOrders, isAllSelect: $isAllSelect, setOfMarker: $setOfMarker, start: $start, end: $end)';
  }

  @override
  bool operator ==(dynamic other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$_OrderTableState &&
            const DeepCollectionEquality()
                .equals(other.isListView, isListView) &&
            const DeepCollectionEquality()
                .equals(other.selectTabIndex, selectTabIndex) &&
            const DeepCollectionEquality()
                .equals(other.showFilter, showFilter) &&
            const DeepCollectionEquality()
                .equals(other._selectOrders, _selectOrders) &&
            const DeepCollectionEquality()
                .equals(other.isAllSelect, isAllSelect) &&
            const DeepCollectionEquality()
                .equals(other._setOfMarker, _setOfMarker) &&
            const DeepCollectionEquality().equals(other.start, start) &&
            const DeepCollectionEquality().equals(other.end, end));
  }

  @override
  int get hashCode => Object.hash(
      runtimeType,
      const DeepCollectionEquality().hash(isListView),
      const DeepCollectionEquality().hash(selectTabIndex),
      const DeepCollectionEquality().hash(showFilter),
      const DeepCollectionEquality().hash(_selectOrders),
      const DeepCollectionEquality().hash(isAllSelect),
      const DeepCollectionEquality().hash(_setOfMarker),
      const DeepCollectionEquality().hash(start),
      const DeepCollectionEquality().hash(end));

  @JsonKey(ignore: true)
  @override
  _$$_OrderTableStateCopyWith<_$_OrderTableState> get copyWith =>
      __$$_OrderTableStateCopyWithImpl<_$_OrderTableState>(this, _$identity);
}

abstract class _OrderTableState extends OrderTableState {
  const factory _OrderTableState(
      {final bool isListView,
      final int selectTabIndex,
      final bool showFilter,
      final List<dynamic> selectOrders,
      final bool isAllSelect,
      final Set<Marker> setOfMarker,
      final DateTime? start,
      final DateTime? end}) = _$_OrderTableState;
  const _OrderTableState._() : super._();

  @override
  bool get isListView;
  @override
  int get selectTabIndex;
  @override
  bool get showFilter;
  @override
  List<dynamic> get selectOrders;
  @override
  bool get isAllSelect;
  @override
  Set<Marker> get setOfMarker;
  @override // @Default('') String usersQuery,
  DateTime? get start;
  @override
  DateTime? get end;
  @override
  @JsonKey(ignore: true)
  _$$_OrderTableStateCopyWith<_$_OrderTableState> get copyWith =>
      throw _privateConstructorUsedError;
}
