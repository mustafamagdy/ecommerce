import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../../theme/app_colors.dart';

class PinContainer extends StatelessWidget {
  final bool isActive;
  const PinContainer({Key? key, required this.isActive}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 24.r,
      width: 24.r,
      margin: EdgeInsets.only(right: 12.w),
      decoration: BoxDecoration(
          shape: BoxShape.circle,
          color: isActive ? AppColors.brandColor: AppColors.transparent,
        border: Border.all(color: isActive? AppColors.transparent: AppColors.outlineButtonBorder)
      ),
    );
  }
}
