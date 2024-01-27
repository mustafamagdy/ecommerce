import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/presentation/components/components.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/right_side/riverpod/provider/right_side_provider.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

class PromoCodeDialog extends ConsumerStatefulWidget {
  const PromoCodeDialog({Key? key}) : super(key: key);

  @override
  ConsumerState<PromoCodeDialog> createState() => _PromoCodeDialogState();
}

class _PromoCodeDialogState extends ConsumerState<PromoCodeDialog> {
  late TextEditingController controller;

  @override
  void initState() {
    controller = TextEditingController();
    super.initState();
  }

  @override
  void didChangeDependencies() {
    controller.text = ref.watch(rightSideProvider).coupon ?? "";
    super.didChangeDependencies();
  }

  @override
  void deactivate() {
    controller.dispose();
    super.deactivate();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 300.w,
      height: 240.w,
      padding: EdgeInsets.all(16.r),
      decoration: BoxDecoration(
          color: AppColors.white, borderRadius: BorderRadius.circular(10.r)),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            AppHelpers.getTranslation(TrKeys.addPromoCode),
            style:
                GoogleFonts.inter(fontWeight: FontWeight.w600, fontSize: 22.r),
          ),
          24.verticalSpace,
          OutlinedBorderTextField(
            textController: controller,
            label: AppHelpers.getTranslation(TrKeys.promoCode),
            onChanged: (s) {
              ref.read(rightSideProvider.notifier).checkPromoCode(context, s);
            },
          ),
          const Spacer(),
          LoginButton(
              isLoading: ref.watch(rightSideProvider).isPromoCodeLoading,
              isActive: ref.watch(rightSideProvider).isActive,
              title: AppHelpers.getTranslation(TrKeys.save),
              onPressed: () {
                if(ref.watch(rightSideProvider).isActive){
                  ref
                      .read(rightSideProvider.notifier)
                      .setCoupon(controller.text, context);
                  context.popRoute();
                }

              })
        ],
      ),
    );
  }
}
