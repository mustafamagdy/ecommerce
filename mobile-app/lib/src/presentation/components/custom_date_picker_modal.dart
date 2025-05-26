import 'package:auto_route/auto_route.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../core/constants/constants.dart';
import '../../core/utils/utils.dart';
import '../theme/theme.dart';
import 'buttons/confirm_button.dart';

class CustomDatePickerModal extends StatefulWidget {
  final Function(DateTime? date) onDateSaved;

  const CustomDatePickerModal({
    Key? key,
    required this.onDateSaved,
  }) : super(key: key);

  @override
  State<CustomDatePickerModal> createState() => _CustomDatePickerModalState();
}

class _CustomDatePickerModalState extends State<CustomDatePickerModal> {
  DateTime? date;

  @override
  void initState() {
    super.initState();
    date = DateTime.now();
  }

  @override
  Widget build(BuildContext context) {
    return Material(
      borderRadius: BorderRadius.circular(10.r),
      color: AppColors.white,
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
                  mode: CupertinoDatePickerMode.date,
                  initialDateTime: date,
                  minimumDate: date,
                  onDateTimeChanged: (DateTime value) {
                    date = value;
                  },
                ),
              ),
            ),
            16.verticalSpace,
            ConfirmButton(
              title: AppHelpers.getTranslation(TrKeys.save),
              onTap: () {
                widget.onDateSaved(date);
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
