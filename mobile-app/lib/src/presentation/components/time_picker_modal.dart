import 'package:admin_desktop/src/presentation/components/buttons/confirm_button.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../core/constants/constants.dart';
import '../../core/utils/utils.dart';
import '../theme/theme.dart';

class TimePickerModal extends StatefulWidget {
  final Function(DateTime) onSaved;
  final String? openTime;
  final String? closeTime;

  const TimePickerModal({
    Key? key,
    required this.onSaved,
    this.openTime,
    this.closeTime,
  }) : super(key: key);

  @override
  State<TimePickerModal> createState() => _TimePickerModalState();
}

class _TimePickerModalState extends State<TimePickerModal> {
  DateTime time = DateTime.now();

  @override
  void initState() {
    super.initState();
    time = widget.openTime == null
        ? DateTime.now()
        : AppHelpers.getMinTime(widget.openTime!);
  }

  @override
  Widget build(BuildContext context) {
    return Material(
      color: AppColors.white,
      borderRadius: BorderRadius.circular(10.r),
      child: Padding(
        padding: REdgeInsets.symmetric(horizontal: 15),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            24.verticalSpace,
            SizedBox(
              height: 300.r,
              child: CupertinoTheme(
                data: const CupertinoThemeData(
                  brightness: Brightness.light,
                ),
                child: CupertinoDatePicker(
                  mode: CupertinoDatePickerMode.time,
                  minimumDate: widget.openTime == null
                      ? DateTime.now()
                      : AppHelpers.getMinTime(widget.openTime!),
                  maximumDate: widget.closeTime == null
                      ? null
                      : AppHelpers.getMaxTime(widget.closeTime!),
                  use24hFormat: true,
                  initialDateTime: widget.openTime == null
                      ? DateTime.now()
                      : AppHelpers.getMinTime(widget.openTime!),
                  onDateTimeChanged: (DateTime value) {
                    time = value;
                  },
                ),
              ),
            ),
            16.verticalSpace,
            ConfirmButton(
              title: AppHelpers.getTranslation(TrKeys.save),
              onTap: () {
                widget.onSaved(time);
                context.popRoute();
              },
            ),
            24.verticalSpace,
          ],
        ),
      ),
    );
  }
}
