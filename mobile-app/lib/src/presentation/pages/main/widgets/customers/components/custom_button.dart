import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../components/buttons/animation_button_effect.dart';
import '../../../../../theme/app_colors.dart';

class CustomButton extends StatelessWidget {
  final Color? borderColor;
  final Color? textColor;
  final Color? containerColor;
  final String? title;
  final Function()? onTap;
  final bool? isLoading;
  final Widget? loadingwidget;
  const CustomButton(
      {super.key,
      this.borderColor,
      this.title,
      this.textColor,
      this.containerColor,
      this.onTap,
      this.isLoading,
      this.loadingwidget});

  @override
  Widget build(BuildContext context) {
    return AnimationButtonEffect(
      child: GestureDetector(
        onTap: onTap,
        child: Container(
          height: 36.h,
          width: 122.w,
          decoration: BoxDecoration(
              color: containerColor,
              borderRadius: BorderRadius.circular(5),
              border: Border.all(color: borderColor ?? AppColors.transparent)),
          child: Center(
            child: isLoading ?? false
                ? loadingwidget
                : Text(
                    title ?? '',
                    style: GoogleFonts.inter(
                        fontSize: 18.sp,
                        color: textColor,
                        fontWeight: FontWeight.w500),
                  ),
          ),
        ),
      ),
    );
  }
}
