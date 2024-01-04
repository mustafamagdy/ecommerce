import 'package:admin_desktop/src/models/data/customer_model.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import '../../../../../../../models/models.dart';

part 'customer_state.freezed.dart';

@freezed
class CustomerState with _$CustomerState {
  const factory CustomerState({
    @Default(false) bool isLoading,
    @Default(false) bool isMoreLoading,
    @Default(false) bool createUserLoading,
    @Default(true) bool hasMore,
    @Default('') String query,
    @Default([]) List<UserData> users,
    @Default(null) UserData? selectUser,
    @Default(0) int totalCount,
    @Default(null) CustomerModel? user,
  }) = _CustomerState;

  const CustomerState._();
}
