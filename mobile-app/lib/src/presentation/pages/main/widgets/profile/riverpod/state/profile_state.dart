import 'package:admin_desktop/src/models/models.dart';
import 'package:freezed_annotation/freezed_annotation.dart';




part 'profile_state.freezed.dart';

@freezed
class ProfileState with _$ProfileState {
  const factory ProfileState({
    @Default(true) bool isLoading,
    @Default(true) bool isReferralLoading,
    @Default(false) bool isSaveLoading,
    @Default(true) bool isLoadingHistory,
    @Default(0) int typeIndex,
    @Default("") String bgImage,
    @Default("") String logoImage,
    @Default(null) AddressData? addressModel,
    @Default(null) UserData? userData,
   
  }) = _ProfileState;

  const ProfileState._();
}
