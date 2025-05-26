import 'package:admin_desktop/src/core/routes/app_router.gr.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../../core/utils/utils.dart';
import '../../../../../repository/repository.dart';
import '../state/splash_state.dart';

class SplashNotifier extends StateNotifier<SplashState> {
  final SettingsRepository _settingsRepository;

  SplashNotifier(this._settingsRepository) : super(const SplashState());

  Future<void> fetchGlobalSettings(
    BuildContext context, {
    VoidCallback? checkYourNetwork,
  }) async {
    if (LocalStorage.instance.getActiveLocale().isEmpty) {
      final connect = await AppConnectivity.connectivity();
      if (connect) {
        final response = await _settingsRepository.getGlobalSettings();
        response.when(
          success: (data) async {
            await LocalStorage.instance.setSettingsList(data.data ?? []);
            await LocalStorage.instance
                .setActiveLocale(AppHelpers.getInitialLocale());
            getTranslations(
              goLogin: () {
                context.replaceRoute(const LoginRoute());
              },
              goMain: () {
                bool checkPin = LocalStorage.instance.getPinCode().isEmpty;
                context.replaceRoute(PinCodeRoute(isNewPassword: checkPin));
              },
            );
          },
          failure: (failure) {
            debugPrint('==> error with settings fetched');
            getTranslations(
              goLogin: () {
                context.replaceRoute(const LoginRoute());
              },
              goMain: () {
                bool checkPin = LocalStorage.instance.getPinCode().isEmpty;
                context.replaceRoute(PinCodeRoute(isNewPassword: checkPin));
              },
            );
          },
        );
      } else {
        debugPrint('==> get active languages no connection');
        checkYourNetwork?.call();
      }
    } else {
      getTranslations(
        goLogin: () {
          context.replaceRoute(const LoginRoute());
        },
        goMain: () {
          context.replaceRoute(const MainRoute());
        },
      );
    }
  }

  Future<void> getTranslations({
    VoidCallback? goMain,
    VoidCallback? goLogin,
  }) async {
    final response = await _settingsRepository.getTranslations();
    response.when(
      success: (data) async {
        await LocalStorage.instance.setTranslations(data.data);
        if (LocalStorage.instance.getToken().isEmpty) {
          goLogin?.call();
        } else {
          goMain?.call();
        }
      },
      failure: (failure) {
        debugPrint('==> error with fetching translations $failure');
        if (LocalStorage.instance.getToken().isEmpty) {
          goLogin?.call();
        } else {
          goMain?.call();
        }
      },
    );
  }
}
