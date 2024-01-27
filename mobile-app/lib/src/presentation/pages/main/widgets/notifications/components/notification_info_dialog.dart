import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../components/common_image.dart';
import '../../../../../theme/app_colors.dart';

class NotificationInfoDialog extends StatelessWidget {
  final String? image;
  final String? name;
  final String? lastName;
  final String? info;
  final String? updatedAt;
  const NotificationInfoDialog(
      {super.key,
      this.image,
      this.name,
      this.lastName,
      this.info,
      this.updatedAt});

  @override
  Widget build(BuildContext context) {
    return Container(
        width: 300,
        decoration: BoxDecoration(borderRadius: BorderRadius.circular(10)),
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  CommonImage(
                    imageUrl: image,
                    height: 50,
                    width: 50,
                    radius: 100,
                  ),
                  15.horizontalSpace,
                  Text(
                    '$name $lastName',
                    style: GoogleFonts.inter(
                        fontWeight: FontWeight.w700,
                        fontSize: 16.sp,
                        color: AppColors.black),
                  ),
                ],
              ),
              16.verticalSpace,
              Text(
                info ?? '',
                style: GoogleFonts.inter(
                    fontWeight: FontWeight.w400,
                    fontSize: 14.sp,
                    color: AppColors.black),
              ),
              15.verticalSpace,
              Text(
                updatedAt ?? '0',
                style: GoogleFonts.inter(
                    fontWeight: FontWeight.w500,
                    fontSize: 12.sp,
                    color: AppColors.iconColor),
              ),
            ],
          ),
        ));
  }
}
