import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:freezed_annotation/freezed_annotation.dart';



part 'cooking_orders_state.freezed.dart';

@freezed
class CookingOrdersState with _$CookingOrdersState {
  const factory CookingOrdersState({
    @Default(false) bool isLoading,
    @Default(true) bool hasMore,
    @Default([]) List<OrderData> orders,
    @Default(0) int totalCount,
    @Default('') String query,
  }) = _CookingOrdersState;

  const CookingOrdersState._();
}
