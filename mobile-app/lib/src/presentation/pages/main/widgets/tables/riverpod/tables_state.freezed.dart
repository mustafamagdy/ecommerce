// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target

part of 'tables_state.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
    'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#custom-getters-and-methods');

/// @nodoc
mixin _$TablesState {
  bool get isLoading => throw _privateConstructorUsedError;
  bool get hasMore => throw _privateConstructorUsedError;
  int get selectTabIndex => throw _privateConstructorUsedError;
  int get selectFloor => throw _privateConstructorUsedError;
  List<String> get listFloor => throw _privateConstructorUsedError;
  List<TableModel?> get tableList => throw _privateConstructorUsedError;
  String get addFloor => throw _privateConstructorUsedError;

  @JsonKey(ignore: true)
  $TablesStateCopyWith<TablesState> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $TablesStateCopyWith<$Res> {
  factory $TablesStateCopyWith(
          TablesState value, $Res Function(TablesState) then) =
      _$TablesStateCopyWithImpl<$Res>;
  $Res call(
      {bool isLoading,
      bool hasMore,
      int selectTabIndex,
      int selectFloor,
      List<String> listFloor,
      List<TableModel?> tableList,
      String addFloor});
}

/// @nodoc
class _$TablesStateCopyWithImpl<$Res> implements $TablesStateCopyWith<$Res> {
  _$TablesStateCopyWithImpl(this._value, this._then);

  final TablesState _value;
  // ignore: unused_field
  final $Res Function(TablesState) _then;

  @override
  $Res call({
    Object? isLoading = freezed,
    Object? hasMore = freezed,
    Object? selectTabIndex = freezed,
    Object? selectFloor = freezed,
    Object? listFloor = freezed,
    Object? tableList = freezed,
    Object? addFloor = freezed,
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
      selectTabIndex: selectTabIndex == freezed
          ? _value.selectTabIndex
          : selectTabIndex // ignore: cast_nullable_to_non_nullable
              as int,
      selectFloor: selectFloor == freezed
          ? _value.selectFloor
          : selectFloor // ignore: cast_nullable_to_non_nullable
              as int,
      listFloor: listFloor == freezed
          ? _value.listFloor
          : listFloor // ignore: cast_nullable_to_non_nullable
              as List<String>,
      tableList: tableList == freezed
          ? _value.tableList
          : tableList // ignore: cast_nullable_to_non_nullable
              as List<TableModel?>,
      addFloor: addFloor == freezed
          ? _value.addFloor
          : addFloor // ignore: cast_nullable_to_non_nullable
              as String,
    ));
  }
}

/// @nodoc
abstract class _$$_TablesStateCopyWith<$Res>
    implements $TablesStateCopyWith<$Res> {
  factory _$$_TablesStateCopyWith(
          _$_TablesState value, $Res Function(_$_TablesState) then) =
      __$$_TablesStateCopyWithImpl<$Res>;
  @override
  $Res call(
      {bool isLoading,
      bool hasMore,
      int selectTabIndex,
      int selectFloor,
      List<String> listFloor,
      List<TableModel?> tableList,
      String addFloor});
}

/// @nodoc
class __$$_TablesStateCopyWithImpl<$Res> extends _$TablesStateCopyWithImpl<$Res>
    implements _$$_TablesStateCopyWith<$Res> {
  __$$_TablesStateCopyWithImpl(
      _$_TablesState _value, $Res Function(_$_TablesState) _then)
      : super(_value, (v) => _then(v as _$_TablesState));

  @override
  _$_TablesState get _value => super._value as _$_TablesState;

  @override
  $Res call({
    Object? isLoading = freezed,
    Object? hasMore = freezed,
    Object? selectTabIndex = freezed,
    Object? selectFloor = freezed,
    Object? listFloor = freezed,
    Object? tableList = freezed,
    Object? addFloor = freezed,
  }) {
    return _then(_$_TablesState(
      isLoading: isLoading == freezed
          ? _value.isLoading
          : isLoading // ignore: cast_nullable_to_non_nullable
              as bool,
      hasMore: hasMore == freezed
          ? _value.hasMore
          : hasMore // ignore: cast_nullable_to_non_nullable
              as bool,
      selectTabIndex: selectTabIndex == freezed
          ? _value.selectTabIndex
          : selectTabIndex // ignore: cast_nullable_to_non_nullable
              as int,
      selectFloor: selectFloor == freezed
          ? _value.selectFloor
          : selectFloor // ignore: cast_nullable_to_non_nullable
              as int,
      listFloor: listFloor == freezed
          ? _value._listFloor
          : listFloor // ignore: cast_nullable_to_non_nullable
              as List<String>,
      tableList: tableList == freezed
          ? _value._tableList
          : tableList // ignore: cast_nullable_to_non_nullable
              as List<TableModel?>,
      addFloor: addFloor == freezed
          ? _value.addFloor
          : addFloor // ignore: cast_nullable_to_non_nullable
              as String,
    ));
  }
}

/// @nodoc

class _$_TablesState extends _TablesState {
  const _$_TablesState(
      {this.isLoading = false,
      this.hasMore = true,
      this.selectTabIndex = 0,
      this.selectFloor = 0,
      final List<String> listFloor = const [TrKeys.oneFloor, TrKeys.twoFloor],
      final List<TableModel?> tableList = const [],
      this.addFloor = ""})
      : _listFloor = listFloor,
        _tableList = tableList,
        super._();

  @override
  @JsonKey()
  final bool isLoading;
  @override
  @JsonKey()
  final bool hasMore;
  @override
  @JsonKey()
  final int selectTabIndex;
  @override
  @JsonKey()
  final int selectFloor;
  final List<String> _listFloor;
  @override
  @JsonKey()
  List<String> get listFloor {
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_listFloor);
  }

  final List<TableModel?> _tableList;
  @override
  @JsonKey()
  List<TableModel?> get tableList {
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_tableList);
  }

  @override
  @JsonKey()
  final String addFloor;

  @override
  String toString() {
    return 'TablesState(isLoading: $isLoading, hasMore: $hasMore, selectTabIndex: $selectTabIndex, selectFloor: $selectFloor, listFloor: $listFloor, tableList: $tableList, addFloor: $addFloor)';
  }

  @override
  bool operator ==(dynamic other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$_TablesState &&
            const DeepCollectionEquality().equals(other.isLoading, isLoading) &&
            const DeepCollectionEquality().equals(other.hasMore, hasMore) &&
            const DeepCollectionEquality()
                .equals(other.selectTabIndex, selectTabIndex) &&
            const DeepCollectionEquality()
                .equals(other.selectFloor, selectFloor) &&
            const DeepCollectionEquality()
                .equals(other._listFloor, _listFloor) &&
            const DeepCollectionEquality()
                .equals(other._tableList, _tableList) &&
            const DeepCollectionEquality().equals(other.addFloor, addFloor));
  }

  @override
  int get hashCode => Object.hash(
      runtimeType,
      const DeepCollectionEquality().hash(isLoading),
      const DeepCollectionEquality().hash(hasMore),
      const DeepCollectionEquality().hash(selectTabIndex),
      const DeepCollectionEquality().hash(selectFloor),
      const DeepCollectionEquality().hash(_listFloor),
      const DeepCollectionEquality().hash(_tableList),
      const DeepCollectionEquality().hash(addFloor));

  @JsonKey(ignore: true)
  @override
  _$$_TablesStateCopyWith<_$_TablesState> get copyWith =>
      __$$_TablesStateCopyWithImpl<_$_TablesState>(this, _$identity);
}

abstract class _TablesState extends TablesState {
  const factory _TablesState(
      {final bool isLoading,
      final bool hasMore,
      final int selectTabIndex,
      final int selectFloor,
      final List<String> listFloor,
      final List<TableModel?> tableList,
      final String addFloor}) = _$_TablesState;
  const _TablesState._() : super._();

  @override
  bool get isLoading;
  @override
  bool get hasMore;
  @override
  int get selectTabIndex;
  @override
  int get selectFloor;
  @override
  List<String> get listFloor;
  @override
  List<TableModel?> get tableList;
  @override
  String get addFloor;
  @override
  @JsonKey(ignore: true)
  _$$_TablesStateCopyWith<_$_TablesState> get copyWith =>
      throw _privateConstructorUsedError;
}
