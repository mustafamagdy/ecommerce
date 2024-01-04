
import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:freezed_annotation/freezed_annotation.dart';


part 'kitchen_state.freezed.dart';

@freezed
class KitchenState with _$KitchenState {
  const factory KitchenState({
    @Default(true) bool isLoading,
    @Default(TrKeys.all) String selectType,
    @Default(true) bool hasMore,
    @Default([]) List<OrderData> orders,
    @Default(null) OrderData? selectOrder,
    @Default('') String query,
    @Default(0) int selectIndex,
  }) = _KitchenState;

  const KitchenState._();
}
