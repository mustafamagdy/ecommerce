import 'package:admin_desktop/src/models/data/addons_data.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/kitchen/riverpod/kitchen_provider.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import '../../../../../core/constants/constants.dart';
import '../../../../../core/utils/app_helpers.dart';
import '../../../../components/components.dart';

class OrderInfo extends ConsumerWidget {
  const OrderInfo({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context, ref) {
    final state = ref.watch(kitchenProvider);
    final event = ref.read(kitchenProvider.notifier);
    return Container(
      margin: EdgeInsets.only(right: 16.r, top: 16.r, bottom: 16.r),
      decoration: BoxDecoration(
          color: AppColors.white, borderRadius: BorderRadius.circular(10)),
      child: state.selectOrder != null
          ? SingleChildScrollView(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.start,
                children: [
                  Padding(
                    padding: EdgeInsets.only(top: 16.r, left: 16.r),
                    child: Row(
                      children: [
                        CommonImage(
                          imageUrl: state.selectOrder?.user?.img ?? "",
                          width: 40.r,
                          height: 40.r,
                          radius: 20.r,
                        ),
                        10.horizontalSpace,
                        Text(
                          "${state.selectOrder?.user?.firstname} ${state.selectOrder?.user?.lastname}",
                          style: GoogleFonts.inter(
                              fontWeight: FontWeight.w600, fontSize: 16.sp),
                        )
                      ],
                    ),
                  ),
                  const Divider(),
                  Padding(
                    padding: EdgeInsets.only(left: 16.r),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        6.verticalSpace,
                        Text(
                          AppHelpers.getTranslation(TrKeys.order),
                          style: GoogleFonts.inter(
                              fontWeight: FontWeight.w600, fontSize: 18.sp),
                        ),
                        10.verticalSpace,
                        Row(
                          children: [
                            Text(
                              "#${AppHelpers.getTranslation(TrKeys.id)}${state.selectOrder?.id}",
                              style: GoogleFonts.inter(
                                  fontWeight: FontWeight.w500,
                                  fontSize: 16.sp,
                                  color: AppColors.iconColor),
                            ),
                            12.horizontalSpace,
                            Container(
                              width: 8.r,
                              height: 8.r,
                              decoration: const BoxDecoration(
                                  color: AppColors.iconColor,
                                  shape: BoxShape.circle),
                            ),
                            12.horizontalSpace,
                            Text(
                              DateFormat("MMM d, h:mm a").format(
                                  DateTime.tryParse(
                                          state.selectOrder?.createdAt ?? "") ??
                                      DateTime.now()),
                              style: GoogleFonts.inter(
                                  fontWeight: FontWeight.w500,
                                  fontSize: 16.sp,
                                  color: AppColors.iconColor),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                  const Divider(),
                  Padding(
                    padding: EdgeInsets.only(left: 16.r),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        10.verticalSpace,
                        Text(
                          AppHelpers.getTranslation(TrKeys.totalItem),
                          style: GoogleFonts.inter(
                              fontWeight: FontWeight.w600, fontSize: 18.sp),
                        ),
                        ListView.builder(
                            padding: EdgeInsets.only(top: 16.r, right: 16.r),
                            physics: const NeverScrollableScrollPhysics(),
                            shrinkWrap: true,
                            itemCount: state.selectOrder?.details?.length ?? 0,
                            itemBuilder: (context, index) {
                              return Padding(
                                padding: EdgeInsets.only(bottom: 16.r),
                                child: Row(
                                  mainAxisAlignment:
                                      MainAxisAlignment.spaceBetween,
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Column(
                                      crossAxisAlignment:
                                          CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          "${state.selectOrder?.details?[index].stock?.product?.translation?.title ?? ""} x ${state.selectOrder?.details?[index].quantity ?? ""}",
                                          style: GoogleFonts.inter(
                                              fontWeight: FontWeight.w600,
                                              fontSize: 16.sp,
                                              color: AppColors.black),
                                        ),
                                        6.verticalSpace,
                                        for (Addons e in (state.selectOrder
                                                ?.details?[index].addons ??
                                            []))
                                          Text(
                                            "${e.stocks?.product?.translation?.title ?? ""} ( ${NumberFormat.currency(
                                              symbol: state.selectOrder
                                                      ?.currency?.symbol ??
                                                  "",
                                            ).format((e.price ?? 0) / (e.quantity ?? 1))} x ${(e.quantity ?? 1)} )",
                                            style: GoogleFonts.inter(
                                              fontSize: 15.sp,
                                              color: AppColors.unselectedTab,
                                            ),
                                          ),
                                      ],
                                    ),
                                    Text(
                                      NumberFormat.currency(
                                        symbol:
                                            state.selectOrder?.currency?.symbol,
                                      ).format(state.selectOrder
                                              ?.details?[index].totalPrice ??
                                          0),
                                      style: GoogleFonts.inter(
                                          fontWeight: FontWeight.w500,
                                          fontSize: 14.sp,
                                          color: AppColors.black),
                                    )
                                  ],
                                ),
                              );
                            }),
                      ],
                    ),
                  ),
                  const Divider(),
                  Padding(
                    padding: EdgeInsets.symmetric(horizontal: 16.r),
                    child: Column(
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              AppHelpers.getTranslation(TrKeys.subtotal),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontSize: 16.sp,
                                fontWeight: FontWeight.w500,
                                letterSpacing: -0.4,
                              ),
                            ),
                            Text(
                              NumberFormat.currency(
                                symbol: state.selectOrder?.currency?.symbol,
                              ).format((state.selectOrder?.totalPrice ?? 0) -
                                  (state.selectOrder?.deliveryFee ?? 0) -
                                  (state.selectOrder?.tax ?? 0) -
                                  (state.selectOrder?.totalDiscount ?? 0)),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontSize: 16.sp,
                                fontWeight: FontWeight.w500,
                                letterSpacing: -0.4,
                              ),
                            ),
                          ],
                        ),
                        12.verticalSpace,
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              AppHelpers.getTranslation(TrKeys.tax),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontSize: 16.sp,
                                fontWeight: FontWeight.w500,
                                letterSpacing: -0.4,
                              ),
                            ),
                            Text(
                              NumberFormat.currency(
                                symbol: state.selectOrder?.currency?.symbol,
                              ).format((state.selectOrder?.tax ?? 0)),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontSize: 16.sp,
                                fontWeight: FontWeight.w400,
                                letterSpacing: -0.4,
                              ),
                            ),
                          ],
                        ),
                        12.verticalSpace,
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              AppHelpers.getTranslation(TrKeys.deliveryFee),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontSize: 16.sp,
                                fontWeight: FontWeight.w500,
                                letterSpacing: -0.4,
                              ),
                            ),
                            Text(
                              NumberFormat.currency(
                                symbol: state.selectOrder?.currency?.symbol,
                              ).format((state.selectOrder?.deliveryFee ?? 0)),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontSize: 16.sp,
                                fontWeight: FontWeight.w400,
                                letterSpacing: -0.4,
                              ),
                            ),
                          ],
                        ),
                        12.verticalSpace,
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              AppHelpers.getTranslation(TrKeys.discount),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontSize: 16.sp,
                                fontWeight: FontWeight.w500,
                                letterSpacing: -0.4,
                              ),
                            ),
                            Text(
                              "-${NumberFormat.currency(
                                symbol: state.selectOrder?.currency?.symbol,
                              ).format((state.selectOrder?.totalDiscount ?? 0))}",
                              style: GoogleFonts.inter(
                                color: AppColors.red,
                                fontSize: 16.sp,
                                fontWeight: FontWeight.w400,
                                letterSpacing: -0.4,
                              ),
                            ),
                          ],
                        ),
                        12.verticalSpace,
                        const Divider(),
                        20.verticalSpace,
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              AppHelpers.getTranslation(TrKeys.totalPrice),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontSize: 18.sp,
                                fontWeight: FontWeight.w600,
                                letterSpacing: -0.4,
                              ),
                            ),
                            Text(
                              NumberFormat.currency(
                                symbol: state.selectOrder?.currency?.symbol,
                              ).format((state.selectOrder?.totalPrice ?? 0)),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontSize: 22.sp,
                                fontWeight: FontWeight.w600,
                                letterSpacing: -0.4,
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                  24.verticalSpace,
                  if (!(state.selectOrder?.status == TrKeys.ready ||
                      state.selectOrder?.status == TrKeys.canceled))
                    Padding(
                      padding: EdgeInsets.symmetric(horizontal: 22.r),
                      child: LoginButton(
                          title: AppHelpers.getTranslation(
                              state.selectOrder?.status == TrKeys.accepted
                                  ? TrKeys.startCooking
                                  : TrKeys.done),
                          onPressed: () {
                            event.changeStatus();
                          }),
                    ),
                  if (state.selectOrder?.status != TrKeys.canceled)
                    Padding(
                      padding: EdgeInsets.symmetric(
                          vertical: 22.r, horizontal: 22.r),
                      child: LoginButton(
                          titleColor: AppColors.white,
                          title: AppHelpers.getTranslation(TrKeys.cancel),
                          bgColor: AppColors.red,
                          onPressed: () {
                            AppHelpers.showAlertDialog(
                              context: context,
                              child: Container(
                                decoration: BoxDecoration(
                                    borderRadius: BorderRadius.circular(16.r)),
                                padding: EdgeInsets.all(16.r),
                                width: 300.r,
                                child: Column(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    Text(
                                      "${AppHelpers.getTranslation(TrKeys.areYouSureChange)} ${AppHelpers.getTranslation(TrKeys.cancel)}",
                                      textAlign: TextAlign.center,
                                      style: GoogleFonts.inter(),
                                    ),
                                    16.verticalSpace,
                                    Row(
                                      children: [
                                        Expanded(
                                          child: LoginButton(
                                              title: AppHelpers.getTranslation(
                                                  TrKeys.cancel),
                                              onPressed: () {
                                                context.popRoute();
                                              },
                                            bgColor: AppColors.transparent,

                                          ),
                                        ),
                                        24.horizontalSpace,
                                        Expanded(
                                          child: LoginButton(
                                              title: AppHelpers.getTranslation(
                                                  TrKeys.apply),
                                              onPressed: () {
                                                context.popRoute();
                                                event.changeStatus(status: TrKeys.canceled);
                                              },
                                          bgColor: AppColors.red,
                                            titleColor: AppColors.white,
                                          ),
                                        ),
                                      ],
                                    )
                                  ],
                                ),
                              ),
                            );

                          }),
                    )
                ],
              ),
            )
          : Center(
              child: Text(AppHelpers.getTranslation(TrKeys.thereAreNoOrders))),
    );
  }
}
