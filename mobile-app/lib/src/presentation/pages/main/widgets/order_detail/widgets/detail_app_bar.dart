import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/utils.dart';
import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:intl/intl.dart';

import 'app_bar_item.dart';

class DetailAppBar extends StatelessWidget {
  final OrderData? orderData;

  const DetailAppBar({Key? key, required this.orderData}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(16.r),
        border: Border.all(color: AppColors.borderColor),
      ),
      padding: EdgeInsets.all(24.r),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          AppbarItem(
              title: AppHelpers.getTranslation(TrKeys.deliveryDate),
              icon: FlutterRemix.calendar_line,
              desc: orderData?.deliveryDate ?? ""),
          AppbarItem(
              title: AppHelpers.getTranslation(TrKeys.totalPrice),
              icon: FlutterRemix.bank_card_line,
              desc: NumberFormat.currency(
                symbol: LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(orderData?.totalPrice ?? 0)),
          AppbarItem(
              title: AppHelpers.getTranslation(TrKeys.messages),
              icon: FlutterRemix.message_2_line,
              desc: orderData?.deliveryDate ?? ""),
          AppbarItem(
              title: AppHelpers.getTranslation(TrKeys.products),
              icon: FlutterRemix.shopping_cart_line,
              desc: "${orderData?.details?.length ?? 0}"),
        ],
      ),
    );
  }
}
