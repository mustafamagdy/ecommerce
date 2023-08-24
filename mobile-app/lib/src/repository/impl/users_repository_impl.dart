import 'dart:convert';

import 'package:admin_desktop/src/models/data/customer_model.dart';
import 'package:admin_desktop/src/models/response/edit_profile.dart';
import 'package:admin_desktop/src/models/response/profile_response.dart';
import 'package:flutter/material.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';

import '../../core/di/injection.dart';
import '../../core/handlers/handlers.dart';
import '../../core/utils/utils.dart';
import '../../models/models.dart';
import '../repository.dart';

class UsersRepositoryImpl extends UsersRepository {
  @override
  Future<ApiResult<UsersPaginateResponse>> searchUsers(String? query) async {
    final data = {
      if (query != null) 'search': query,
      'lang': LocalStorage.instance.getActiveLocale(),
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/users/paginate',
        queryParameters: data,
      );
      return ApiResult.success(
        data: UsersPaginateResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> search users failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<UsersPaginateResponse>> searchDeliveryman(
      String? query) async {
    final data = {
      if (query != null) 'search': query,
      'lang': LocalStorage.instance.getActiveLocale(),
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/shop/users/role/deliveryman',
        queryParameters: data,
      );
      return ApiResult.success(
        data: UsersPaginateResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> search users failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<SingleUserResponse>> getUserDetails(String uuid) async {
    final data = {'lang': LocalStorage.instance.getActiveLocale()};
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/users/$uuid',
        queryParameters: data,
      );
      return ApiResult.success(
        data: SingleUserResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get user details failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<ProfileResponse>> getProfileDetails() async {
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/user/profile/show',
      );
      return ApiResult.success(
        data: ProfileResponse.fromJson(response.data),
      );
    } catch (e) {
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<bool>> checkDriverZone(LatLng location, int? shopId) async {
    try {
      final client = inject<HttpService>().client(requireAuth: false);
      final data = <String, dynamic>{
        'address[latitude]': location.latitude,
        'address[longitude]': location.longitude,
      };

      final response = await client.get(
          '/api/v1/rest/shop/$shopId/delivery-zone/check/distance',
          queryParameters: data);

      return ApiResult.success(
        data: response.data["status"],
      );
    } catch (e) {
      debugPrint('==> get delivery zone failure: $e');
      return ApiResult.failure(
        error: NetworkExceptions.getDioException(e),
      );
    }
  }

  @override
  Future<ApiResult> checkCoupon({
    required String coupon,
    required int shopId,
  }) async {
    final data = {
      'coupon': coupon,
      'shop_id': shopId,
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      await client.post(
        '/api/v1/rest/coupons/check',
        data: data,
      );
      return const ApiResult.success(data: true);
    } catch (e) {
      debugPrint('==> check coupon failure: $e');
      return ApiResult.failure(
        error: NetworkExceptions.getDioException(e),
      );
    }
  }

  @override
  Future<ApiResult<ProfileResponse>> updatePassword(
      {required String password, required String passwordConfirmation}) async {
    final data = {
      'password': password,
      'password_confirmation': passwordConfirmation,
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.post(
        '/api/v1/dashboard/user/profile/password/update',
        data: data,
      );
      return ApiResult.success(
        data: ProfileResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> update password failure: $e');
      return ApiResult.failure(
        error: NetworkExceptions.getDioException(e),
      );
    }
  }

  @override
  Future<ApiResult<ProfileResponse>> updateProfileImage(
      {required String firstName, required String imageUrl}) async {
    final data = {
      'firstname': firstName,
      'images': [imageUrl],
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.put(
        '/api/v1/dashboard/user/profile/update',
        data: data,
      );
      return ApiResult.success(
        data: ProfileResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> update profile image failure: $e');
      return ApiResult.failure(
        error: NetworkExceptions.getDioException(e),
      );
    }
  }

  @override
  Future<ApiResult<ProfileResponse>> editProfile(
      {required EditProfile? user}) async {
    final data = user?.toJson();
    debugPrint('===> update general info data ${jsonEncode(data)}');
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.put(
        '/api/v1/dashboard/user/profile/update',
        data: data,
      );
      return ApiResult.success(
        data: ProfileResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> update profile details failure: $e');
      return ApiResult.failure(
        error: NetworkExceptions.getDioException(e),
      );
    }
  }

  @override
  Future<ApiResult<CustomerModel>> createUser(
      {required CustomerModel query}) async {
    final data = query.toJson();
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.post(
        '/api/v1/dashboard/seller/users',
        data: data,
      );
      return ApiResult.success(
        data: CustomerModel.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> create user failure: $e');
      return ApiResult.failure(
        error: NetworkExceptions.getDioException(e),
      );
    }
  }

  @override
  Future<ApiResult<UsersPaginateResponse>> getUsers({int? page}) async {
    final data = {
      'perPage': 6,
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/users/paginate',
        queryParameters: data
      );
      return ApiResult.success(
        data: UsersPaginateResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get users failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }
}
