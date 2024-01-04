import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

class StatusItemPage extends StatelessWidget {
  final String title;
  final String index;
  final bool isDivider;
  final bool isActive;
  final bool isOldStatus;

  const StatusItemPage(
      {Key? key,
      required this.title,
      this.isDivider = true,
      this.isActive = false,
      this.isOldStatus = false,
      required this.index})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          decoration: BoxDecoration(
            color: isActive ? AppColors.brandColor : AppColors.white,
            shape: BoxShape.circle,
            border: Border.all(
                color: isActive || isOldStatus
                    ? AppColors.brandColor
                    : AppColors.borderColor),
          ),
          padding: EdgeInsets.all(8.r),
          child: isOldStatus
              ? const Icon(
                  FlutterRemix.check_line,
                  color: AppColors.brandColor,
                )
              : Padding(
                  padding: EdgeInsets.all(8.r),
                  child: Text(
                    index,
                    style: TextStyle(
                        color: isActive ? AppColors.white : AppColors.black),
                  ),
                ),
        ),
        8.horizontalSpace,
        Text(
          title,
          style: GoogleFonts.inter(
              fontSize: 20.sp,
              fontWeight: isActive ? FontWeight.w600 : FontWeight.normal),
        ),
        8.horizontalSpace,
        isDivider
            ? Expanded(
                child: Divider(
                  color: isOldStatus ? AppColors.brandColor : AppColors.borderColor,
                ),
              )
            : const SizedBox.shrink(),
        8.horizontalSpace,
      ],
    );
  }
}
