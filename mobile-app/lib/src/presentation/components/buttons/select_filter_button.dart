import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../theme/theme.dart';

class SelectFilterButton extends StatelessWidget {
  final String title;

  const SelectFilterButton({
    Key? key,
    required this.title,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: () {},
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            title,
            style: GoogleFonts.inter(
              fontSize: 14.sp,
              color: AppColors.black,
              fontWeight: FontWeight.w500,
              letterSpacing: -14 * 0.02,
            ),
          ),
          Icon(
            FlutterRemix.arrow_down_s_line,
            size: 20.r,
            color: AppColors.black,
          ),
        ],
      ),
    );
  }
}
