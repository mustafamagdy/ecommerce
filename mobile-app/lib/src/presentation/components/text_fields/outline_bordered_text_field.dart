import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../theme/theme.dart';

class OutlinedBorderTextField extends StatelessWidget {
  final String? label;
  final Widget? suffixIcon;
  final Widget? prefixIcon;
  final bool? obscure;
  final TextEditingController? textController;
  final Function(String)? onChanged;
  final VoidCallback? onTap;
  final Function(String)? onFieldSubmitted;
  final TextInputType? inputType;
  final String? initialText;
  final String? descriptionText;
  final bool readOnly;
  final bool isError;
  final bool isSuccess;
  final TextCapitalization? textCapitalization;
  final double? verticalPadding;
  final double? labelSize;
  final int? maxLength;
  final bool? isDate;
  final Color? color;
  final Color? border;
  final TextStyle? style;
  final String? Function(String?)? validator;
  const OutlinedBorderTextField({
    Key? key,
    required this.label,
    this.suffixIcon,
    this.prefixIcon,
    this.obscure,
    this.onChanged,
    this.textController,
    this.inputType,
    this.initialText,
    this.descriptionText,
    this.readOnly = false,
    this.isError = false,
    this.isSuccess = false,
    this.textCapitalization,
    this.onFieldSubmitted,
    this.onTap,
    this.verticalPadding,
    this.labelSize,
    this.maxLength,
    this.isDate,
    this.color,
    this.style,
    this.border,
    this.validator,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
      if(label != null)
        Column(
          children: [
            Text(label ?? "",),
            4.verticalSpace,
          ],
        ),
        Container(
          decoration: BoxDecoration(
            color: AppColors.white,
            borderRadius: BorderRadius.circular(8.r),
            border: Border.all(
              width: 1.r,
              style: BorderStyle.solid,
              color: isError
                  ? AppColors.red
                  : isSuccess
                      ? AppColors.brandColor
                      : border ?? AppColors.borderColor,
            ),
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(8.r),
            child: TextFormField(
              validator: validator,
              maxLength: maxLength,
              onTap: onTap,
              onFieldSubmitted: onFieldSubmitted,
              onChanged: onChanged,
              obscureText: !(obscure ?? true),
              obscuringCharacter: '*',
              controller: textController,
              style: GoogleFonts.inter(
                fontWeight: FontWeight.w500,
                fontSize: labelSize ?? 16.sp,
                color: AppColors.black,
                letterSpacing: -14 * 0.01,
              ),
              cursorWidth: 1,
              cursorColor: AppColors.black,
              keyboardType: inputType,
              initialValue: initialText,
              readOnly: readOnly,
              textCapitalization:
                  textCapitalization ?? TextCapitalization.sentences,
              decoration: InputDecoration(
                counterText: '',
                suffixIcon: suffixIcon,
                suffixIconConstraints:
                    BoxConstraints(maxWidth: 48.r, minWidth: 48.r),
                labelStyle: style,
                prefix: prefixIcon,

                contentPadding: REdgeInsets.symmetric(
                    horizontal: 16, vertical: verticalPadding ?? 10),
                floatingLabelStyle: GoogleFonts.inter(
                  fontWeight: FontWeight.w400,
                  fontSize: labelSize ?? 14.sp,
                  color: AppColors.black,
                  letterSpacing: -14 * 0.01,
                ),
                fillColor: color ?? AppColors.white,
                filled: true,
                hoverColor: AppColors.transparent,
                enabledBorder: InputBorder.none,
                errorBorder: InputBorder.none,
                border: InputBorder.none,
                focusedErrorBorder: InputBorder.none,
                disabledBorder: InputBorder.none,
                focusedBorder: InputBorder.none,
              ),
            ),
          ),
        ),
        if (descriptionText != null)
          Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              4.verticalSpace,
              Text(
                descriptionText!,
                style: GoogleFonts.inter(
                  fontWeight: FontWeight.w400,
                  letterSpacing: -0.3,
                  fontSize: 12.sp,
                  color: isError
                      ? AppColors.red
                      : isSuccess
                          ? AppColors.brandColor
                          : AppColors.black,
                ),
              ),
            ],
          )
      ],
    );
  }
}
