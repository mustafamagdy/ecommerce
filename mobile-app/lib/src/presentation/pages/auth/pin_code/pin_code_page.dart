// ignore_for_file: unrelated_type_equality_checks

import 'package:admin_desktop/generated/assets.dart';
import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/routes/app_router.gr.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/presentation/components/login_button.dart';
import 'package:admin_desktop/src/presentation/pages/auth/pin_code/riverpod/provider/pin_code_provider.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/svg.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../core/utils/local_storage.dart';
import '../../../theme/app_colors.dart';
import 'components/pin_button.dart';
import 'components/pin_container.dart';

class PinCodePage extends ConsumerWidget {
  final bool isNewPassword;

  const PinCodePage(this.isNewPassword, {Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final notifier = ref.read(pinCodeProvider.notifier);
    final state = ref.watch(pinCodeProvider);
    return Scaffold(
      body: SizedBox(
        width: MediaQuery.of(context).size.width,
        child: Row(
          children: [
            Container(
              constraints: BoxConstraints(maxWidth: 500.r),
              child: SafeArea(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    40.r.verticalSpace,
                    Row(
                      children: [
                        36.r.horizontalSpace,
                        SvgPicture.asset(
                          Assets.svgLogo,
                          height: 40.r,
                          width: 40.r,
                        ),
                        12.horizontalSpace,
                        Text(
                          AppHelpers.getAppName() ?? "foodyman",
                          style: GoogleFonts.inter(
                            color: AppColors.black,
                            fontWeight: FontWeight.bold,
                            fontSize: 22.sp,
                          ),
                        ),
                      ],
                    ),
                    Center(
                      child: Column(
                        children: [
                          64.r.verticalSpace,
                          Text(
                            AppHelpers.getTranslation(isNewPassword
                                ? TrKeys.enterNewPinCode
                                : state.isPinCodeNotValid == false
                                    ? TrKeys.enterPinCode
                                    : TrKeys.enterPinCodeError),
                            style: GoogleFonts.inter(
                              color: state.isPinCodeNotValid == false
                                  ? AppColors.black
                                  : AppColors.red,
                              fontWeight: FontWeight.w600,
                              fontSize: 28.sp,
                            ),
                            textAlign: TextAlign.center,
                          ),
                          12.verticalSpace,
                          Text(
                            AppHelpers.getTranslation(
                                state.isPinCodeNotValid == false
                                    ? TrKeys.pinCodeDesc
                                    : TrKeys.pinCodeDescError),
                            style: GoogleFonts.inter(
                              color: state.isPinCodeNotValid == false
                                  ? AppColors.black
                                  : AppColors.red,
                              fontWeight: FontWeight.w500,
                              fontSize: 16.sp,
                            ),
                            maxLines: 2,
                            textAlign: TextAlign.center,
                          ),
                          32.r.verticalSpace,
                          SizedBox(
                            height: 28.r,
                            child: ListView.builder(
                                physics: const NeverScrollableScrollPhysics(),
                                itemCount: 4,
                                shrinkWrap: true,
                                scrollDirection: Axis.horizontal,
                                itemBuilder: (context, index) {
                                  return PinContainer(
                                    isActive: state.pinCode.length > index,
                                  );
                                }),
                          ),
                          GridView.builder(
                              padding: EdgeInsets.symmetric(
                                  vertical: 24.r, horizontal: 68.r),
                              physics: const NeverScrollableScrollPhysics(),
                              itemCount: 12,
                              shrinkWrap: true,
                              gridDelegate:
                                  SliverGridDelegateWithFixedCrossAxisCount(
                                crossAxisCount: 3,
                                crossAxisSpacing: 28.w,
                                mainAxisSpacing: 24.h,
                                mainAxisExtent: 72.r,
                              ),
                              itemBuilder: (context, index) {
                                return PinButton(
                                  title: index != 9 && index != 11
                                      ? AppHelpers.getPinCodeText(index)
                                      : null,
                                  iconData: index == 9
                                      ? FlutterRemix.close_circle_line
                                      : index == 11
                                          ? FlutterRemix.delete_back_2_line
                                          : null,
                                  onTap: () {
                                    if (index == 9) {
                                      notifier.clearPinCode();
                                    } else if (index == 11) {
                                      notifier.removePinCode();
                                    } else {
                                      if (isNewPassword) {
                                        notifier.setNewPinCode(
                                          code:
                                              AppHelpers.getPinCodeText(index),
                                          onSuccess: () {
                                            context.replaceRoute(
                                                const MainRoute());
                                          },
                                        );
                                      } else {
                                        notifier.setPinCode(
                                          code:
                                              AppHelpers.getPinCodeText(index),
                                          onSuccess: () {
                                            context.replaceRoute(
                                                const MainRoute());
                                          },
                                        );
                                      }
                                    }
                                  },
                                );
                              }),
                          28.verticalSpace,
                          Padding(
                            padding: EdgeInsets.symmetric(horizontal: 48.w),
                            child: LoginButton(
                                title: AppHelpers.getTranslation(
                                    isNewPassword ? TrKeys.save : TrKeys.apply),
                                onPressed: () {
                                  if (isNewPassword) {
                                    notifier.checkNewCode(onSuccess: () {
                                      context.replaceRoute(const MainRoute());
                                    });
                                  } else {
                                    notifier.checkCode(onSuccess: () {
                                      context.replaceRoute(const MainRoute());
                                    });
                                  }
                                }),
                          ),
                          if (!isNewPassword)
                            Column(
                              children: [
                                16.verticalSpace,
                                Padding(
                                  padding:
                                      EdgeInsets.symmetric(horizontal: 48.w),
                                  child: LoginButton(
                                      bgColor: AppColors.transparent,
                                      title: AppHelpers.getTranslation(
                                          TrKeys.logout),
                                      onPressed: () {
                                        context
                                            .replaceRoute(const LoginRoute());
                                        LocalStorage.instance.clearStore();
                                      }),
                                ),
                              ],
                            ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
            Expanded(
                child: Image.asset(
              Assets.pngManImage,
              height: double.infinity,
              fit: BoxFit.cover,
            )),
          ],
        ),
      ),
    );
  }
}
