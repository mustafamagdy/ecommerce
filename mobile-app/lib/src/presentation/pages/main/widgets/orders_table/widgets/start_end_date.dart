import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/utils.dart';
import 'package:admin_desktop/src/presentation/components/buttons/animation_button_effect.dart';
import 'package:admin_desktop/src/presentation/theme/theme.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import '../../../../../components/filter_screen.dart';

class StartEndDate extends StatelessWidget {
  final DateTime? start;
  final DateTime? end;

  const StartEndDate({Key? key,  this.start,  this.end}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: () {
        AppHelpers.showAlertDialog(
            context: context,
            child: SizedBox(
                width: MediaQuery
                    .of(context)
                    .size
                    .width / 3,
                child: const FilterScreen(isOrder: true,)));
      },
      child: AnimationButtonEffect(
        child: Container(
          padding: EdgeInsets.symmetric(vertical: 10.r, horizontal: 16.r),
          decoration: BoxDecoration(
            color: AppColors.white,
            borderRadius: BorderRadius.circular(10.r),
            border: Border.all(
              color: AppColors.unselectedBottomBarBack,
              width: 1.r,
            ),

          ),
          child: Row(
              children: [
              const Icon(FlutterRemix.calendar_check_line),
          16.horizontalSpace,
          Text(start == null
              ? AppHelpers.getTranslation(TrKeys.startEnd)
              : "${DateFormat("MMM d,yyyy").format(
              start ?? DateTime.now())} - ${DateFormat("MMM d,yyyy")
              .format(end ?? DateTime.now())}",
              style: GoogleFonts.inter(fontSize: 14.sp),
        )
        ],
      ),
    ),)
    ,
    );
  }
}
