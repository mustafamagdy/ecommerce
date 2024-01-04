import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/presentation/components/login_button.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/income/riverpod/income_provider.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../../core/constants/constants.dart';
import 'custom_date_picker.dart';

class FilterScreen extends StatefulWidget {
  final DateTime? start;
  final DateTime? end;

  const FilterScreen({Key? key, this.start, this.end}) : super(key: key);

  @override
  State<FilterScreen> createState() => _FilterScreenState();
}

class _FilterScreenState extends State<FilterScreen> {
  List<DateTime?> _rangeDatePicker = [];
  List<DateTime?> _newList = [];

  @override
  void initState() {
    _rangeDatePicker = [
      widget.start ?? DateTime.now(),
      widget.end ?? DateTime.now(),
    ];

    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      mainAxisSize: MainAxisSize.min,
      children: [
        Padding(
          padding: EdgeInsets.symmetric(horizontal: 16.w),
          child: Text(
            AppHelpers.getTranslation(TrKeys.selectDesiredOrderHistory),
            style: GoogleFonts.inter(
                fontSize: 14.sp, color: AppColors.black, letterSpacing: -0.3),
          ),
        ),
        CustomDatePicker(
          range: _rangeDatePicker,
          onChange: (n) {
            _newList = n;
          },
        ),
        16.verticalSpace,
        SizedBox(
          width: 200.w,
          child: Padding(
            padding: EdgeInsets.only(left: 16.r),
            child: Consumer(builder: (context, ref, child) {
              return LoginButton(
                title: AppHelpers.getTranslation(TrKeys.save),
                onPressed: () {
                  ref.read(incomeProvider.notifier)
                    ..fetchIncomeCarts(
                        start: _newList.first, end: _newList.last)
                    ..fetchIncomeCharts(
                        start: _newList.first, end: _newList.last)
                    ..fetchIncomeStatistic(
                        start: _newList.first, end: _newList.last);
                  context.popRoute();
                },
              );
            }),
          ),
        ),
        8.verticalSpace,
      ],
    );
  }
}
