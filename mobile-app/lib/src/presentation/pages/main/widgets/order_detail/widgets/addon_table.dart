import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/utils.dart';
import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/presentation/theme/theme.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

class AddonTable extends StatelessWidget {
  final OrderDetail? orderDetail;

  const AddonTable({Key? key, required this.orderDetail}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          AppHelpers.getTranslation(TrKeys.addons),
          style: GoogleFonts.inter(
            fontWeight: FontWeight.w700,
            fontSize: 24.sp,
          ),
        ),
        24.verticalSpace,
        Container(
          width: double.infinity,
          decoration: BoxDecoration(
            color: AppColors.white,
            borderRadius: BorderRadius.circular(16.r),
            border: Border.all(color: AppColors.borderColor),
          ),
          padding: EdgeInsets.all(24.r),
          child: Table(
            defaultColumnWidth:
                FixedColumnWidth(MediaQuery.of(context).size.height / 9.6),
            border: TableBorder.all(color: AppColors.transparent),
            children: [
              TableRow(
                children: [
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        AppHelpers.getTranslation(TrKeys.id),
                        style: GoogleFonts.inter(
                          fontSize: 18.sp,
                          color: AppColors.black,
                          fontWeight: FontWeight.w700,
                          letterSpacing: -0.3,
                        ),
                      )
                    ],
                  ),
                  Column(
                    children: [
                      Text(
                        AppHelpers.getTranslation(TrKeys.productName),
                        style: GoogleFonts.inter(
                          fontSize: 18.sp,
                          color: AppColors.black,
                          fontWeight: FontWeight.w700,
                          letterSpacing: -0.3,
                        ),
                      )
                    ],
                  ),
                  Column(
                    children: [
                      Text(
                        AppHelpers.getTranslation(TrKeys.price),
                        style: GoogleFonts.inter(
                          fontSize: 18.sp,
                          color: AppColors.black,
                          fontWeight: FontWeight.w700,
                          letterSpacing: -0.3,
                        ),
                      )
                    ],
                  ),
                  Column(
                    children: [
                      Text(
                        AppHelpers.getTranslation(TrKeys.quantity),
                        style: GoogleFonts.inter(
                          fontSize: 18.sp,
                          color: AppColors.black,
                          fontWeight: FontWeight.w700,
                          letterSpacing: -0.3,
                        ),
                      )
                    ],
                  ),
                  Column(
                    children: [
                      Text(
                        AppHelpers.getTranslation(TrKeys.tax),
                        style: GoogleFonts.inter(
                          fontSize: 18.sp,
                          color: AppColors.black,
                          fontWeight: FontWeight.w700,
                          letterSpacing: -0.3,
                        ),
                      )
                    ],
                  ),
                  Column(
                    children: [
                      Text(
                        AppHelpers.getTranslation(TrKeys.totalPrice),
                        style: GoogleFonts.inter(
                          fontSize: 18.sp,
                          color: AppColors.black,
                          fontWeight: FontWeight.w700,
                          letterSpacing: -0.3,
                        ),
                      )
                    ],
                  ),
                ],
              ),
              for (int j = 0; j < (orderDetail?.addons?.length ?? 0); j++)
                TableRow(
                  children: [
                    Padding(
                      padding: EdgeInsets.symmetric(vertical: 12.h),
                      child: Column(
                        children: [
                          const Divider(),
                          Text(
                            (orderDetail?.addons?[j].id ?? 0).toString(),
                            style: GoogleFonts.inter(
                              fontSize: 16.sp,
                              color: AppColors.black,
                              letterSpacing: -0.3,
                            ),
                          )
                        ],
                      ),
                    ),
                    Padding(
                      padding: EdgeInsets.symmetric(vertical: 12.h),
                      child: Column(
                        children: [
                          const Divider(),
                          Text(
                            (orderDetail
                                    ?.addons?[j].stocks?.translation?.title ??
                                ""),
                            style: GoogleFonts.inter(
                              fontSize: 16.sp,
                              color: AppColors.black,
                              letterSpacing: -0.3,
                            ),
                          )
                        ],
                      ),
                    ),
                    Padding(
                      padding: REdgeInsets.symmetric(vertical: 12),
                      child: Column(
                        children: [
                          const Divider(),
                          Text(
                            NumberFormat.currency(
                              symbol: LocalStorage.instance
                                  .getSelectedCurrency()
                                  .symbol,
                            ).format(
                                orderDetail?.addons?[j].stocks?.price ?? 0),
                            style: GoogleFonts.inter(
                              fontSize: 16.sp,
                              color: AppColors.black,
                              letterSpacing: -0.3,
                            ),
                          )
                        ],
                      ),
                    ),
                    Padding(
                      padding: EdgeInsets.symmetric(vertical: 12.h),
                      child: Column(
                        children: [
                          const Divider(),
                          Text(
                            (orderDetail?.addons?[j].quantity ?? 0).toString(),
                            style: GoogleFonts.inter(
                              fontSize: 16.sp,
                              color: AppColors.black,
                              letterSpacing: -0.3,
                            ),
                          )
                        ],
                      ),
                    ),
                    Padding(
                      padding: REdgeInsets.symmetric(vertical: 12),
                      child: Column(
                        children: [
                          const Divider(),
                          Text(
                            NumberFormat.currency(
                              symbol: LocalStorage.instance
                                  .getSelectedCurrency()
                                  .symbol,
                            ).format(orderDetail?.addons?[j].stocks?.tax ?? 0),
                            style: GoogleFonts.inter(
                              fontSize: 16.sp,
                              color: AppColors.black,
                              letterSpacing: -0.3,
                            ),
                          )
                        ],
                      ),
                    ),
                    Padding(
                      padding: REdgeInsets.symmetric(vertical: 12),
                      child: Column(
                        children: [
                          const Divider(),
                          Text(
                            NumberFormat.currency(
                              symbol: LocalStorage.instance
                                  .getSelectedCurrency()
                                  .symbol,
                            ).format(
                                orderDetail?.addons?[j].stocks?.totalPrice ??
                                    0),
                            style: GoogleFonts.inter(
                              fontSize: 16.sp,
                              color: AppColors.black,
                              letterSpacing: -0.3,
                            ),
                          )
                        ],
                      ),
                    ),
                  ],
                ),
            ],
          ),
        ),
      ],
    );
  }
}
