import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../core/constants/constants.dart';
import '../../../core/utils/utils.dart';
import '../../theme/theme.dart';

class IncreaseDecreaseButton extends StatelessWidget {
  final int count;
  final Function() onAdd;
  final Function() onSubtract;

  const IncreaseDecreaseButton({
    Key? key,
    required this.count,
    required this.onAdd,
    required this.onSubtract,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 50.r,
      decoration: BoxDecoration(
        border: Border.all(width: 1.r, color: AppColors.black),
        borderRadius: BorderRadius.circular(8.r),
        color: AppColors.white,
      ),
      alignment: Alignment.center,
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          IconButton(
            onPressed: onSubtract,
            splashRadius: 20.r,
            icon: Icon(
              FlutterRemix.subtract_line,
              size: 22.r,
              color: AppColors.black,
            ),
          ),
          Text(
            '$count ${AppHelpers.getTranslation(TrKeys.itemsInCart).toLowerCase()}',
            style: GoogleFonts.k2d(
              fontWeight: FontWeight.w500,
              fontSize: 14.sp,
              color: AppColors.black,
              letterSpacing: -14 * 0.01,
            ),
          ),
          IconButton(
            onPressed: onAdd,
            splashRadius: 20.r,
            icon: Icon(
              FlutterRemix.add_line,
              size: 22.r,
              color: AppColors.black,
            ),
          ),
        ],
      ),
    );
  }
}
