import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../../core/utils/utils.dart';
import '../../../../../theme/theme.dart';

class TableFormField extends StatelessWidget {
  final String prefixSvg;
  final IconData? prefixIcon;
  final String? hintText;
  final bool readOnly;
  final String? Function(String?)? validator;
  final Function(String)? onChanged;
  final VoidCallback? onTap;
  final TextInputType? inputType;
  final TextEditingController? textEditingController;

  const TableFormField(
      {Key? key,
      required this.prefixSvg,
      this.validator,
      this.onChanged,
      this.inputType,
      this.hintText,
      this.textEditingController,
      this.readOnly = false,
      this.prefixIcon,
        this.onTap})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      onTap: onTap,
      controller: textEditingController,
      validator: validator,
      onChanged: onChanged,
      readOnly: readOnly,
      keyboardType: inputType,
      style: GoogleFonts.inter(
          color: AppColors.black, fontWeight: FontWeight.w500, fontSize: 14.sp),
      decoration: InputDecoration(
        hintText: AppHelpers.getTranslation(hintText ?? ""),
        hintStyle: GoogleFonts.inter(
          color: AppColors.hintColor,
          fontWeight: FontWeight.w500,
          fontSize: 13.sp,
        ),
        contentPadding: REdgeInsets.symmetric(vertical: 12, horizontal: 12),
        prefixIcon: Padding(
          padding: REdgeInsets.symmetric(vertical: 16),
          child: prefixIcon == null
              ? SvgPicture.asset(
                  prefixSvg,
                  height: 12.r,
                  width: 21.r,
                  color: AppColors.black,
                )
              : Icon(prefixIcon,color: AppColors.black,),
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
