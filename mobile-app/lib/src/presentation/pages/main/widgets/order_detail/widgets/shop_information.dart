import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/models/data/shop_data.dart';
import 'package:admin_desktop/src/presentation/components/common_image.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import '../../../../../../core/utils/local_storage.dart';

class ShopInformation extends StatelessWidget {
  final ShopData? shop;

  const ShopInformation({Key? key, required this.shop}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(16.r),
        border: Border.all(color: AppColors.borderColor),
      ),
      padding: EdgeInsets.all(24.r),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            "${AppHelpers.getTranslation(TrKeys.shop)}/${AppHelpers.getTranslation(TrKeys.restaurant)} ${AppHelpers.getTranslation(TrKeys.information)}",
            style:
                GoogleFonts.inter(fontSize: 24.sp, fontWeight: FontWeight.w700),
          ),
          16.verticalSpace,
          Row(
            children: [
              CommonImage(
                imageUrl: shop?.logoImg ?? "",
                width: 100,
                height: 100,
              ),
              16.horizontalSpace,
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    shop?.translation?.title ?? "",
                    style: GoogleFonts.inter(fontWeight: FontWeight.w600),
                  ),
                  8.verticalSpace,
                  Row(
                    children: [
                      const Icon(FlutterRemix.phone_fill),
                      8.horizontalSpace,
                      Text(
                        shop?.phone ?? "",
                        style: GoogleFonts.inter(
                          fontSize: 20.sp,
                        ),
                      )
                    ],
                  ),
                  8.verticalSpace,
                  Row(
                    children: [
                      const Icon(FlutterRemix.money_dollar_circle_fill),
                      8.horizontalSpace,
                      Text(
                        NumberFormat.currency(
                          symbol: LocalStorage.instance
                              .getSelectedCurrency()
                              .symbol,
                        ).format(shop?.tax ?? 0),
                        style: GoogleFonts.inter(
                          fontSize: 20.sp,
                        ),
                      )
                    ],
                  ),
                  8.verticalSpace,
                  Row(
                    children: [
                      const Icon(FlutterRemix.map_2_line),
                      8.horizontalSpace,
                      SizedBox(
                        width: 200.w,
                        child: Text(
                          shop?.translation?.address ?? "",
                          style: GoogleFonts.inter(
                            fontSize: 20.sp,
                          ),
                        ),
                      )
                    ],
                  ),
                ],
              ),
            ],
          )
        ],
      ),
    );
  }
}
