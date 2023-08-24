import 'package:admin_desktop/generated/assets.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:lottie/lottie.dart';
import '../../../../../theme/app_colors.dart';

class BoardTopBar extends StatelessWidget {
  final String title;
  final String count;
  final VoidCallback onTap;
  final bool isLoading;
  final Color color;

  const BoardTopBar({
    Key? key,
    required this.title,
    required this.count,
    required this.onTap,
    required this.isLoading,
    required this.color,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 50.r,
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(10.r),
      ),
      padding: EdgeInsets.symmetric(horizontal: 16.r),
     margin: EdgeInsets.only(right: 6.r,left: 6.r,bottom: 8.r),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Text(
            title,
            style: GoogleFonts.inter(
              fontSize: 16.sp,
              fontWeight: FontWeight.w600,
              color: AppColors.black,
            ),
          ),
          12.horizontalSpace,
          Container(
            padding: EdgeInsets.symmetric(vertical: 6.r, horizontal: 16.r),
            decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(100.r), color: color),
            child: Text(
              count,
              style: GoogleFonts.inter(
                fontSize: 16.sp,
                fontWeight: FontWeight.w600,
                color: AppColors.white,
              ),
            ),
          ),
          const Spacer(),
          SizedBox(
            height: 48.r,
            width: 48.r,
            child: GestureDetector(
              onTap: onTap,
              child: isLoading
                  ? Lottie.asset(
                      Assets.lottieRefresh,
                      width: 32.r,
                      height: 32.r,
                      fit: BoxFit.fill,
                    )
                  : const Icon(FlutterRemix.refresh_line),
            ),
          ),
        ],
      ),
    );
  }
}
