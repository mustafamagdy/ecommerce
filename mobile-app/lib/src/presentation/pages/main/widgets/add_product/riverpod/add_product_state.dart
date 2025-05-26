import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../../../../models/models.dart';

part 'add_product_state.freezed.dart';

@freezed
class AddProductState with _$AddProductState {
  const factory AddProductState({
    @Default(false) bool isLoading,
    @Default(false) bool isReviewing,
    @Default([]) List<TypedExtra> typedExtras,
    @Default([]) List<Stocks> initialStocks,
    @Default([]) List<int> selectedIndexes,
    @Default(0) int stockCount,
    ProductData? product,
    Stocks? selectedStock,
  }) = _AddProductState;

  const AddProductState._();
}