import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/app_helpers.dart';
import '../../../../../components/common_image.dart';
import '../../../../../theme/app_colors.dart';

class OrdersInfo extends StatelessWidget {
  final String image;
  final String name;
  final String lastName;
  final String id;
  final String orderTime;
  final String status;
  final String deliveryType;
  final bool active;

  const OrdersInfo(
      {super.key,
      required this.image,
      required this.name,
      required this.lastName,
      required this.id,
      required this.orderTime,
      required this.status,
      required this.deliveryType,
      required this.active});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      width: 228.w,
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadiusDirectional.circular(10),
        boxShadow: [
          BoxShadow(
              offset: const Offset(0, 5),
              blurRadius: 8.r,
              color: active ? AppColors.shadowSecond : AppColors.transparent)
        ],
        border: Border.all(
            color: active ? AppColors.brandColor : AppColors.transparent),
      ),
      child: Padding(
        padding: EdgeInsets.all(16.r),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                CommonImage(
                    height: 40, width: 40, radius: 100, imageUrl: image),
                10.horizontalSpace,
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    SizedBox(
                      width: 130.r,
                      child: Text(
                        '$name $lastName.',
                        style: GoogleFonts.inter(
                            fontSize: 16.sp,
                            fontWeight: FontWeight.w600,
                            color: AppColors.black),
                      ),
                    ),
                    2.verticalSpace,
                    Text(
                      id,
                      style: GoogleFonts.inter(
                          fontSize: 12.sp,
                          fontWeight: FontWeight.w500,
                          color: AppColors.iconColor),
                    ),
                  ],
                ),
                const Spacer(),
                Icon(
                  FlutterRemix.more_2_fill,
                  size: 22.r,
                )
              ],
            ),
            Expanded(
              child: Divider(
                color: AppColors.iconColor.withOpacity(0.6),
              ),
            ),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  AppHelpers.getTranslation(TrKeys.orderTime),
                  style: GoogleFonts.inter(
                      fontSize: 14.sp, fontWeight: FontWeight.w500),
                ),
                Text(
                  orderTime,
                  style: GoogleFonts.inter(
                      fontSize: 12.sp,
                      fontWeight: FontWeight.w500,
                      color: AppColors.iconColor),
                ),
              ],
            ),
            Expanded(
              child: Divider(
                color: AppColors.iconColor.withOpacity(0.6),
              ),
            ),
            4.verticalSpace,
            Row(
              children: [
                Container(
                  height: 30.r,
                  width: 30.r,
                  decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      border: Border.all(color: AppColors.black)),
                  child: Center(
                      child: deliveryType == TrKeys.dine
                          ? SvgPicture.asset("assets/svg/dine.svg")
                          : Icon(
                              deliveryType == TrKeys.pickup
                                  ? FlutterRemix.walk_line
                                  : FlutterRemix.e_bike_2_fill,
                              size: 18,
                            )),
                ),
                8.horizontalSpace,
                deliveryType == TrKeys.pickup
                    ? Text(
                        AppHelpers.getTranslation(TrKeys.takeAway),
                        style: GoogleFonts.inter(
                            fontSize: 14.sp,
                            fontWeight: FontWeight.w600,
                            color: AppColors.black),
                      )
                    : deliveryType == TrKeys.dine
                        ? Text(
                            AppHelpers.getTranslation(TrKeys.dine),
                            style: GoogleFonts.inter(
                                fontSize: 14.sp,
                                fontWeight: FontWeight.w600,
                                color: AppColors.black),
                          )
                        : Text(
                            AppHelpers.getTranslation(TrKeys.delivery),
                            style: GoogleFonts.inter(
                                fontSize: 14.sp,
                                fontWeight: FontWeight.w600,
                                color: AppColors.black),
                          )
              ],
            ),
            const Spacer(),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 7),
              decoration: BoxDecoration(
                  color: status == TrKeys.accepted
                      ? AppColors.blue
                      : status == TrKeys.ready
                          ? AppColors.brandColor
                          : status == TrKeys.canceled
                              ? AppColors.red
                              : AppColors.revenueColor,
                  borderRadius: BorderRadiusDirectional.circular(100)),
              child: Text(
                status == TrKeys.accepted
                    ? TrKeys.newKey
                    : status == TrKeys.ready
                        ? TrKeys.done
                        : status == TrKeys.canceled
                            ? TrKeys.cancel
                            : TrKeys.cooking,
                style: GoogleFonts.inter(
                    fontSize: 12.sp,
                    fontWeight: FontWeight.w600,
                    color: AppColors.white),
              ),
            )
          ],
        ),
      ),
    );
  }
}
