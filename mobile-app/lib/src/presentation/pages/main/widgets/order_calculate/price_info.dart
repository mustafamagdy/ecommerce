import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/core/utils/local_storage.dart';
import 'package:admin_desktop/src/models/data/bag_data.dart';
import 'package:admin_desktop/src/models/data/location_data.dart';
import 'package:admin_desktop/src/models/data/order_body_data.dart';
import 'package:admin_desktop/src/presentation/pages/main/riverpod/notifier/main_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/new/new_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/right_side/riverpod/notifier/right_side_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/right_side/riverpod/state/right_side_state.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart' as intl;

import '../../../../components/components.dart';
import '../orders_table/orders/accepted/accepted_orders_provider.dart';

class PriceInfo extends StatelessWidget {
  final BagData bag;
  final RightSideState state;
  final RightSideNotifier notifier;
  final MainNotifier mainNotifier;

  const PriceInfo(
      {Key? key,
      required this.state,
      required this.notifier,
      required this.bag,
      required this.mainNotifier})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
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
              intl.NumberFormat.currency(
                symbol: bag.selectedCurrency?.symbol ??
                    LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(state.paginateResponse?.price ?? 0),
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
              intl.NumberFormat.currency(
                symbol: bag.selectedCurrency?.symbol ??
                    LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(state.paginateResponse?.totalTax ?? 0),
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
              intl.NumberFormat.currency(
                symbol: bag.selectedCurrency?.symbol ??
                    LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(state.paginateResponse?.deliveryFee ?? 0),
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
              "-${intl.NumberFormat.currency(
                symbol: bag.selectedCurrency?.symbol ??
                    LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(state.paginateResponse?.totalDiscount ?? 0)}",
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
        state.paginateResponse?.couponPrice != 0
            ? Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    AppHelpers.getTranslation(TrKeys.promoCode),
                    style: GoogleFonts.inter(
                      color: AppColors.black,
                      fontSize: 16.sp,
                      fontWeight: FontWeight.w500,
                      letterSpacing: -0.4,
                    ),
                  ),
                  Text(
                    "-${intl.NumberFormat.currency(
                      symbol: bag.selectedCurrency?.symbol ??
                          LocalStorage.instance.getSelectedCurrency().symbol,
                    ).format(state.paginateResponse?.couponPrice ?? 0)}",
                    style: GoogleFonts.inter(
                      color: AppColors.red,
                      fontSize: 16.sp,
                      fontWeight: FontWeight.w400,
                      letterSpacing: -0.4,
                    ),
                  ),
                ],
              )
            : const SizedBox.shrink(),
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
              intl.NumberFormat.currency(
                symbol: bag.selectedCurrency?.symbol ??
                    LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(state.paginateResponse?.totalPrice ?? 0),
              style: GoogleFonts.inter(
                color: AppColors.black,
                fontSize: 22.sp,
                fontWeight: FontWeight.w600,
                letterSpacing: -0.4,
              ),
            ),
          ],
        ),
        20.verticalSpace,
        state.calculate.isEmpty
            ? const SizedBox.shrink()
            : Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    AppHelpers.getTranslation(TrKeys.refund),
                    style: GoogleFonts.inter(
                      color: AppColors.black,
                      fontSize: 18.sp,
                      fontWeight: FontWeight.w600,
                      letterSpacing: -0.4,
                    ),
                  ),
                  Text(
                    intl.NumberFormat.currency(
                      symbol: bag.selectedCurrency?.symbol ??
                          LocalStorage.instance.getSelectedCurrency().symbol,
                    ).format((state.paginateResponse?.totalPrice ?? 0) -
                        (double.tryParse(state.calculate) ?? 0)),
                    style: GoogleFonts.inter(
                      color: AppColors.black,
                      fontSize: 22.sp,
                      fontWeight: FontWeight.w600,
                      letterSpacing: -0.4,
                    ),
                  ),
                ],
              ),
        32.verticalSpace,
        Consumer(
          builder: (BuildContext context, WidgetRef ref, Widget? child) {
            return LoginButton(
                isLoading: state.isOrderLoading,
                title: AppHelpers.getTranslation(TrKeys.confirmOrder),
                onPressed: () {
                  notifier.createOrder(
                      context,
                      OrderBodyData(
                          bagData: bag,
                          coupon: state.coupon,
                          note: state.comment,
                          userId: state.selectedUser?.id ?? 0,
                          deliveryFee:
                              (state.paginateResponse?.deliveryFee ?? 0),
                          deliveryType: state.orderType,
                          location: state.selectedAddress?.location ??
                              LocationData(latitude: 0, longitude: 0),
                          address: AddressModel(
                              address: state.selectedAddress?.address),
                          deliveryDate: intl.DateFormat("yyyy-MM-dd")
                              .format(state.orderDate ?? DateTime.now()),
                          deliveryTime: state.orderTime != null
                              ? (state.orderTime?.hour.toString().length == 2
                                  ? "${state.orderTime?.hour}:${state.orderTime?.minute.toString().padLeft(2, '0')}"
                                  : "0${state.orderTime?.hour}:${state.orderTime?.minute.toString().padLeft(2, '0')}")
                              : (TimeOfDay.now().hour.toString().length == 2
                                  ? "${TimeOfDay.now().hour}:${TimeOfDay.now().minute.toString().padLeft(2, '0')}"
                                  : "0${TimeOfDay.now().hour}:${TimeOfDay.now().minute.toString().padLeft(2, '0')}"),
                          currencyId: state.currencies
                                  .firstWhere(
                                      (element) => element.isDefault ?? false)
                                  .id ??
                              0,
                          rate: state.selectedCurrency?.rate ?? 0),
                      onSuccess: () {
                    ref.read(newOrdersProvider.notifier).fetchNewOrders(isRefresh: true);
                    ref
                        .read(acceptedOrdersProvider.notifier)
                        .fetchAcceptedOrders(isRefresh: true);
                    AppHelpers.showAlertDialog(
                        context: context,
                        child: Container(
                          width: 200.w,
                          height: 200.w,
                          padding: EdgeInsets.all(30.r),
                          decoration: BoxDecoration(
                              color: AppColors.white,
                              borderRadius: BorderRadius.circular(10.r)),
                          child: Column(
                            children: [
                              Container(
                                decoration: const BoxDecoration(
                                    color: AppColors.brandColor,
                                    shape: BoxShape.circle),
                                padding: EdgeInsets.all(12.r),
                                child: Icon(
                                  Icons.check,
                                  size: 56.r,
                                  color: AppColors.white,
                                ),
                              ),
                              const Spacer(),
                              Text(
                                AppHelpers.getTranslation(
                                    TrKeys.thankYouForOrder),
                                style: GoogleFonts.inter(
                                    fontWeight: FontWeight.w600,
                                    fontSize: 22.r),
                                textAlign: TextAlign.center,
                              )
                            ],
                          ),
                        ));
                    mainNotifier.setPriceDate(null);
                  });
                });
          },
        )
      ],
    );
  }
}
