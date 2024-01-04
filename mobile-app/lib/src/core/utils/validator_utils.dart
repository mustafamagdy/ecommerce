// ignore_for_file: constant_identifier_names


import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';

abstract class ValidatorUtils {
  static String? validateEmpty(String? input) {
    if (input == null || input.isEmpty) {
      return AppHelpers.getTranslation(TrKeys.fieldRequired);
    } else {
      return null;
    }
  }
}


