
import 'package:admin_desktop/src/models/response/income_chart_response.dart';
import 'package:flash/flash.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../models/models.dart';
import '../../presentation/theme/theme.dart';
import '../constants/constants.dart';
import 'local_storage.dart';

class AppHelpers {
  AppHelpers._();

  static String? getAppName() {
    final List<SettingsData> settings = LocalStorage.instance.getSettingsList();
    for (final setting in settings) {
      if (setting.key == 'title') {
        return setting.value;
      }
    }
    return '';
  }

  static String? getInitialLocale() {
    final List<SettingsData> settings = LocalStorage.instance.getSettingsList();
    for (final setting in settings) {
      if (setting.key == 'lang') {
        return setting.value;
      }
    }
    return null;
  }

  static double? getInitialLatitude() {
    final List<SettingsData> settings = LocalStorage.instance.getSettingsList();
    for (final setting in settings) {
      if (setting.key == 'location') {
        final String? latString =
            setting.value?.substring(0, setting.value?.indexOf(','));
        if (latString == null) {
          return null;
        }
        final double? lat = double.tryParse(latString);
        return lat;
      }
    }
    return null;
  }

  static double? getInitialLongitude() {
    final List<SettingsData> settings = LocalStorage.instance.getSettingsList();
    for (final setting in settings) {
      if (setting.key == 'location') {
        final String? latString =
            setting.value?.substring(0, setting.value?.indexOf(','));
        if (latString == null) {
          return null;
        }
        final String? lonString = setting.value
            ?.substring((latString.length) + 2, setting.value?.length);
        if (lonString == null) {
          return null;
        }
        final double lon = double.parse(lonString);
        return lon;
      }
    }
    return null;
  }

  static void showAlertDialog({
    required BuildContext context,
    required Widget child,
    double radius = 16,
  }) {
    AlertDialog alert = AlertDialog(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.all(
          Radius.circular(radius.r),
        ),
      ),
      contentPadding: EdgeInsets.all(20.r),
      iconPadding: EdgeInsets.zero,
      content: child,
    );

    showDialog(
      context: context,
      builder: (BuildContext context) {
        return alert;
      },
    );
  }

  static showSnackBar(BuildContext context, String title,
      {bool isIcon = false}) {
    ScaffoldMessenger.of(context).clearSnackBars();
    final snackBar = SnackBar(
      backgroundColor: AppColors.white,
      behavior: SnackBarBehavior.floating,
      duration: const Duration(seconds: 3),
      margin: EdgeInsets.fromLTRB(MediaQuery.of(context).size.width - 400.w, 0,
          32, MediaQuery.of(context).size.height - 160.h),
      content: Row(
        children: [
          if (isIcon)
            Padding(
              padding: EdgeInsets.only(right: 8.r),
              child: const Icon(
                FlutterRemix.checkbox_circle_fill,
                color: AppColors.brandColor,
              ),
            ),
          Text(
            title,
            style: GoogleFonts.inter(
              fontSize: 14,
              color: AppColors.black,
            ),
          ),
        ],
      ),
      action: SnackBarAction(
        label: AppHelpers.getTranslation(TrKeys.close),
        disabledTextColor: AppColors.black,
        textColor: AppColors.black,
        onPressed: () {
          ScaffoldMessenger.of(context).hideCurrentSnackBar();
        },
      ),
    );
    ScaffoldMessenger.of(context).showSnackBar(snackBar);
  }

  static showFlashBar(BuildContext context, String text) {
    return showFlash(
      context: context,
      duration: const Duration(seconds: 3),
      builder: (BuildContext context, FlashController controller) {
        return Flash(
          controller: controller,
          position: FlashPosition.top,
          // backgroundColor: AppColors.white,
          // borderRadius: BorderRadius.circular(8.r),
          // behavior: FlashBehavior.floating,
          // margin: REdgeInsets.only(top: 50),
          // brightness: Brightness.light,
          // barrierBlur: 1.5.r,
          // barrierColor: Colors.black38,
          // barrierDismissible: true,
          child: Container(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(8.r),
              border: Border.all(
                color: AppColors.dontHaveAccBtnBack,
                width: 2.r,
              ),
            ),
            child: Padding(
              padding: REdgeInsets.all(15),
              child: Text(
                text,
                style: GoogleFonts.inter(
                  color: AppColors.black,
                  fontSize: 18.sp,
                  fontWeight: FontWeight.w500,
                  letterSpacing: -0.4,
                ),
                textAlign: TextAlign.center,
              ),
            ),
          ),
        );
      },
    );
  }

  static String getTranslation(String trKey) {
    final Map<String, dynamic> translations =
        LocalStorage.instance.getTranslations();
    for (final key in translations.keys) {
      if (trKey == key) {
        return translations[key];
      }
    }
    return trKey;
  }

  static ExtrasType getExtraTypeByValue(String? value) {
    switch (value) {
      case 'color':
        return ExtrasType.color;
      case 'text':
        return ExtrasType.text;
      case 'image':
        return ExtrasType.image;
      default:
        return ExtrasType.text;
    }
  }

  static DateTime getMinTime(String openTime) {
    final int openHour = int.parse(openTime.substring(3, 5)) == 0
        ? int.parse(openTime.substring(0, 2))
        : int.parse(openTime.substring(0, 2)) + 1;
    final DateTime now = DateTime.now();
    return DateTime(now.year, now.month, now.day, openHour);
  }

  static DateTime getMaxTime(String closeTime) {
    final int closeHour = int.parse(closeTime.substring(0, 2));
    final DateTime now = DateTime.now();
    return DateTime(now.year, now.month, now.day, closeHour);
  }

  static String getOrderStatusText(OrderStatus value) {
    switch (value) {
      case OrderStatus.newOrder:
        return "new";
      case OrderStatus.accepted:
        return "accepted";
      case OrderStatus.cooking:
        return "cooking";
      case OrderStatus.ready:
        return "ready";
      case OrderStatus.onAWay:
        return "on_a_way";
      case OrderStatus.delivered:
        return "delivered";
      default:
        return "canceled";
    }
  }

  static OrderStatus getOrderStatus(String? value, {bool? isNextStatus}) {
    if (isNextStatus ?? false) {
      switch (value) {
        case 'new':
          return OrderStatus.accepted;
        case 'accepted':
          return OrderStatus.cooking;
        case 'cooking':
          return OrderStatus.ready;
        case 'ready':
          return OrderStatus.onAWay;
        case 'on_a_way':
          return OrderStatus.delivered;
        default:
          return OrderStatus.canceled;
      }
    } else {
      switch (value) {
        case 'new':
          return OrderStatus.newOrder;
        case 'accepted':
          return OrderStatus.accepted;
        case 'cooking':
          return OrderStatus.cooking;
        case 'ready':
          return OrderStatus.ready;
        case 'on_a_way':
          return OrderStatus.onAWay;
        case 'delivered':
          return OrderStatus.delivered;
        default:
          return OrderStatus.canceled;
      }
    }
  }

  static String getPinCodeText(int index) {
    switch (index) {
      case 0:
        return "1";
      case 1:
        return "2";
      case 2:
        return "3";
      case 3:
        return "4";
      case 4:
        return "5";
      case 5:
        return "6";
      case 6:
        return "7";
      case 7:
        return "8";
      case 8:
        return "9";
      case 10:
        return "0";
      default:
        return "0";
    }
  }

  static Widget getStatusType(String text) {
    return Container(
      padding: EdgeInsets.symmetric(vertical: 2.r, horizontal: 10.r),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(100.r),
        color: text == "new"
            ? AppColors.blue
            : text == "accept"
                ? Colors.deepPurple
                : text == "ready"
                    ? AppColors.rate
                    : AppColors.brandColor,
      ),
      child: Text(
        getTranslation(text),
        style: GoogleFonts.inter(
          fontSize: 12.sp,
          color: AppColors.white,
          fontWeight: FontWeight.w500,
        ),
      ),
    );
  }


  static  PositionModel fixedTable(int n){
    int top=0;
    int left=0;
    int right=0;
    int bottom=0;
    if(n==1){
      top=1;
    }else if(n==2){
      top=1;
      bottom=1;

    }else if(n==3){
      top=1;
      bottom=1;
      left=1;
    }
    else if(n==4){
      top=1;
      bottom=1;
      left=1;
      right=1;

    }
    else if(n>4 && n<=10){
      top=((n-2)/2).ceil();
      bottom=((n-2)/2).floor();
      left=1;
      right=1;
    }else if(n>10){
      top=((n-4)/2).ceil();
      bottom=((n-4)/2).floor();
      left=2;
      right=2;
    }

    return PositionModel(top: top, left: left, right: right, bottom: bottom);

  }

}

extension Time on DateTime {
  bool toEqualTime(DateTime time) {
    if (time.year != year) {
      return false;
    } else if (time.month != month) {
      return false;
    } else if (time.day != day) {
      return false;
    }
    return true;
  }

  bool toEqualTimeWithHour(DateTime time) {
    if (time.year != year) {
      return false;
    } else if (time.month != month) {
      return false;
    } else if (time.day != day) {
      return false;
    }else if (time.hour != hour) {
      return false;
    }
    return true;
  }
}

extension FindPriceIndex on List<num> {
  double findPriceIndex(num price) {
    if(price != 0){
      int startIndex = 0;
      int endIndex = 0;
      for (int i = 0; i < length; i++) {
        if ((this[i]) >= price.toInt()) {
          startIndex = i;
          break;
        }
      }
      for (int i = 0; i < length; i++) {
        if ((this[i]) <= price) {
          endIndex = i;
        }
      }
      if(startIndex == endIndex){
        return length.toDouble();
      }

      num a = this[startIndex] - this[endIndex];
      num b =  price - this[endIndex];
      num c = b / a;
      return startIndex.toDouble() + c;
    }else{
      return 0;
    }

  }
}

extension FindPrice on List<IncomeChartResponse> {
  num findPrice(DateTime time) {
    num price = 0;
    for (int i = 0; i < length; i++){
      if(this[i].time!.toEqualTime(time)){
        price = this[i].totalPrice ?? 0;
      }
    }
    return price;
  }

  num findPriceWithHour(DateTime time) {
    num price = 0;
    for (int i = 0; i < length; i++){
      if(this[i].time!.toEqualTimeWithHour(time)){
        price = this[i].totalPrice ?? 0;
      }
    }
    return price;
  }

}
