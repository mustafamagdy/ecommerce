// ignore_for_file: unused_field

import 'package:admin_desktop/src/models/models.dart';
import 'package:admin_desktop/src/repository/repository.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../../../../core/utils/app_connectivity.dart';
import '../../../../../../../core/utils/app_helpers.dart';
import '../../../../../../../core/utils/local_storage.dart';
import '../../../../../../../repository/gallery.dart';
import '../state/profile_state.dart';

class ProfileNotifier extends StateNotifier<ProfileState> {
  final UsersRepository _userRepository;
  final ShopsRepository _shopsRepository;
  final GalleryRepositoryFacade _galleryRepository;

  ProfileNotifier(
      this._userRepository, this._galleryRepository, this._shopsRepository)
      : super(const ProfileState());
  int page = 1;

  resetShopData() {
    state = state.copyWith(
        bgImage: "", logoImage: "", addressModel: null, isSaveLoading: false);
  }

  setBgImage(String bgImage) {
    state = state.copyWith(bgImage: bgImage);
  }

  setLogoImage(String logoImage) {
    state = state.copyWith(logoImage: logoImage);
  }

  setAddress(dynamic data) {
    state = state.copyWith(addressModel: data);
  }
   void setUser(UserData user) async {
    state = state.copyWith(userData: user);
  }

  Future<void> fetchUser(BuildContext context) async {
    if (LocalStorage.instance.getToken().isNotEmpty) {
      final connected = await AppConnectivity.connectivity();
      if (connected) {
        state = state.copyWith(isLoading: true);

        final response = await _userRepository.getProfileDetails();
        response.when(
          success: (data) async {
            LocalStorage.instance.setUser(data.data);
            state = state.copyWith(isLoading: false, userData: data.data);
          },
          failure: (activeFailure) {
            state = state.copyWith(isLoading: false);

            AppHelpers.showSnackBar(
              context,
              AppHelpers.getTranslation(activeFailure.toString()),
            );
          },
        );
      } else {
        if (mounted) {
          // AppHelpers.showNoConnectionSnackBar(context);
        }
      }
    }
  }

  void logOut() {
    // LocalStorage.instance.logout();
  }

  // Future<void> deleteAccount(BuildContext context) async {
  //   final connected = await AppConnectivity.connectivity();
  //   if (connected) {
  //     state = state.copyWith(isLoading: true);
  //     final response = await _userRepository.deleteAccount();
  //     response.when(
  //       success: (data) async {
  //         context.router.popUntilRoot();
  //         context.replaceRoute(const LoginRoute());
  //       },
  //       failure: (activeFailure, status) {
  //         state = state.copyWith(isLoading: false);
  //         AppHelpers.showSnackBar(
  //           context,
  //           AppHelpers.getTranslation(status.toString()),
  //         );
  //       },
  //     );
  //   } else {
  //     if (mounted) {
  //       AppHelpers.showSnackBar(context, 'No internet connection');
  //     }
  //   }
  // }
}
