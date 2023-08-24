import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/app_helpers.dart';
import '../../../../../components/components.dart';
import '../../../../../theme/theme.dart';
import '../../../riverpod/provider/main_provider.dart';
import '../icon_title.dart';
import 'custom_popup_item.dart';

class DragItem extends ConsumerWidget {
  final OrderData orderData;
  final bool isDrag;

  const DragItem({Key? key, required this.orderData, this.isDrag = false})
      : super(key: key);

  @override
  Widget build(BuildContext context, ref) {
    return InkWell(
      child: Transform.rotate(
        angle: isDrag ? (3.14 * (0.03)) : 0,
        child: Container(
          foregroundDecoration: BoxDecoration(
              borderRadius: BorderRadius.circular(16.r),
              color: isDrag ? AppColors.iconColor.withOpacity(0.3) : null),
          decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(16.r),
              color: AppColors.white),
          padding: EdgeInsets.all(12.r),
          margin: EdgeInsets.all(6.r),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  CommonImage(
                    imageUrl: orderData.user?.img,
                    height: 56,
                    width: 56,
                    radius: 32,
                  ),
                  8.w.horizontalSpace,
                  Expanded(
                    child: Padding(
                      padding: EdgeInsets.symmetric(
                          horizontal: 12.r, vertical: 12.r),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            orderData.user?.firstname ?? "",
                            maxLines: 1,
                            style: GoogleFonts.inter(
                                fontSize: 14.sp,
                                color: AppColors.black,
                                fontWeight: FontWeight.w600),
                          ),
                          Text(
                            "#${orderData.id}",
                            style: GoogleFonts.inter(
                                fontSize: 14.sp, color: AppColors.hintColor),
                          ),
                        ],
                      ),
                    ),
                  ),
                  CustomPopup(
                    orderData: orderData,
                    isLocation: orderData.deliveryType == TrKeys.delivery,
                  ),
                ],
              ),
              8.h.verticalSpace,
              const Divider(height: 2),
              16.verticalSpace,
              IconTitle(
                title: AppHelpers.getTranslation(TrKeys.date),
                icon: FlutterRemix.calendar_2_line,
                value: DateFormat("MMMM dd hh:mm")
                    .format(DateTime.parse(orderData.createdAt ?? "")),
              ),
              IconTitle(
                title: AppHelpers.getTranslation(TrKeys.amount),
                icon: FlutterRemix.money_dollar_circle_line,
                value: NumberFormat.currency(
                  symbol: orderData.currency?.symbol,
                ).format(orderData.totalPrice ?? 0),
              ),
              IconTitle(
                title: AppHelpers.getTranslation(TrKeys.paymentType),
                icon: FlutterRemix.money_euro_circle_line,
                value: orderData.transaction?.paymentSystem?.tag ?? "- -",
              ),
              (orderData.deliveryman?.firstname?.isNotEmpty ?? false)
                  ? IconTitle(
                      title: AppHelpers.getTranslation(TrKeys.deliveryman),
                      icon: FlutterRemix.car_line,
                      value: orderData.deliveryman?.firstname ?? "- -",
                    )
                  : const SizedBox.shrink(),
              (orderData.orderAddress?.address?.isNotEmpty ?? false)
                  ? IconTitle(
                      title: AppHelpers.getTranslation(TrKeys.address),
                      icon: FlutterRemix.map_pin_2_line,
                      value: orderData.orderAddress?.address ?? "- -",
                    )
                  : const SizedBox.shrink(),
              (orderData.transaction?.status?.isNotEmpty ?? false)
                  ? IconTitle(
                      title: AppHelpers.getTranslation(TrKeys.paymentStatus),
                      icon: FlutterRemix.money_dollar_circle_line,
                      value: orderData.transaction?.status ?? "- -",
                    )
                  : const SizedBox.shrink(),
              12.h.verticalSpace,
              Container(
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(20.r),
                    border: Border.all(color: AppColors.borderColor),
                  ),
                  child: Row(
                    children: [
                      Expanded(
                        child: Padding(
                          padding: EdgeInsets.all(12.r),
                          child: Text(
                            AppHelpers.getTranslation(
                                orderData.deliveryType ?? ""),
                            style: GoogleFonts.inter(
                              fontSize: 14.sp,
                              color: AppColors.black,
                            ),
                          ),
                        ),
                      ),
                      Container(
                        margin: EdgeInsets.all(5.r),
                        height: 32.sp,
                        width: 32.sp,
                        decoration: BoxDecoration(
                            border: Border.all(color: AppColors.black),
                            shape: BoxShape.circle),
                        child: (orderData.deliveryType ?? "") == TrKeys.dine
                            ? Padding(
                                padding: EdgeInsets.all(4.r),
                                child: SvgPicture.asset("assets/svg/dine.svg"))
                            : Icon(
                                (orderData.deliveryType ?? "") ==
                                        TrKeys.delivery
                                    ? FlutterRemix.e_bike_2_fill
                                    : FlutterRemix.walk_line,
                                size: 16.r,
                              ),
                      )
                    ],
                  ))
            ],
          ),
        ),
      ),
      onTap: () {
        ref.read(mainProvider.notifier).setOrder(orderData);
      },
    );
  }
}
