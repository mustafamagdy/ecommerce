import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:freezed_annotation/freezed_annotation.dart';



part 'delivered_orders_state.freezed.dart';

@freezed
class DeliveredOrdersState with _$DeliveredOrdersState {
  const factory DeliveredOrdersState({
    @Default(false) bool isLoading,
    @Default(true) bool hasMore,
    @Default([]) List<OrderData> orders,
    @Default(0) int totalCount,
    @Default('') String query,
  }) = _DeliveredOrdersState;

  const DeliveredOrdersState._();
}
