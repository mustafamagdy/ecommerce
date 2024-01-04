import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/presentation/theme/theme.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

class TimeInformation extends StatelessWidget {
  final OrderData? orderData;
  const TimeInformation({Key? key, required this.orderData}) : super(key: key);

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
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                "${AppHelpers.getTranslation(TrKeys.createdDate)}: ${orderData?.createdAt?.substring(0, orderData?.createdAt?.lastIndexOf(":"))}",
                style: GoogleFonts.inter(fontSize: 20.sp),
              ),
              16.verticalSpace,
              Text(
                "${AppHelpers.getTranslation(TrKeys.deliveryDate)}: ${orderData?.deliveryDate}",
                style: GoogleFonts.inter(fontSize: 20.sp),
              ),
              16.verticalSpace,
              Text(
                "${AppHelpers.getTranslation(TrKeys.paymentStatus)}: ${orderData?.transaction?.status ?? ""}",
                style: GoogleFonts.inter(fontSize: 20.sp),
              ),
              16.verticalSpace,
              SizedBox(
                width: 300.r,
                child: Text(
                  "${AppHelpers.getTranslation(TrKeys.address)}: ${orderData?.orderAddress?.address ?? ""}",
                  style: GoogleFonts.inter(fontSize: 20.sp),
                ),
              )
            ],
          ),
          const Spacer(),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                "${AppHelpers.getTranslation(TrKeys.status)}: ${orderData?.status}",
                style: GoogleFonts.inter(fontSize: 20.sp),
              ),
              16.verticalSpace,
              Text(
                "${AppHelpers.getTranslation(TrKeys.deliveryType)}: ${orderData?.deliveryType}",
                style: GoogleFonts.inter(fontSize: 20.sp),
              ),
              16.verticalSpace,
              Text(
                "${AppHelpers.getTranslation(TrKeys.paymentType)}: ${orderData?.transaction?.paymentSystem?.tag ?? ""}",
                style: GoogleFonts.inter(fontSize: 20.sp),
              )
            ],
          ),
          const Spacer(),
        ],
      ),
    );
  }
}
