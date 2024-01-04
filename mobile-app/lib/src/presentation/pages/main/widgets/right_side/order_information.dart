// ignore_for_file: must_be_immutable

import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/core/utils/local_storage.dart';
import 'package:admin_desktop/src/models/data/bag_data.dart';
import 'package:admin_desktop/src/presentation/components/buttons/animation_button_effect.dart';
import 'package:admin_desktop/src/presentation/components/components.dart';
import 'package:admin_desktop/src/presentation/pages/main/riverpod/notifier/main_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/riverpod/provider/main_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/right_side/address/select_address_page.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/right_side/riverpod/notifier/right_side_notifier.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import 'riverpod/provider/right_side_provider.dart';
import 'riverpod/state/right_side_state.dart';

class OrderInformation extends ConsumerWidget {
  OrderInformation({
    Key? key,
  }) : super(key: key);

  List listOfType = [
    TrKeys.delivery,
    TrKeys.pickup,
    TrKeys.dine,
  ];

  @override
  Widget build(BuildContext context, ref) {
    final notifier = ref.read(rightSideProvider.notifier);
    final state = ref.watch(rightSideProvider);
    final BagData bag = state.bags[state.selectedBagIndex];
    return Container(
      width: MediaQuery.of(context).size.width / 2,
      padding: REdgeInsets.symmetric(
        horizontal: 24.r,
      ),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(10.r),
        color: AppColors.white,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(
                AppHelpers.getTranslation(TrKeys.order),
                style: GoogleFonts.inter(
                    fontSize: 22.r, fontWeight: FontWeight.w600),
              ),
              const Spacer(),
              IconButton(
                  onPressed: () {
                    context.popRoute();
                  },
                  icon: const Icon(FlutterRemix.close_line))
            ],
          ),
          16.verticalSpace,
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Container(
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(12.r),
                        border: Border.all(
                          color: AppColors.unselectedBottomBarBack,
                          width: 1.r,
                        ),
                      ),
                      alignment: Alignment.center,
                      height: 56.r,
                      padding: EdgeInsets.only(left: 16.r),
                      child: CustomDropdown(
                        hintText: AppHelpers.getTranslation(TrKeys.selectUser),
                        searchHintText:
                            AppHelpers.getTranslation(TrKeys.searchUser),
                        dropDownType: DropDownType.users,
                        onChanged: (value) =>
                            notifier.setUsersQuery(context, value),
                        initialUser: bag.selectedUser,
                      ),
                    ),
                    Visibility(
                        visible: state.selectUserError != null,
                        child: Padding(
                          padding: EdgeInsets.only(top: 6.r,left: 4.r),
                          child: Text(
                            AppHelpers.getTranslation(
                                state.selectUserError ?? ""),
                            style: GoogleFonts.inter(
                              color: AppColors.red,
                              fontSize: 14.sp
                            ),
                          ),
                        ),
                    ),
                    26.verticalSpace,
                    PopupMenuButton<int>(
                      itemBuilder: (context) {
                        return state.currencies
                            .map(
                              (currency) => PopupMenuItem<int>(
                                value: currency.id,
                                child: Text(
                                  '${currency.title}(${currency.symbol})',
                                  style: GoogleFonts.inter(
                                    fontWeight: FontWeight.w500,
                                    fontSize: 14.sp,
                                    color: AppColors.black,
                                    letterSpacing: -14 * 0.02,
                                  ),
                                ),
                              ),
                            )
                            .toList();
                      },
                      onSelected: notifier.setSelectedCurrency,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10.r),
                      ),
                      color: AppColors.white,
                      elevation: 10,
                      child: SelectFromButton(
                        title: state.selectedCurrency?.title ??
                            AppHelpers.getTranslation(TrKeys.selectCurrency),
                      ),
                    ),
                    Visibility(
                        visible: state.selectCurrencyError != null,
                      child: Padding(
                        padding: EdgeInsets.only(top: 6.r,left: 4.r),
                        child: Text(
                          AppHelpers.getTranslation(
                              state.selectCurrencyError ?? ""),
                          style: GoogleFonts.inter(
                              color: AppColors.red,
                              fontSize: 14.sp
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              16.horizontalSpace,
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    PopupMenuButton<int>(
                      itemBuilder: (context) {
                        AppHelpers.showAlertDialog(
                            context: context,
                            child: SizedBox(
                              child: SelectAddressPage(
                                location: state.selectedAddress?.location,
                                onSelect: (address) {
                                  notifier.setSelectedAddress(address: address);
                                  ref
                                      .read(rightSideProvider.notifier)
                                      .fetchCarts(
                                          checkYourNetwork: () {
                                            AppHelpers.showSnackBar(
                                              context,
                                              AppHelpers.getTranslation(TrKeys
                                                  .checkYourNetworkConnection),
                                            );
                                          },
                                          isNotLoading: true);
                                },
                              ),
                            ));

                        return [];
                      },
                      onSelected: (s) => notifier.setSelectedAddress(),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10.r),
                      ),
                      color: AppColors.white,
                      elevation: 10,
                      child: SelectFromButton(
                        title: state.selectedAddress?.address ??
                            AppHelpers.getTranslation(TrKeys.selectAddress),
                      ),
                    ),
                    Visibility(
                        visible: state.selectAddressError != null,
                      child: Padding(
                        padding: EdgeInsets.only(top: 6.r,left: 4.r),
                        child: Text(
                          AppHelpers.getTranslation(
                              state.selectAddressError ?? ""),
                          style: GoogleFonts.inter(
                              color: AppColors.red,
                              fontSize: 14.sp
                          ),
                        ),
                      ),
                    ),
                    26.verticalSpace,
                    PopupMenuButton<int>(
                      itemBuilder: (context) {
                        return state.payments
                            .map(
                              (payment) => PopupMenuItem<int>(
                                value: payment.id,
                                child: Text(
                                  '${payment.tag}',
                                  style: GoogleFonts.inter(
                                    fontWeight: FontWeight.w500,
                                    fontSize: 14.sp,
                                    color: AppColors.black,
                                    letterSpacing: -14 * 0.02,
                                  ),
                                ),
                              ),
                            )
                            .toList();
                      },
                      onSelected: notifier.setSelectedPayment,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10.r),
                      ),
                      color: AppColors.white,
                      elevation: 10,
                      child: SelectFromButton(
                        title: state.selectedPayment?.tag ??
                            AppHelpers.getTranslation(TrKeys.selectPayment),
                      ),
                    ),
                    Visibility(
                        visible: state.selectPaymentError != null,
                      child: Padding(
                        padding: EdgeInsets.only(top: 6.r,left: 4.r),
                        child: Text(
                          AppHelpers.getTranslation(
                              state.selectPaymentError ?? ""),
                          style: GoogleFonts.inter(
                              color: AppColors.red,
                              fontSize: 14.sp
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          12.verticalSpace,
          const Divider(),
          12.verticalSpace,
          Text(
            AppHelpers.getTranslation(TrKeys.shippingInformation),
            style:
                GoogleFonts.inter(fontWeight: FontWeight.w600, fontSize: 22.r),
          ),
          16.verticalSpace,
          Row(
            children: [
              ...listOfType
                  .map((e) => Expanded(
                        child: InkWell(
                          onTap: () {
                            notifier.setSelectedOrderType(e);
                            if (state.orderType.toLowerCase() !=
                                e.toString().toLowerCase()) {
                              ref.read(rightSideProvider.notifier).fetchCarts(
                                  checkYourNetwork: () {
                                    AppHelpers.showSnackBar(
                                      context,
                                      AppHelpers.getTranslation(
                                          TrKeys.checkYourNetworkConnection),
                                    );
                                  },
                                  isNotLoading: true);
                            }
                          },
                          child: AnimationButtonEffect(
                            child: Container(
                              margin: EdgeInsets.symmetric(horizontal: 4.r),
                              decoration: BoxDecoration(
                                color: state.orderType.toLowerCase() ==
                                        e.toString().toLowerCase()
                                    ? AppColors.brandColor
                                    : AppColors.editProfileCircle,
                                borderRadius: BorderRadius.circular(6.r),
                              ),
                              padding: EdgeInsets.symmetric(vertical: 10.r),
                              child: Center(
                                child: Row(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Container(
                                      decoration: BoxDecoration(
                                        color: AppColors.transparent,
                                        shape: BoxShape.circle,
                                        border:
                                            Border.all(color: AppColors.black),
                                      ),
                                      padding: EdgeInsets.all(6.r),
                                      child: e == TrKeys.delivery
                                          ? Icon(
                                              FlutterRemix.takeaway_fill,
                                              size: 18.sp,
                                            )
                                          : e == TrKeys.pickup
                                              ? SvgPicture.asset(
                                                  "assets/svg/pickup.svg")
                                              : SvgPicture.asset(
                                                  "assets/svg/dine.svg"),
                                    ),
                                    8.horizontalSpace,
                                    Text(
                                      AppHelpers.getTranslation(e),
                                      style: GoogleFonts.inter(
                                          fontSize: 14.sp,
                                          fontWeight: FontWeight.w600),
                                    )
                                  ],
                                ),
                              ),
                            ),
                          ),
                        ),
                      ))
                  .toList(),
            ],
          ),
          12.verticalSpace,
          Row(
            children: [
              Expanded(
                child: PopupMenuButton<int>(
                  itemBuilder: (context) {
                    showDatePicker(
                      context: context,
                      initialDate: state.orderDate ?? DateTime.now(),
                      firstDate: DateTime.now(),
                      lastDate: DateTime.now().add(
                        const Duration(days: 1000),
                      ),
                      builder: (context, child) {
                        return Theme(
                          data: Theme.of(context).copyWith(
                            colorScheme: const ColorScheme.light(
                              primary: AppColors.brandColor,
                              onPrimary: AppColors.black,
                              onSurface: AppColors.black,
                            ),
                            textButtonTheme: TextButtonThemeData(
                              style: TextButton.styleFrom(
                                foregroundColor: AppColors.black,
                              ),
                            ),
                          ),
                          child: child!,
                        );
                      },
                    ).then((date) {
                      if (date != null) {
                        notifier.setDate(date);
                      }
                    });
                    return [];
                  },
                  onSelected: (s) {},
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10.r),
                  ),
                  color: AppColors.white,
                  elevation: 10,
                  child: SelectFromButton(
                    title: state.orderDate == null
                        ? AppHelpers.getTranslation(TrKeys.selectDeliveryDate)
                        : DateFormat("MMM dd")
                            .format(state.orderDate ?? DateTime.now()),
                  ),
                ),
              ),
              16.horizontalSpace,
              Expanded(
                child: PopupMenuButton<int>(
                  itemBuilder: (context) {
                    showTimePicker(
                      context: context,
                      initialTime: state.orderTime ?? TimeOfDay.now(),
                      builder: (context, child) {
                        return Theme(
                          data: Theme.of(context).copyWith(
                            colorScheme: const ColorScheme.light(
                              primary: AppColors.brandColor,
                              onPrimary: AppColors.black,
                              onSurface: AppColors.black,
                            ),
                            textButtonTheme: TextButtonThemeData(
                              style: TextButton.styleFrom(
                                foregroundColor: AppColors.black,
                              ),
                            ),
                          ),
                          child: child!,
                        );
                      },
                    ).then((time) {
                      if (time != null) {
                        notifier.setTime(time);
                      }
                    });
                    return [];
                  },
                  onSelected: (s) {},
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10.r),
                  ),
                  color: AppColors.white,
                  elevation: 10,
                  child: SelectFromButton(
                    title: state.orderTime == null
                        ? AppHelpers.getTranslation(TrKeys.selectDeliveryTime)
                        : "${state.orderTime?.format(context)}",
                  ),
                ),
              ),
            ],
          ),
          24.verticalSpace,
          const Divider(),
          24.verticalSpace,
          _priceInformation(
              state: state,
              notifier: notifier,
              bag: bag,
              context: context,
              mainNotifier: ref.read(mainProvider.notifier))
        ],
      ),
    );
  }

  Widget _priceInformation(
      {required RightSideState state,
      required RightSideNotifier notifier,
      required MainNotifier mainNotifier,
      required BagData bag,
      required BuildContext context}) {
    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              AppHelpers.getTranslation(TrKeys.subtotal),
              style: GoogleFonts.inter(
                color: AppColors.black,
                fontSize: 14.sp,
                fontWeight: FontWeight.w500,
                letterSpacing: -0.4,
              ),
            ),
            Text(
              NumberFormat.currency(
                symbol: bag.selectedCurrency?.symbol ??
                    LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(state.paginateResponse?.price ?? 0),
              style: GoogleFonts.inter(
                color: AppColors.black,
                fontSize: 14.sp,
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
                fontSize: 14.sp,
                fontWeight: FontWeight.w500,
                letterSpacing: -0.4,
              ),
            ),
            Text(
              NumberFormat.currency(
                symbol: bag.selectedCurrency?.symbol ??
                    LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(state.paginateResponse?.totalTax ?? 0),
              style: GoogleFonts.inter(
                color: AppColors.black,
                fontSize: 14.sp,
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
                fontSize: 14.sp,
                fontWeight: FontWeight.w500,
                letterSpacing: -0.4,
              ),
            ),
            Text(
              NumberFormat.currency(
                symbol: bag.selectedCurrency?.symbol ??
                    LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(state.paginateResponse?.deliveryFee ?? 0),
              style: GoogleFonts.inter(
                color: AppColors.black,
                fontSize: 14.sp,
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
                fontSize: 14.sp,
                fontWeight: FontWeight.w500,
                letterSpacing: -0.4,
              ),
            ),
            Text(
              "-${NumberFormat.currency(
                symbol: bag.selectedCurrency?.symbol ??
                    LocalStorage.instance.getSelectedCurrency().symbol,
              ).format(state.paginateResponse?.totalDiscount ?? 0)}",
              style: GoogleFonts.inter(
                color: AppColors.red,
                fontSize: 14.sp,
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
                      fontSize: 14.sp,
                      fontWeight: FontWeight.w500,
                      letterSpacing: -0.4,
                    ),
                  ),
                  Text(
                    "-${NumberFormat.currency(
                      symbol: bag.selectedCurrency?.symbol ??
                          LocalStorage.instance.getSelectedCurrency().symbol,
                    ).format(state.paginateResponse?.couponPrice ?? 0)}",
                    style: GoogleFonts.inter(
                      color: AppColors.red,
                      fontSize: 14.sp,
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
            SizedBox(
              width: 186.w,
              child: LoginButton(
                  title: AppHelpers.getTranslation(TrKeys.placeOrder),
                  onPressed: () {
                    notifier.placeOrder(
                      checkYourNetwork: () {
                        AppHelpers.showSnackBar(
                          context,
                          AppHelpers.getTranslation(
                              TrKeys.checkYourNetworkConnection),
                        );
                      },
                      openSelectDeliveriesDrawer: () {
                        mainNotifier.setPriceDate(state.paginateResponse);
                        context.popRoute();
                      },
                    );
                  }),
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  AppHelpers.getTranslation(TrKeys.totalPrice),
                  style: GoogleFonts.inter(
                    color: AppColors.black,
                    fontSize: 18.sp,
                    fontWeight: FontWeight.w500,
                    letterSpacing: -0.4,
                  ),
                ),
                Text(
                  NumberFormat.currency(
                    symbol: bag.selectedCurrency?.symbol ??
                        LocalStorage.instance.getSelectedCurrency().symbol,
                  ).format(state.paginateResponse?.totalPrice ?? 0),
                  style: GoogleFonts.inter(
                    color: AppColors.black,
                    fontSize: 30.sp,
                    fontWeight: FontWeight.w600,
                    letterSpacing: -0.4,
                  ),
                ),
              ],
            ),
          ],
        ),
      ],
    );
  }
}
