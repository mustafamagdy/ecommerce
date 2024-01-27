import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../../components/components.dart';
import '../../../../../../theme/app_colors.dart';

class CustomColumnWidget extends StatelessWidget {
  final String trName;
  final Function(String)? onChanged;
  final TextEditingController controller;
  final TextInputType? inputType;
  final bool? obscure;
  final Widget? suffixIcon;
  final Widget? prefixIcon;
  final int? maxLength;
  final bool? readOnly;

  const CustomColumnWidget({
    super.key,
    required this.trName,
    required this.controller,
    this.inputType,
    this.onChanged,
    this.obscure,
    this.suffixIcon,
    this.prefixIcon,
    this.maxLength,
    this.readOnly,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          trName,
          style: GoogleFonts.inter(
              fontSize: 12.sp,
              color: AppColors.iconColor,
              fontWeight: FontWeight.w500),
        ),
        4.verticalSpace,
        OutlinedBorderTextField(
          readOnly: readOnly ?? false,
          inputType: inputType,
          maxLength: maxLength,
          prefixIcon: prefixIcon,
          onChanged: onChanged,
          textController: controller,
          label: null,
          obscure: obscure,
          suffixIcon: suffixIcon,
        ),
      ],
    );
  }
}
