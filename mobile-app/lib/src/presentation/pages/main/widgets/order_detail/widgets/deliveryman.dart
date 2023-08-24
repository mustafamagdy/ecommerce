import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/models/data/user_data.dart';
import 'package:admin_desktop/src/presentation/components/buttons/animation_button_effect.dart';
import 'package:admin_desktop/src/presentation/components/buttons/confirm_button.dart';
import 'package:admin_desktop/src/presentation/components/common_image.dart';
import 'package:admin_desktop/src/presentation/components/dropdowns/custom_dropdown.dart';
import 'package:admin_desktop/src/presentation/theme/theme.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:url_launcher/url_launcher.dart';

class DeliverymanScreen extends StatelessWidget {
  final OrderData? orderData;
  final UserData? selectUser;
  final ValueChanged? onChanged;
  final VoidCallback setDeliveryman;

  const DeliverymanScreen(
      {Key? key,
      required this.orderData,
      this.selectUser,
      this.onChanged,
      required this.setDeliveryman})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              AppHelpers.getTranslation(TrKeys.deliveryman),
              style: GoogleFonts.inter(
                  fontSize: 24.sp, fontWeight: FontWeight.w700),
            ),
            (orderData?.status == "ready") &&
                    (orderData?.deliveryType != "pickup") &&
                    (orderData?.deliveryman == null)
                ? ConfirmButton(
                    title:
                        "${AppHelpers.getTranslation(TrKeys.add)} ${AppHelpers.getTranslation(TrKeys.deliveryman)}",
                    onTap: () {
                      showDialog(
                          context: context,
                          builder: (contextt) {
                            return AlertDialog(
                              content: Container(
                                decoration: BoxDecoration(
                                  borderRadius: BorderRadius.circular(12.r),
                                  border: Border.all(
                                    color: AppColors.unselectedBottomBarBack,
                                    width: 1.r,
                                  ),
                                ),
                                alignment: Alignment.center,
                                height: 64.r,
                                padding: EdgeInsets.only(left: 8.r),
                                child: CustomDropdown(
                                  key: UniqueKey(),
                                  hintText: AppHelpers.getTranslation(
                                      TrKeys.selectDeliveryman),
                                  searchHintText:
                                      AppHelpers.getTranslation(TrKeys.search),
                                  dropDownType: DropDownType.categories,
                                  onChanged: onChanged,
                                  initialUser: selectUser,
                                ),
                              ),
                              actions: [
                                Padding(
                                  padding: EdgeInsets.only(right: 16.r),
                                  child: SizedBox(
                                    width: 150.w,
                                    child: ConfirmButton(
                                        title: AppHelpers.getTranslation(
                                            TrKeys.save),
                                        onTap: () {
                                          selectUser == null
                                              ? null
                                              : setDeliveryman();
                                          context.popRoute();
                                        }),
                                  ),
                                ),
                              ],
                            );
                          });
                    },
                    height: 72.r,
                  )
                : const SizedBox.shrink()
          ],
        ),
        16.verticalSpace,
        orderData?.deliveryType == "pickup"
            ? Text(
                AppHelpers.getTranslation(TrKeys.typePickup),
                style: GoogleFonts.inter(
                  fontSize: 16.sp,
                ),
              )
            : orderData?.deliveryman != null
                ? Row(
                    children: [
                      CommonImage(
                        imageUrl: orderData?.deliveryman?.img,
                        width: 60.r,
                        height: 60.r,
                        radius: 30.r,
                      ),
                      16.horizontalSpace,
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            "${orderData?.deliveryman?.firstname ?? ""} ${orderData?.deliveryman?.lastname ?? ""}",
                            style: GoogleFonts.inter(
                                fontSize: 20.sp, fontWeight: FontWeight.w700),
                          ),
                          4.verticalSpace,
                          Text(
                            orderData?.deliveryman?.role ?? "",
                            style: GoogleFonts.inter(
                                fontSize: 16.sp, fontWeight: FontWeight.w500),
                          ),
                        ],
                      ),
                      const Spacer(),
                      InkWell(
                        onTap: () async {
                          final Uri launchUri = Uri(
                            scheme: 'tel',
                            path: orderData?.deliveryman?.phone ?? "",
                          );
                          await launchUrl(launchUri);
                        },
                        child: AnimationButtonEffect(
                          child: Container(
                            decoration: const BoxDecoration(
                                color: AppColors.black, shape: BoxShape.circle),
                            padding: EdgeInsets.all(10.r),
                            child: const Icon(
                              FlutterRemix.phone_fill,
                              color: AppColors.white,
                            ),
                          ),
                        ),
                      ),
                      8.horizontalSpace,
                      InkWell(
                        onTap: () async {
                          final Uri launchUri = Uri(
                            scheme: 'sms',
                            path: orderData?.deliveryman?.phone ?? "",
                          );
                          await launchUrl(launchUri);
                        },
                        child: AnimationButtonEffect(
                          child: Container(
                            decoration: const BoxDecoration(
                                color: AppColors.black, shape: BoxShape.circle),
                            padding: EdgeInsets.all(10.r),
                            child: const Icon(
                              FlutterRemix.chat_1_fill,
                              color: AppColors.white,
                            ),
                          ),
                        ),
                      )
                    ],
                  )
                : orderData?.status != "ready"
                    ? Text(
                        AppHelpers.getTranslation(TrKeys.statusReady),
                        style: GoogleFonts.inter(
                          fontSize: 16.sp,
                        ),
                      )
                    : Text(
                        AppHelpers.getTranslation(TrKeys.notAssigned),
                        style: GoogleFonts.inter(
                          fontSize: 16.sp,
                        ),
                      )
      ],
    );
  }
}
