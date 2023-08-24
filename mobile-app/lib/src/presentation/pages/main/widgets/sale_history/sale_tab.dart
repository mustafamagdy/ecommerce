import 'package:admin_desktop/src/core/utils/utils.dart';
import 'package:admin_desktop/src/models/response/sale_history_response.dart';
import 'package:admin_desktop/src/presentation/components/buttons/animation_button_effect.dart';
import 'package:admin_desktop/src/presentation/theme/theme.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import '../../../../../core/constants/constants.dart';

class SaleTab extends StatelessWidget {
  final List<SaleHistoryModel> list;
  final bool isLoading;
  final bool isMoreLoading;
  final bool hasMore;
  final VoidCallback viewMore;

  const SaleTab(
      {Key? key,
      required this.list,
      required this.isLoading,
      required this.hasMore,
      required this.viewMore,
      required this.isMoreLoading})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(vertical: 22.r),
      decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(10.r), color: AppColors.white),
      child: isLoading
          ? const Center(
              child: CircularProgressIndicator(
              color: AppColors.black,
            ))
          : SingleChildScrollView(
              child: Column(
                children: [
                  Table(
                    defaultColumnWidth: FixedColumnWidth(
                        (MediaQuery.of(context).size.height) / 4.6.r),
                    border: TableBorder.all(color: AppColors.transparent),
                    children: [
                      TableRow(
                        children: [
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Padding(
                                padding: EdgeInsets.only(left: 16.r),
                                child: Text(
                                  AppHelpers.getTranslation(TrKeys.id),
                                  style: GoogleFonts.inter(
                                    fontSize: 16.sp,
                                    color: AppColors.black,
                                    fontWeight: FontWeight.w600,
                                    letterSpacing: -0.3,
                                  ),
                                ),
                              )
                            ],
                          ),
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                AppHelpers.getTranslation(TrKeys.client),
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
                                AppHelpers.getTranslation(TrKeys.amount),
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
                                AppHelpers.getTranslation(TrKeys.paymentType),
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
                                AppHelpers.getTranslation(TrKeys.note),
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
                                AppHelpers.getTranslation(TrKeys.date),
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
                      for (int i = 0; i < list.length; i++)
                        TableRow(
                          children: [
                            Padding(
                              padding: EdgeInsets.symmetric(vertical: 12.h),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  const Divider(),
                                  Padding(
                                    padding: EdgeInsets.only(left: 16.r),
                                    child: Text(
                                      "#${AppHelpers.getTranslation(TrKeys.id)}${list[i].id}",
                                      style: GoogleFonts.inter(
                                        fontSize: 14.sp,
                                        color: AppColors.iconColor,
                                        letterSpacing: -0.3,
                                      ),
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
                                    "${list[i].user?.firstname} ${list[i].user?.lastname}",
                                    style: GoogleFonts.inter(
                                      fontSize: 14.sp,
                                      color: AppColors.iconColor,
                                      letterSpacing: -0.3,
                                    ),
                                    maxLines: 1,
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
                                    NumberFormat.currency(
                                      symbol: LocalStorage.instance
                                          .getSelectedCurrency()
                                          .symbol,
                                    ).format(list[i].totalPrice ?? 0),
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
                                    (list[i].transactions?.isNotEmpty ?? false)
                                        ? list[i]
                                                .transactions
                                                ?.first
                                                .paymentSystem
                                                ?.tag ??
                                            ""
                                        : AppHelpers.getTranslation(TrKeys.na),
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
                                    list[i].note ??
                                        AppHelpers.getTranslation(TrKeys.na),
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
                                    DateFormat('d MMM yyyy').format(
                                        list[i].createdAt ?? DateTime.now()),
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
                  ),
                  isMoreLoading
                      ? const Padding(
                          padding: EdgeInsets.only(top: 18),
                          child: Center(
                              child: CircularProgressIndicator(
                            color: AppColors.black,
                          )),
                        )
                      : hasMore
                          ? InkWell(
                              borderRadius: BorderRadius.circular(10.r),
                              onTap: () {
                                viewMore.call();
                              },
                              child: AnimationButtonEffect(
                                child: Container(
                                  margin: EdgeInsets.symmetric(
                                      horizontal: 64.r, vertical: 16.r),
                                  height: 50.r,
                                  decoration: BoxDecoration(
                                    borderRadius: BorderRadius.circular(10.r),
                                    border: Border.all(
                                      color: AppColors.black.withOpacity(0.17),
                                      width: 1.r,
                                    ),
                                  ),
                                  alignment: Alignment.center,
                                  child: Text(
                                    AppHelpers.getTranslation(TrKeys.viewMore),
                                    style: GoogleFonts.inter(
                                      fontSize: 16.sp,
                                      color: AppColors.black,
                                      fontWeight: FontWeight.w500,
                                    ),
                                  ),
                                ),
                              ),
                            )
                          : const SizedBox.shrink()
                ],
              ),
            ),
    );
  }
}
