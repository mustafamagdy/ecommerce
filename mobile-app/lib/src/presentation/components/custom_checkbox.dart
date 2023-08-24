import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

class CustomCheckbox extends StatelessWidget {
  final bool isActive;
  final VoidCallback onTap;

  const CustomCheckbox({Key? key, required this.isActive, required this.onTap})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        width: 26.r,
        height: 26.r,
        decoration: BoxDecoration(
          color: isActive ? AppColors.brandColor : AppColors.transparent,
          borderRadius: BorderRadius.circular(8.r),
          border: Border.all(
            width: 2,
            color: isActive
                ? AppColors.transparent
                : AppColors.unselectedBottomItem,
          ),
        ),
        duration: const Duration(milliseconds: 500),
        child: isActive
            ? Icon(
                FlutterRemix.check_fill,
                color: AppColors.black,
                size: 18.r,
              )
            : const SizedBox.shrink(),
      ),
    );
  }
}
