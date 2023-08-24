import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../theme/theme.dart';

class CustomDropDownField extends StatelessWidget {
  final String? value;
  final List list;
  final ValueChanged onChanged;

  const CustomDropDownField({
    Key? key,
    this.value,
    required this.list,
    required this.onChanged,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return DropdownButtonFormField(
      value: value,
      items: list.map((e) {
        return DropdownMenuItem(
            value: e, child: Text(AppHelpers.getTranslation(e)));
      }).toList(),
      onChanged: onChanged,
      dropdownColor: AppColors.white,
      iconEnabledColor: AppColors.black,
      borderRadius: BorderRadius.circular(10.r),
      style: GoogleFonts.inter(
          color: AppColors.black, fontWeight: FontWeight.w500, fontSize: 14.sp),
      icon: const Icon(FlutterRemix.arrow_down_s_line),
      decoration: InputDecoration(
        contentPadding: REdgeInsets.symmetric(vertical: 12,horizontal: 12),
        prefixIcon: Icon(
          FlutterRemix.building_line,
          size: 24.r,
          color: AppColors.black,
        ),
        filled: true,
        fillColor: AppColors.editProfileCircle,
        border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(5.r),
            borderSide: BorderSide.none),
        focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(5.r),
            borderSide: BorderSide.none),
        enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(5.r),
            borderSide: BorderSide.none),
      ),
    );
  }
}
