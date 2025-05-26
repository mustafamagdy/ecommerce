import 'package:admin_desktop/src/presentation/components/components.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../components/common_image.dart';
import '../../../../../theme/app_colors.dart';

class CustomersList extends StatelessWidget {
  final String? imageUrl;
  final String? name;
  final String? lastname;
  final String? gmail;
  final String? date;

  const CustomersList({
    super.key,
    this.imageUrl,
    this.name,
    this.gmail,
    this.date, this.lastname,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        14.verticalSpace,
        Row(
          children: [
            Expanded(
              child: Divider(
                color: AppColors.black.withOpacity(0.2),
              ),
            )
          ],
        ),
        14.verticalSpace,
        Row(
          children: [
            CommonImage(
              imageUrl: imageUrl,
              radius: 25,
              height: 50,
              width: 50,
            ),
            14.horizontalSpace,
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  '${name ?? ''} ${lastname ?? ''}',
                  style: GoogleFonts.inter(
                      fontSize: 16.sp,
                      color: AppColors.black,
                      fontWeight: FontWeight.w600),
                ),
                4.verticalSpace,
                Text(
                  gmail ?? '',
                  style: GoogleFonts.inter(
                      fontSize: 12.sp,
                      color: AppColors.iconColor,
                      fontWeight: FontWeight.w400),
                )
              ],
            ),
            const Spacer(),
            Text(
              date ?? '',
              style: GoogleFonts.inter(
                  fontSize: 12.sp,
                  color: AppColors.iconColor,
                  fontWeight: FontWeight.w400),
            ),
          ],
        ),
      ],
    );
  }
}
