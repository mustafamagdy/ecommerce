import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';

part 'order_table_state.freezed.dart';

@freezed
class OrderTableState with _$OrderTableState {
  const factory OrderTableState({
    @Default(false) bool isListView,
    @Default(0) int selectTabIndex,
    @Default(false) bool showFilter,
    @Default([]) List selectOrders,
    @Default(false) bool isAllSelect,
    @Default({}) Set<Marker> setOfMarker,
    // @Default('') String usersQuery,
    @Default(null) DateTime? start,
    @Default(null) DateTime? end,
  }) = _OrderTableState;

  const OrderTableState._();
}
