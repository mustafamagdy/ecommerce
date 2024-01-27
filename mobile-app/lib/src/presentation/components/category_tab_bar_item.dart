import 'package:admin_desktop/src/presentation/components/buttons/animation_button_effect.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

class CategoryTabBarItem extends StatelessWidget {
  final String? title;
  final bool isActive;
  final Function() onTap;

  const CategoryTabBarItem({
    Key? key,
    this.title,
    required this.isActive,
    required this.onTap,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return AnimationButtonEffect(
      child: GestureDetector(
        onTap: isActive ? null : onTap,
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 300),
          height: 36.r,
          decoration: BoxDecoration(
            color: isActive ? AppColors.brandColor : AppColors.white,
            borderRadius: BorderRadius.circular(10.r),
            boxShadow: [
              BoxShadow(
                color: AppColors.white.withOpacity(0.07),
                spreadRadius: 0,
                blurRadius: 2,
                offset: const Offset(0, 1),
              ),
            ],
          ),
          alignment: Alignment.center,
          padding: REdgeInsets.symmetric(horizontal: 18),
          margin: REdgeInsets.only(right: 8),
          child: Row(
            children: [
              Text(
                '$title',
                style: GoogleFonts.inter(
                  fontSize: 13.sp,
                  color: AppColors.black,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
