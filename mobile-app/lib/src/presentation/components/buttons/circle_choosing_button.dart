import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../theme/app_colors.dart';

class CircleChoosingButton extends StatelessWidget {
  final bool isActive;
  const CircleChoosingButton({super.key, required this.isActive});

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 18.h,
      width: 18.w,
      decoration: BoxDecoration(
          color: isActive
              ? AppColors.brandColor
              : AppColors.transparent,
          shape: BoxShape.circle,
          border: Border.all(
              color: !isActive
                  ? AppColors.iconColor
                  : AppColors.transparent,
              width: 2)),
      child: Center(
        child: Container(
          height: 6.h,
          width: 6.w,
          decoration: BoxDecoration(
              color:isActive
                  ? AppColors.locationAddress
                  : AppColors.transparent,
              shape: BoxShape.circle),
        ),
      ),
    );
  }
}
