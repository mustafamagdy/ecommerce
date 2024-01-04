import 'package:admin_desktop/src/models/models.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:image_picker/image_picker.dart';

 part 'edit_profile_state.freezed.dart';

@freezed
class EditProfileState with _$EditProfileState {
  const factory EditProfileState({
   @Default(false) bool isLoading,
    @Default(false) bool checked,
    @Default(false) bool isSuccess,
    @Default("") String email,
    @Default("") String firstName,
    @Default("") String lastName,
    @Default("") String phone,
    @Default("") String secondPhone,
    @Default("") String birth,
    @Default("") String gender,
    @Default("") String url,
    @Default("") String imagePath,
    @Default(null) XFile? image,
    @Default(null) UserData? userData,
    @Default(0) int? selectIndex ,
    @Default(false) bool showPassword,
    @Default(false) bool showOldPassword,
    @Default(false) bool showPincode,
    @Default('') String password,
    @Default('') String confirmPassword,
  }) = _EditProfileState;

  const EditProfileState._();
}
