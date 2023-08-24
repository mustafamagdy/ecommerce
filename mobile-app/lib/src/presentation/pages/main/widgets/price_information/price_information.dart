import 'package:admin_desktop/src/models/data/location_data.dart';
import 'package:admin_desktop/src/presentation/components/list_items/product_bag_item.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import '../../../../../core/constants/constants.dart';
import '../../../../../core/utils/utils.dart';
import '../../../../../models/models.dart';
import '../../../../components/components.dart';
import '../../../../theme/theme.dart';
import '../right_side/riverpod/provider/right_side_provider.dart';

class PriceInformation extends ConsumerStatefulWidget {
  final BagData bag;

  const PriceInformation({Key? key, required this.bag}) : super(key: key);

  @override
  ConsumerState<PriceInformation> createState() => _DeliveriesDrawerState();
}

class _DeliveriesDrawerState extends ConsumerState<PriceInformation> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(rightSideProvider.notifier).fetchCarts(
        checkYourNetwork: () {
          AppHelpers.showSnackBar(
            context,
            AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
          );
        },
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    final rightSideState = ref.watch(rightSideProvider);

    final rightSideNotifier = ref.read(rightSideProvider.notifier);
    return AbsorbPointer(
      absorbing: rightSideState.isProductCalculateLoading,
      child: Drawer(
        backgroundColor: AppColors.white,
        width: 465.r,
        child: rightSideState.isProductCalculateLoading
            ? Center(
                child: CircularProgressIndicator(
                  strokeWidth: 4.r,
                  color: AppColors.black,
                ),
              )
            : SingleChildScrollView(
                padding: EdgeInsets.all(24.r),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      AppHelpers.getTranslation(TrKeys.orderSummary),
                      style: GoogleFonts.inter(fontSize: 22.sp),
                    ),
                    28.verticalSpace,
                    ListView.builder(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount:
                          rightSideState.paginateResponse?.stocks?.length ?? 0,
                      itemBuilder: (context, index) {
                        return CartOrderItem(
                          symbol: widget.bag.selectedCurrency?.symbol,
                          delete: (){},
                          isActive: false,
                          add: () {},
                          remove: () {},
                          cart:
                              rightSideState.paginateResponse?.stocks?[index] ??
                                  ProductData(),
                        );
                      },
                    ),
                    28.verticalSpace,
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          AppHelpers.getTranslation(TrKeys.subtotal),
                          style: GoogleFonts.inter(
                            color: AppColors.black,
                            fontSize: 18.sp,
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                        Text(
                          NumberFormat.currency(
                            symbol: LocalStorage.instance
                                .getSelectedCurrency()
                                .symbol,
                          ).format(rightSideState.paginateResponse?.price ?? 0),
                          style: GoogleFonts.inter(
                            color: AppColors.black,
                            fontSize: 18.sp,
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                      ],
                    ),
                    20.verticalSpace,
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          AppHelpers.getTranslation(TrKeys.tax),
                          style: GoogleFonts.inter(
                            color: AppColors.black,
                            fontSize: 18.sp,
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                        Text(
                          NumberFormat.currency(
                            symbol: LocalStorage.instance
                                .getSelectedCurrency()
                                .symbol,
                          ).format(
                              rightSideState.paginateResponse?.totalTax ?? 0),
                          style: GoogleFonts.inter(
                            color: AppColors.black,
                            fontSize: 18.sp,
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                      ],
                    ),
                    20.verticalSpace,
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          AppHelpers.getTranslation(TrKeys.deliveryFee),
                          style: GoogleFonts.inter(
                            color: AppColors.black,
                            fontSize: 18.sp,
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                        Text(
                          NumberFormat.currency(
                            symbol: LocalStorage.instance
                                .getSelectedCurrency()
                                .symbol,
                          ).format(
                              rightSideState.paginateResponse?.deliveryFee ??
                                  0),
                          style: GoogleFonts.inter(
                            color: AppColors.black,
                            fontSize: 18.sp,
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                      ],
                    ),
                    20.verticalSpace,
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          AppHelpers.getTranslation(TrKeys.discount),
                          style: GoogleFonts.inter(
                            color: AppColors.black,
                            fontSize: 18.sp,
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                        Text(
                          "-${NumberFormat.currency(
                            symbol: LocalStorage.instance
                                .getSelectedCurrency()
                                .symbol,
                          ).format(rightSideState.paginateResponse?.totalDiscount ?? 0)}",
                          style: GoogleFonts.inter(
                            color: AppColors.red,
                            fontSize: 18.sp,
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                      ],
                    ),
                    20.verticalSpace,
                    rightSideState.paginateResponse?.couponPrice != 0
                        ? Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text(
                                AppHelpers.getTranslation(TrKeys.promoCode),
                                style: GoogleFonts.inter(
                                  color: AppColors.black,
                                  fontSize: 18.sp,
                                  fontWeight: FontWeight.w500,
                                  letterSpacing: -0.4,
                                ),
                              ),
                              Text(
                                "-${NumberFormat.currency(
                                  symbol: LocalStorage.instance
                                      .getSelectedCurrency()
                                      .symbol,
                                ).format(rightSideState.paginateResponse?.couponPrice ?? 0)}",
                                style: GoogleFonts.inter(
                                  color: AppColors.red,
                                  fontSize: 18.sp,
                                  fontWeight: FontWeight.w500,
                                  letterSpacing: -0.4,
                                ),
                              ),
                            ],
                          )
                        : const SizedBox.shrink(),
                    const Divider(),
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
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                        Text(
                          NumberFormat.currency(
                            symbol: LocalStorage.instance
                                .getSelectedCurrency()
                                .symbol,
                          ).format(
                              rightSideState.paginateResponse?.totalPrice ?? 0),
                          style: GoogleFonts.inter(
                            color: AppColors.black,
                            fontSize: 18.sp,
                            fontWeight: FontWeight.w500,
                            letterSpacing: -0.4,
                          ),
                        ),
                      ],
                    ),
                    20.verticalSpace,
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Expanded(
                          child: ConfirmButton(
                            height: 72.r,
                            title:
                                AppHelpers.getTranslation(TrKeys.confirmOrder),
                            onTap: () {
                              rightSideNotifier.createOrder(
                                  context,
                                  OrderBodyData(
                                      currencyId: rightSideState.selectedCurrency?.id ?? 0,
                                      rate: rightSideState.selectedCurrency?.rate ?? 0,
                                      bagData: widget.bag,
                                      userId:
                                          rightSideState.selectedUser?.id ?? 0,
                                      deliveryFee: (rightSideState
                                              .paginateResponse?.deliveryFee ??
                                          0),
                                      deliveryType: rightSideState.orderType,
                                      location: rightSideState.selectedAddress?.location ??
                                          LocationData(
                                              latitude: 0, longitude: 0),
                                      address: AddressModel(
                                          address: "${rightSideState.selectedAddress?.title}, ${rightSideState.selectedAddress?.address}",),
                                      deliveryDate: DateFormat("yyyy-MM-dd")
                                          .format(rightSideState.orderDate ??
                                              DateTime.now()),
                                      deliveryTime: rightSideState.orderTime?.hour
                                                  .toString()
                                                  .length ==
                                              2
                                          ? "${rightSideState.orderTime?.hour}:${rightSideState.orderTime?.minute.toString().padLeft(2, '0')}"
                                          : "0${rightSideState.orderTime?.hour}:${rightSideState.orderTime?.minute.toString().padLeft(2, '0')}"));
                            },
                            isLoading: rightSideState.isOrderLoading,
                          ),
                        ),
                        24.horizontalSpace,
                        Expanded(
                          child: ConfirmButton(
                            textColor: AppColors.black,
                            borderColor: AppColors.black,
                            bgColor: AppColors.transparent,
                            height: 72.r,
                            title: AppHelpers.getTranslation(TrKeys.close),
                            onTap: () {
                              context.popRoute();
                            },
                          ),
                        ),
                      ],
                    )
                  ],
                ),
              ),
      ),
    );
  }
}
