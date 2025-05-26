import 'package:admin_desktop/src/models/data/customer_model.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';
import '../core/handlers/handlers.dart';
import '../models/models.dart';
import '../models/response/edit_profile.dart';
import '../models/response/profile_response.dart';

abstract class UsersRepository {
  Future<ApiResult<UsersPaginateResponse>> searchUsers(String? query);

  Future<ApiResult<UsersPaginateResponse>> searchDeliveryman(String? query);

  Future<ApiResult<SingleUserResponse>> getUserDetails(String uuid);

  Future<ApiResult<ProfileResponse>> getProfileDetails();

  Future<ApiResult<bool>> checkDriverZone(LatLng location, int? shopId);

  Future<ApiResult> checkCoupon({
    required String coupon,
    required int shopId,
  });

  Future<ApiResult<ProfileResponse>> editProfile({required EditProfile? user});

  Future<ApiResult<ProfileResponse>> updateProfileImage({
    required String firstName,
    required String imageUrl,
  });

  Future<ApiResult<ProfileResponse>> updatePassword({
    required String password,
    required String passwordConfirmation,
  });

  Future<ApiResult<UsersPaginateResponse>> getUsers({int? page});

  Future<ApiResult<CustomerModel>> createUser({required CustomerModel query});
}
