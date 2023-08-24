import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/models/response/income_statistic_response.dart';
import 'package:fl_chart/fl_chart.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../../core/constants/constants.dart';
import '../../../../../theme/app_colors.dart';

class PieChartPage extends StatefulWidget {
  final IncomeStatisticResponse statistic;

  const PieChartPage({super.key, required this.statistic});

  @override
  State<PieChartPage> createState() => _PieChartState();
}

class _PieChartState extends State<PieChartPage> {
  int touchedIndex = -1;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: MediaQuery.of(context).size.width / 3,
      height: 360.r,
      padding: EdgeInsets.symmetric(horizontal: 20.r, vertical: 30.r),
      decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(10.r), color: AppColors.white),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            AppHelpers.getTranslation(TrKeys.statistics),
            style:
                GoogleFonts.inter(fontSize: 22.sp, fontWeight: FontWeight.w600),
          ),
          16.verticalSpace,
          Expanded(
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Expanded(
                  child: widget.statistic.group?.active?.percent == 0 &&
                          widget.statistic.group?.completed?.percent == 0 &&
                          widget.statistic.group?.ended?.percent == 0
                      ? Center(
                          child: Text(
                            AppHelpers.getTranslation(TrKeys.needOrder),
                            style: GoogleFonts.inter(
                                fontSize: 22.sp, fontWeight: FontWeight.w600),
                          ),
                        )
                      : PieChart(
                          PieChartData(
                            pieTouchData: PieTouchData(
                              touchCallback:
                                  (FlTouchEvent event, pieTouchResponse) {
                                setState(() {
                                  if (!event.isInterestedForInteractions ||
                                      pieTouchResponse == null ||
                                      pieTouchResponse.touchedSection == null) {
                                    touchedIndex = -1;
                                    return;
                                  }
                                  touchedIndex = pieTouchResponse
                                      .touchedSection!.touchedSectionIndex;
                                });
                              },
                            ),
                            borderData: FlBorderData(
                              show: false,
                            ),
                            sectionsSpace: 2,
                            centerSpaceRadius: 64,
                            sections: showingSections(widget.statistic),
                          ),
                        ),
                ),
                Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Container(
                          padding: EdgeInsets.all(4.r),
                          decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              border: Border.all(
                                  color: AppColors.brandColor, width: 3.r)),
                        ),
                        8.horizontalSpace,
                        Text(
                          AppHelpers.getTranslation(TrKeys.active),
                          style: GoogleFonts.inter(fontSize: 14.sp),
                        ),
                      ],
                    ),
                    10.verticalSpace,
                    Row(
                      children: [
                        Container(
                          padding: EdgeInsets.all(4.r),
                          decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              border: Border.all(
                                  color: AppColors.starColor, width: 3.r)),
                        ),
                        8.horizontalSpace,
                        Text(
                          AppHelpers.getTranslation(TrKeys.completed),
                          style: GoogleFonts.inter(fontSize: 14.sp),
                        ),
                      ],
                    ),
                    10.verticalSpace,
                    Row(
                      children: [
                        Container(
                          padding: EdgeInsets.all(4.r),
                          decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              border:
                                  Border.all(color: AppColors.red, width: 3.r)),
                        ),
                        8.horizontalSpace,
                        Text(
                          AppHelpers.getTranslation(TrKeys.ended),
                          style: GoogleFonts.inter(fontSize: 14.sp),
                        ),
                      ],
                    ),
                  ],
                ),
              ],
            ),
          )
        ],
      ),
    );
  }

  List<PieChartSectionData> showingSections(IncomeStatisticResponse statistic) {
    return List.generate(3, (i) {
      final isTouched = i == touchedIndex;
      final fontSize = isTouched ? 24.r : 16.r;
      final radius = isTouched ? 60.r : 50.r;
      switch (i) {
        case 0:
          return PieChartSectionData(
            color: AppColors.brandColor,
            value: statistic.group?.active?.percent?.toDouble(),
            title: '${statistic.group?.active?.percent?.floor()}%',
            radius: radius,
            titleStyle: TextStyle(
              fontSize: fontSize,
              fontWeight: FontWeight.bold,
              color: AppColors.black,
            ),
          );
        case 1:
          return PieChartSectionData(
            color: AppColors.starColor,
            value: statistic.group?.completed?.percent?.toDouble(),
            title: '${statistic.group?.completed?.percent?.floor()}%',
            radius: radius,
            titleStyle: TextStyle(
              fontSize: fontSize,
              fontWeight: FontWeight.bold,
              color: AppColors.black,
            ),
          );
        case 2:
          return PieChartSectionData(
            color: AppColors.red,
            value: statistic.group?.ended?.percent?.toDouble(),
            title: '${statistic.group?.ended?.percent?.floor()}%',
            radius: radius,
            titleStyle: TextStyle(
              fontSize: fontSize,
              fontWeight: FontWeight.bold,
              color: AppColors.white,
            ),
          );
        default:
          throw Error();
      }
    });
  }
}
