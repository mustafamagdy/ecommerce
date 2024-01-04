
import 'package:admin_desktop/src/models/response/sale_cart_response.dart';
import 'package:admin_desktop/src/models/response/sale_history_response.dart';
import 'package:freezed_annotation/freezed_annotation.dart';




part 'sale_history_state.freezed.dart';

@freezed
class SaleHistoryState with _$SaleHistoryState {
  const factory SaleHistoryState({
    @Default(true) bool isLoading,
    @Default(false) bool isMoreLoading,
    @Default(2) int selectIndex,
    @Default(true) bool hasMore,
    @Default(null) SaleCartResponse? saleCart,
    @Default([]) List<SaleHistoryModel> listHistory,
    @Default([]) List<SaleHistoryModel> listDriver,
    @Default([]) List<SaleHistoryModel> listToday,
  }) = _SaleHistoryState;

  const SaleHistoryState._();
}
