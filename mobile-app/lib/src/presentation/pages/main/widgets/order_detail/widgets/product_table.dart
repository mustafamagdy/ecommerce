import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/presentation/theme/theme.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

class ProductTable extends StatelessWidget {
  final OrderData? orderData;

  const ProductTable({Key? key, required this.orderData}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Table(
      defaultColumnWidth:
          FixedColumnWidth((MediaQuery.of(context).size.height - 90.r) / 5),
      border: TableBorder.all(color: AppColors.transparent),
      children: [
        TableRow(
          children: [
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  AppHelpers.getTranslation(TrKeys.number),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w600,
                    letterSpacing: -0.3,
                  ),
                )
              ],
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  AppHelpers.getTranslation(TrKeys.productName),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w600,
                    letterSpacing: -0.3,
                  ),
                )
              ],
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  AppHelpers.getTranslation(TrKeys.quantity),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w600,
                    letterSpacing: -0.3,
                  ),
                )
              ],
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  AppHelpers.getTranslation(TrKeys.totalPrice),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w600,
                    letterSpacing: -0.3,
                  ),
                )
              ],
            ),
          ],
        ),
        for (int i = 0; i < (orderData?.details?.length ?? 0); i++)
          TableRow(
            children: [
              Padding(
                padding: EdgeInsets.symmetric(vertical: 12.h),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Divider(),
                    Text(
                      "#${orderData?.details?[i].id ?? 0}",
                      style: GoogleFonts.inter(
                        fontSize: 14.sp,
                        color: AppColors.iconColor,
                        letterSpacing: -0.3,
                      ),
                    ),
                  ],
                ),
              ),
              Padding(
                padding: EdgeInsets.symmetric(vertical: 12.h),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Divider(),
                    Text(
                      (orderData?.details?[i].stock?.translation?.title ?? ""),
                      style: GoogleFonts.inter(
                        fontSize: 14.sp,
                        color: AppColors.iconColor,
                        letterSpacing: -0.3,
                      ),
                    )
                  ],
                ),
              ),
              Padding(
                padding: EdgeInsets.symmetric(vertical: 12.h),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Divider(),
                    Text(
                      (orderData?.details?[i].quantity ?? 0).toString(),
                      style: GoogleFonts.inter(
                        fontSize: 14.sp,
                        color: AppColors.iconColor,
                        letterSpacing: -0.3,
                      ),
                    )
                  ],
                ),
              ),
              Padding(
                padding: REdgeInsets.symmetric(vertical: 12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Divider(),
                    Text(
                      NumberFormat.currency(
                        symbol:
                        orderData?.currency?.symbol,
                      ).format(orderData?.details?[i].totalPrice ?? 0),
                      style: GoogleFonts.inter(
                        fontSize: 14.sp,
                        color: AppColors.iconColor,
                        letterSpacing: -0.3,
                      ),
                    )
                  ],
                ),
              ),
            ],
          ),
        for (int i = 0; i < (orderData?.details?.length ?? 0); i++)
          for (int j = 0; j < (orderData?.details?[i].addons?.length ?? 0); j++)
            TableRow(
              children: [
                Padding(
                  padding: EdgeInsets.symmetric(vertical: 12.h),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Divider(),
                      Text(
                        "#${orderData?.details?[i].addons?[j].id ?? 0}",
                        style: GoogleFonts.inter(
                          fontSize: 14.sp,
                          color: AppColors.iconColor,
                          letterSpacing: -0.3,
                        ),
                      ),
                    ],
                  ),
                ),
                Padding(
                  padding: EdgeInsets.symmetric(vertical: 12.h),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Divider(),
                      Text(
                        (orderData?.details?[i].addons?[j].stocks?.product?.translation
                                ?.title ??
                            ""),
                        style: GoogleFonts.inter(
                          fontSize: 14.sp,
                          color: AppColors.iconColor,
                          letterSpacing: -0.3,
                        ),
                      )
                    ],
                  ),
                ),
                Padding(
                  padding: EdgeInsets.symmetric(vertical: 12.h),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Divider(),
                      Text(
                        (orderData?.details?[i].addons?[j].quantity ?? 0)
                            .toString(),
                        style: GoogleFonts.inter(
                          fontSize: 14.sp,
                          color: AppColors.iconColor,
                          letterSpacing: -0.3,
                        ),
                      )
                    ],
                  ),
                ),
                Padding(
                  padding: REdgeInsets.symmetric(vertical: 12),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Divider(),
                      Text(
                        NumberFormat.currency(
                          symbol: orderData?.currency?.symbol,
                        ).format(orderData?.details?[i].addons?[j].price ?? 0),
                        style: GoogleFonts.inter(
                          fontSize: 14.sp,
                          color: AppColors.iconColor,
                          letterSpacing: -0.3,
                        ),
                      )
                    ],
                  ),
                ),
              ],
            ),
      ],
    );
  }
}
