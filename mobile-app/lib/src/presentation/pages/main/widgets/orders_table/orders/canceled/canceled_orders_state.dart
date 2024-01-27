import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:freezed_annotation/freezed_annotation.dart';



part 'canceled_orders_state.freezed.dart';

@freezed
class CanceledOrdersState with _$CanceledOrdersState {
  const factory CanceledOrdersState({
    @Default(false) bool isLoading,
    @Default(true) bool hasMore,
    @Default([]) List<OrderData> orders,
    @Default(0) int totalCount,
    @Default('') String query,

  }) = _CanceledOrdersState;

  const CanceledOrdersState._();
}
