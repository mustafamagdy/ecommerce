import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/core/utils/local_storage.dart';
import 'package:admin_desktop/src/models/data/addons_data.dart';
import 'package:admin_desktop/src/presentation/components/buttons/animation_button_effect.dart';
import 'package:admin_desktop/src/presentation/components/common_image.dart';
import 'package:admin_desktop/src/presentation/components/components.dart';
import 'package:admin_desktop/src/presentation/pages/main/riverpod/notifier/main_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/riverpod/provider/main_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/riverpod/state/main_state.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/order_calculate/price_info.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/right_side/riverpod/notifier/right_side_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/right_side/riverpod/state/right_side_state.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart' as intl;
import '../right_side/riverpod/provider/right_side_provider.dart';

class OrderCalculate extends ConsumerWidget {
  const OrderCalculate({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context, ref) {
    final notifier = ref.read(mainProvider.notifier);
    final rightNotifier = ref.read(rightSideProvider.notifier);
    final state = ref.read(mainProvider);
    final stateRight = ref.watch(rightSideProvider);
    return Scaffold(
      backgroundColor: AppColors.mainBack,
      body: Padding(
        padding: EdgeInsets.all(16.r),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _informationWidget(notifier, rightNotifier, state, stateRight),
            16.horizontalSpace,
            calculator(stateRight, rightNotifier)
          ],
        ),
      ),
    );
  }

  Widget calculator(
      RightSideState stateRight, RightSideNotifier rightSideNotifier) {
    return Expanded(
      child: Container(
        decoration: const BoxDecoration(color: AppColors.white),
        padding: EdgeInsets.symmetric(vertical: 28.r, horizontal: 16.r),
        child: Column(
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                Row(
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          AppHelpers.getTranslation(TrKeys.payableAmount),
                          style: GoogleFonts.inter(
                              fontSize: 18.sp, fontWeight: FontWeight.w600),
                        ),
                        6.verticalSpace,
                        Text(
                          intl.NumberFormat.currency(
                            symbol: LocalStorage.instance
                                .getSelectedCurrency()
                                .symbol,
                          ).format(stateRight.selectedUser?.wallet?.price ?? 0),
                          style: GoogleFonts.inter(
                              fontSize: 26.sp, fontWeight: FontWeight.w600),
                        ),
                      ],
                    )
                  ],
                ),
                const Spacer(),
                if(stateRight.selectedUser != null)
                Row(
                  children: [
                    CommonImage(
                      imageUrl: stateRight.selectedUser?.img ?? "",
                      width: 50,
                      height: 50,
                      radius: 25,
                    ),
                    16.horizontalSpace,
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          "${stateRight.selectedUser?.firstname ?? ""} ${stateRight.selectedUser?.lastname ?? ""}",
                          style: GoogleFonts.inter(
                              fontWeight: FontWeight.w600, fontSize: 18.sp),
                        ),
                        Text(
                          "#${AppHelpers.getTranslation(TrKeys.id)}${stateRight.selectedUser?.id ?? ""}",
                          style: GoogleFonts.inter(
                              fontWeight: FontWeight.w500,
                              fontSize: 14.sp,
                              color: AppColors.iconColor),
                        ),
                      ],
                    ),
                  ],
                ),
              ],
            ),
            16.verticalSpace,
            const Divider(),
            const Spacer(),
            Container(
              width: double.infinity,
              padding: EdgeInsets.all(20.r),
              decoration: BoxDecoration(
                  border: Border.all(color: AppColors.differBorderColor),
                  borderRadius: BorderRadius.circular(8.r)),
              child: Align(
                alignment: Alignment.centerRight,
                child: Text(
                  stateRight.calculate,
                  style: GoogleFonts.inter(
                      fontWeight: FontWeight.w600, fontSize: 24.sp),
                  maxLines: 1,
                ),
              ),
            ),
           const Spacer(),
            GridView.builder(
                physics: const NeverScrollableScrollPhysics(),
                itemCount: 12,
                shrinkWrap: true,
                gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 3,
                  crossAxisSpacing: 28.w,
                  mainAxisSpacing: 24.h,
                  mainAxisExtent: 78.r,
                ),
                itemBuilder: (context, index) {
                  return InkWell(
                    onTap: () {
                      rightSideNotifier.setCalculate(index == 9
                          ? "00"
                          : index == 10
                              ? "0"
                              : index == 11
                                  ? "-1"
                                  : (index + 1).toString());
                    },
                    child: AnimationButtonEffect(
                      child: Container(
                        decoration: BoxDecoration(
                          color: AppColors.addButtonColor,
                          borderRadius: BorderRadius.circular(6.r),
                        ),
                        child: Center(
                          child: index == 11
                              ? const Icon(FlutterRemix.delete_back_2_line)
                              : Text(
                                  index == 9
                                      ? "00"
                                      : index == 10
                                          ? "0"
                                          : (index + 1).toString(),
                                  style: GoogleFonts.inter(
                                      fontSize: 24.sp,
                                      fontWeight: FontWeight.w600),
                                ),
                        ),
                      ),
                    ),
                  );
                }),
            16.verticalSpace,
            Row(
              children: [
                Expanded(
                  flex: 1,
                  child: InkWell(
                    onTap: () {
                      rightSideNotifier.setCalculate(".");
                    },
                    child: AnimationButtonEffect(
                      child: Container(
                        height: 78.r,
                        decoration: BoxDecoration(
                          color: AppColors.addButtonColor,
                          borderRadius: BorderRadius.circular(6.r),
                        ),
                        child: Center(
                          child: Text(
                            ".",
                            style: GoogleFonts.inter(
                                fontSize: 24.sp, fontWeight: FontWeight.w600),
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
                28.horizontalSpace,
                Expanded(
                  flex: 2,
                  child: AnimationButtonEffect(
                    child: Container(
                      height: 78.r,
                      decoration: BoxDecoration(
                        color: AppColors.addButtonColor,
                        borderRadius: BorderRadius.circular(6.r),
                      ),
                      child: Center(
                        child: Text(
                          AppHelpers.getTranslation(TrKeys.ok),
                          style: GoogleFonts.inter(
                              fontSize: 24.sp, fontWeight: FontWeight.w600),
                        ),
                      ),
                    ),
                  ),
                ),
              ],
            )
          ],
        ),
      ),
    );
  }

  Widget _informationWidget(
      MainNotifier notifier,
      RightSideNotifier rightSideNotifier,
      MainState state,
      RightSideState stateRight) {
    return Expanded(
      child: SingleChildScrollView(
        child: Column(
          children: [
            Row(
              children: [
                InkWell(
                  onTap: () {
                    notifier.setPriceDate(null);
                  },
                  child: Row(
                    children: [
                      Icon(
                        FlutterRemix.arrow_left_s_line,
                        size: 32.r,
                      ),
                      Text(
                        AppHelpers.getTranslation(TrKeys.back),
                        style: GoogleFonts.inter(
                          fontSize: 16.sp,
                        ),
                      ),
                    ],
                  ),
                ),
                const Spacer(),
                InkWell(
                  onTap: () {
                    rightSideNotifier.fetchCarts();
                  },
                  child: AnimationButtonEffect(
                    child: Container(
                        decoration: BoxDecoration(
                            color: AppColors.white,
                            borderRadius: BorderRadius.circular(10.r)),
                        padding: EdgeInsets.all(8.r),
                        child: const Icon(FlutterRemix.restart_line)),
                  ),
                )
              ],
            ),
            16.verticalSpace,
            Container(

              padding: EdgeInsets.symmetric(vertical: 20.r, horizontal: 16.r),
              decoration: BoxDecoration(
                  color: AppColors.white,
                  borderRadius: BorderRadius.circular(10.r)),
              child: ListView(
                physics: const NeverScrollableScrollPhysics(),
                shrinkWrap: true,
                children: [
                  Text(
                    AppHelpers.getTranslation(TrKeys.order),
                    style: GoogleFonts.inter(
                        fontWeight: FontWeight.w600, fontSize: 22.sp),
                  ),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        "${stateRight.bags[stateRight.selectedBagIndex].selectedUser?.firstname ?? ""} ${stateRight.bags[stateRight.selectedBagIndex].selectedUser?.lastname ?? ""}",
                        style: GoogleFonts.inter(
                            fontSize: 16.sp, color: AppColors.iconColor),
                      ),
                      Text(
                        stateRight.orderType,
                        style: GoogleFonts.inter(
                            fontSize: 16.sp, color: AppColors.iconColor),
                      ),
                    ],
                  ),
                  8.verticalSpace,
                  const Divider(),
                  8.verticalSpace,
                  Text(
                    AppHelpers.getTranslation(TrKeys.totalItem),
                    style: GoogleFonts.inter(
                        fontWeight: FontWeight.w600, fontSize: 18.sp),
                  ),
                  ListView.builder(
                      padding: EdgeInsets.only(top: 16.r),
                      physics: const NeverScrollableScrollPhysics(),
                      shrinkWrap: true,
                      itemCount: stateRight.paginateResponse?.stocks?.length ?? 0,
                      itemBuilder: (context, index) {
                        return Padding(
                          padding: EdgeInsets.only(bottom: 16.r),
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    stateRight.paginateResponse?.stocks?[index]
                                            .stock?.product?.translation?.title ??
                                        "",
                                    style: GoogleFonts.inter(
                                        fontWeight: FontWeight.w600,
                                        fontSize: 16.sp,
                                        color: AppColors.black),
                                  ),
                                  for (Addons e in (stateRight.paginateResponse
                                          ?.stocks?[index].addons ??
                                      []))
                                    Text(
                                      "${e.product?.translation?.title ?? ""} ( ${intl.NumberFormat.currency(
                                        symbol: LocalStorage.instance
                                            .getSelectedCurrency()
                                            .symbol,
                                      ).format((e.price ?? 0) / (e.quantity ?? 1))} x ${(e.quantity ?? 1)} )",
                                      style: GoogleFonts.inter(
                                        fontSize: 15.sp,
                                        color: AppColors.unselectedTab,
                                      ),
                                    ),
                                ],
                              ),
                              Text(
                                intl.NumberFormat.currency(
                                  symbol: LocalStorage.instance
                                      .getSelectedCurrency()
                                      .symbol,
                                ).format(stateRight.paginateResponse
                                        ?.stocks?[index].totalPrice ??
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
                  const Divider(),
                  PriceInfo(
                    bag: stateRight.bags[stateRight.selectedBagIndex],
                    state: stateRight,
                    notifier: rightSideNotifier,
                    mainNotifier: notifier,
                  )
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
