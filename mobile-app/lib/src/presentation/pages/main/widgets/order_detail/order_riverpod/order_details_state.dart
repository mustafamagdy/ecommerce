import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/models/models.dart';
import 'package:freezed_annotation/freezed_annotation.dart';



part 'order_details_state.freezed.dart';

@freezed
class OrderDetailsState with _$OrderDetailsState {
  const factory OrderDetailsState({
    @Default(false) bool isLoading,
    @Default("") String status,
    @Default('') String usersQuery,
    @Default(false) bool isUsersLoading,
    @Default([]) List<UserData> users,
    UserData? selectedUser,
    @Default(false) bool isUpdating,
    @Default([]) List<DropDownItemData> dropdownUsers,
    OrderData? order,
  }) = _OrderDetailsState;

  const OrderDetailsState._();
}
