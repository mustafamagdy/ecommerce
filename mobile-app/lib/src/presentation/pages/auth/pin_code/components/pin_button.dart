import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../components/buttons/animation_button_effect.dart';

class PinButton extends StatelessWidget {
  final String? title;
  final IconData? iconData;
  final VoidCallback onTap;

  const PinButton({Key? key, this.title, this.iconData, required this.onTap})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return AnimationButtonEffect(
      child: InkWell(
        borderRadius: BorderRadius.circular(5.r),
        onTap: onTap,
        child: Container(
          height: 68.r,
          width: 68.r,
          decoration: BoxDecoration(
              border: Border.all(width: 2, color: AppColors.outlineButtonBorder),
              borderRadius: BorderRadius.circular(5.r)),
          child: Center(
            child: title != null
                ? Text(
                    title!,
                    style: GoogleFonts.inter(
                      color: AppColors.black,
                      fontWeight: FontWeight.w600,
                      fontSize: 26.sp,
                    ),
                  )
                : iconData != null
                    ? Icon(
                        iconData!,
                        size: 26.r,
                      )
                    : const Placeholder(),
          ),
        ),
      ),
    );
  }
}
