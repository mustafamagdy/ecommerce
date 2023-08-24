import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/models/response/income_chart_response.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:fl_chart/fl_chart.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import '../../../../../../core/utils/local_storage.dart';

class ChartPage extends StatefulWidget {
  final List<num> price;
  final List<DateTime> times;
  final List<IncomeChartResponse> chart;
  final bool isDay;

  const ChartPage(
      {super.key,
      required this.price,
      required this.chart,
      required this.times,
      required this.isDay});

  @override
  State<ChartPage> createState() => _ChartPageState();
}

class _ChartPageState extends State<ChartPage> {
  List<Color> gradientColors = [
    AppColors.brandColor.withOpacity(0.5),
    AppColors.transparent,
  ];

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      height: 326.h,
      padding: EdgeInsets.symmetric(horizontal: 20.r, vertical: 30.r),
      decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(10.r), color: AppColors.white),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            AppHelpers.getTranslation(TrKeys.saleChart),
            style:
                GoogleFonts.inter(fontSize: 22.sp, fontWeight: FontWeight.w600),
          ),
          24.verticalSpace,
          Expanded(
            child: widget.chart.isNotEmpty
                ? LineChart(
                    mainData(),
                  )
                : Center(
                  child: Text(
                      AppHelpers.getTranslation(TrKeys.needOrder),
                      style: GoogleFonts.inter(
                          fontSize: 22.sp, fontWeight: FontWeight.w600),
                    ),
                ),
          ),
        ],
      ),
    );
  }

  Widget bottomTitleWidgets(double value, TitleMeta meta) {
    final style = GoogleFonts.inter(
      fontSize: 10.sp,
    );

    return SideTitleWidget(
      axisSide: meta.axisSide,
      child: Text(
          DateFormat(widget.isDay ? "HH:00" : "MMM d")
              .format(widget.times[value.ceil()]),
          style: style),
    );
  }

  Widget leftTitleWidgets(double value, TitleMeta meta) {
    final style = GoogleFonts.inter(
      fontSize: 12.sp,
    );
    return Text(
        NumberFormat.currency(
          decimalDigits: 0,
          symbol: LocalStorage.instance.getSelectedCurrency().symbol,
        ).format(widget.price[value.toInt()]),
        style: style,
        textAlign: TextAlign.left);
  }

  LineChartData mainData() {
    return LineChartData(
      gridData: FlGridData(
        show: true,
        drawVerticalLine: false,
        horizontalInterval: 1,
        verticalInterval: 1,
        getDrawingHorizontalLine: (value) {
          return FlLine(
              color: AppColors.iconButtonBack, strokeWidth: 1, dashArray: [10]);
        },
      ),
      titlesData: FlTitlesData(
        show: true,
        rightTitles: AxisTitles(
          sideTitles: SideTitles(showTitles: false),
        ),
        topTitles: AxisTitles(
          sideTitles: SideTitles(showTitles: false),
        ),
        bottomTitles: AxisTitles(
          sideTitles: SideTitles(
            showTitles: true,
            reservedSize: 30.r,
            interval: 1,
            getTitlesWidget: bottomTitleWidgets,
          ),
        ),
        leftTitles: AxisTitles(
          sideTitles: SideTitles(
            showTitles: true,
            interval: 1,
            getTitlesWidget: leftTitleWidgets,
            reservedSize: 80.r,
          ),
        ),
      ),
      borderData: FlBorderData(
        show: false,
      ),
      minX: 0,
      maxX: widget.times.length.toDouble() - 1,
      minY: 0,
      maxY: 6,
      lineBarsData: [
        LineChartBarData(
          spots: [
            for (int index = 0; index < widget.times.length; index++)
              FlSpot(
                  index.toDouble(),
                  widget.price.findPriceIndex(widget.isDay
                      ? widget.chart.findPriceWithHour(widget.times[index])
                      : widget.chart.findPrice(widget.times[index]))),
          ],
          isCurved: true,
          color: AppColors.brandColor,
          barWidth: 5,
          isStrokeCapRound: true,
          dotData: FlDotData(
            show: false,
          ),
          belowBarData: BarAreaData(
            show: true,
            gradient: LinearGradient(
              begin: Alignment.topCenter,
              end: Alignment.bottomCenter,
              colors: gradientColors,
            ),
          ),
        ),
      ],
    );
  }
}
