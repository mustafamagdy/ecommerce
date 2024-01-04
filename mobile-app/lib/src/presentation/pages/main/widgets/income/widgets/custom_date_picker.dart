import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:calendar_date_picker2/calendar_date_picker2.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';



class CustomDatePicker extends StatefulWidget {
  final List<DateTime?> range;
  final ValueChanged<List<DateTime?>> onChange;

  const CustomDatePicker(
      {Key? key, required this.range, required this.onChange})
      : super(key: key);

  @override
  State<CustomDatePicker> createState() => _CustomDatePickerState();
}

class _CustomDatePickerState extends State<CustomDatePicker> {
  final config = CalendarDatePicker2Config(
    calendarType: CalendarDatePicker2Type.range,
    selectedDayHighlightColor: AppColors.brandColor,
    weekdayLabelTextStyle: GoogleFonts.inter(
        fontSize: 14.sp, letterSpacing: -0.3, color: AppColors.black),
    controlsTextStyle: GoogleFonts.inter(
        fontSize: 14.sp, letterSpacing: -0.3, color: AppColors.black),
    dayTextStyle: GoogleFonts.inter(
        fontSize: 14.sp, letterSpacing: -0.3, color: AppColors.black),
    disabledDayTextStyle: GoogleFonts.inter(
        fontSize: 14.sp, letterSpacing: -0.3, color: AppColors.iconColor),
    dayBorderRadius: BorderRadius.circular(10.r),
  );

  @override
  Widget build(BuildContext context) {
    return CalendarDatePicker2(
        key: UniqueKey(),
        config: config,
        initialValue: widget.range,
        onValueChanged: widget.onChange);
  }
}
