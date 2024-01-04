import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../theme/app_colors.dart';

class NotificationCountsContainer extends StatelessWidget {
  final String? count;
  const NotificationCountsContainer({super.key, this.count});

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 24.r,
      width: 38.r,
      decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(100),
          color: AppColors.brandColor),
      child: Center(
        child: Text(
          count ?? '',
          style: GoogleFonts.inter(
            fontWeight: FontWeight.w500,
            fontSize: 14.sp,
            color: AppColors.black,
          ),
        ),
      ),
    );
  }
}
