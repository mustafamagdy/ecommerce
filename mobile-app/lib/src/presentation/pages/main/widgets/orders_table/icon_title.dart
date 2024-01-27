import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../theme/app_colors.dart';

class IconTitle extends StatelessWidget {
  final String title;
  final IconData icon;
  final String value;

  const IconTitle(
      {Key? key, required this.title, required this.icon, required this.value})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.only(bottom: 8.r),
      child: Row(
        children: [
          Icon(icon, size: 24.r),
          8.horizontalSpace,
          Expanded(
            child: Text(
              "$title: $value",
              style: GoogleFonts.inter(
                fontSize: 16.sp,
                color: AppColors.black,
              ),
              maxLines: 1,
            ),
          )
        ],
      ),
    );
  }
}
