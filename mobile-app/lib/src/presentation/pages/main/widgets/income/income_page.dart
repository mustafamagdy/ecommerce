import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/core/utils/local_storage.dart';
import 'package:admin_desktop/src/models/response/income_statistic_response.dart';
import 'package:admin_desktop/src/presentation/components/buttons/animation_button_effect.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/income/riverpod/income_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/income/riverpod/income_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/income/riverpod/income_state.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/income/widgets/statistics_page.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import '../../../../../../generated/assets.dart';
import 'widgets/chart_page.dart';
import '../../../../components/filter_screen.dart';
import 'widgets/pie_chart.dart';

class InComePage extends ConsumerStatefulWidget {
  const InComePage({Key? key}) : super(key: key);

  @override
  ConsumerState<InComePage> createState() => _InComePageState();
}

class _InComePageState extends ConsumerState<InComePage> {
  List list = [
    TrKeys.day,
    TrKeys.week,
    TrKeys.month,
  ];

  @override
  void initState() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(incomeProvider.notifier)
        ..fetchIncomeCarts()
        ..fetchIncomeCharts()
        ..fetchIncomeStatistic();
    });
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(incomeProvider);
    final event = ref.read(incomeProvider.notifier);
    return SingleChildScrollView(
      child: Padding(
        padding: EdgeInsets.symmetric(vertical: 20.r, horizontal: 16.r),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              AppHelpers.getTranslation(TrKeys.income),
              style: GoogleFonts.inter(
                  fontSize: 22.r, fontWeight: FontWeight.w600),
            ),
            16.verticalSpace,
            _filter(state, event),
            16.verticalSpace,
            _carts(state),
            16.verticalSpace,
            ChartPage(
              isDay: state.selectType == TrKeys.day,
              price: state.prices,
              chart: state.incomeCharts ?? [],
              times: state.time,
            ),
            16.verticalSpace,
            Row(
              children: [
                PieChartPage(
                  statistic: state.incomeStatistic ?? IncomeStatisticResponse(),
                ),
                16.horizontalSpace,
                StatisticPage(statistic: state.incomeStatistic)
              ],
            )
          ],
        ),
      ),
    );
  }

  Widget _carts(IncomeState state) {
    return Row(
      children: [
        Expanded(
          child: Container(
            padding: EdgeInsets.symmetric(horizontal: 20.r, vertical: 30.r),
            decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(10.r),
                color: AppColors.white),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      AppHelpers.getTranslation(TrKeys.revenue),
                      style: GoogleFonts.inter(
                          fontSize: 22.sp, fontWeight: FontWeight.w600),
                    ),
                    20.verticalSpace,
                    Row(
                      children: [
                        Container(
                          decoration: const BoxDecoration(
                            shape: BoxShape.circle,
                            color: AppColors.revenueColor,
                          ),
                          padding: EdgeInsets.all(10.r),
                          child: SvgPicture.asset(Assets.svgRevenue),
                        ),
                        24.horizontalSpace,
                        Text(
                          NumberFormat.currency(
                            symbol: LocalStorage.instance
                                .getSelectedCurrency()
                                .symbol,
                          ).format(state.incomeCart?.revenue ?? 0),
                          style: GoogleFonts.inter(
                              fontSize: 24.sp, fontWeight: FontWeight.w600),
                        ),
                      ],
                    ),
                  ],
                ),
                const Spacer(),
                Row(
                  children: [
                    state.incomeCart?.revenueType == TrKeys.plus
                        ? Icon(
                            FlutterRemix.arrow_up_line,
                            color: AppColors.brandColor,
                            size: 18.r,
                          )
                        : Icon(
                            FlutterRemix.arrow_down_line,
                            color: AppColors.red,
                            size: 18.r,
                          ),
                    4.horizontalSpace,
                    Text(
                      "${state.incomeCart?.revenuePercent?.ceil() ?? 0}%",
                      style: GoogleFonts.inter(
                          fontSize: 16.sp,
                          color: state.incomeCart?.revenueType == TrKeys.plus
                              ? AppColors.brandColor
                              : AppColors.red),
                    ),
                  ],
                )
              ],
            ),
          ),
        ),
        12.horizontalSpace,
        Expanded(
          child: Container(
            padding: EdgeInsets.symmetric(horizontal: 20.r, vertical: 30.r),
            decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(10.r),
                color: AppColors.white),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      AppHelpers.getTranslation(TrKeys.orders),
                      style: GoogleFonts.inter(
                          fontSize: 22.sp, fontWeight: FontWeight.w600),
                    ),
                    20.verticalSpace,
                    Row(
                      children: [
                        Container(
                          decoration: const BoxDecoration(
                            shape: BoxShape.circle,
                            color: AppColors.brandColor,
                          ),
                          padding: EdgeInsets.all(10.r),
                          child: const Icon(
                            FlutterRemix.shopping_cart_fill,
                            color: AppColors.white,
                          ),
                        ),
                        24.horizontalSpace,
                        Text(
                          NumberFormat.currency(
                            symbol: LocalStorage.instance
                                .getSelectedCurrency()
                                .symbol,
                          ).format(state.incomeCart?.orders ?? 0),
                          style: GoogleFonts.inter(
                              fontSize: 24.sp, fontWeight: FontWeight.w600),
                        ),
                      ],
                    ),
                  ],
                ),
                const Spacer(),
                Row(
                  children: [
                    state.incomeCart?.ordersType == TrKeys.plus
                        ? Icon(
                            FlutterRemix.arrow_up_line,
                            color: AppColors.brandColor,
                            size: 18.r,
                          )
                        : Icon(
                            FlutterRemix.arrow_down_line,
                            color: AppColors.red,
                            size: 18.r,
                          ),
                    4.horizontalSpace,
                    Text(
                      "${state.incomeCart?.ordersPercent?.ceil() ?? 0}%",
                      style: GoogleFonts.inter(
                          fontSize: 16.sp,
                          color: state.incomeCart?.ordersType == TrKeys.plus
                              ? AppColors.brandColor
                              : AppColors.red),
                    ),
                  ],
                )
              ],
            ),
          ),
        ),
        12.horizontalSpace,
        Expanded(
          child: Container(
            padding: EdgeInsets.symmetric(horizontal: 20.r, vertical: 30.r),
            decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(10.r),
                color: AppColors.white),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      AppHelpers.getTranslation(TrKeys.average),
                      style: GoogleFonts.inter(
                          fontSize: 22.sp, fontWeight: FontWeight.w600),
                    ),
                    20.verticalSpace,
                    Row(
                      children: [
                        Container(
                          decoration: const BoxDecoration(
                            shape: BoxShape.circle,
                            color: AppColors.black,
                          ),
                          padding: EdgeInsets.all(10.r),
                          child: SvgPicture.asset(Assets.svgAverage),
                        ),
                        24.horizontalSpace,
                        Text(
                          NumberFormat.currency(
                            symbol: LocalStorage.instance
                                .getSelectedCurrency()
                                .symbol,
                          ).format(state.incomeCart?.average ?? 0),
                          style: GoogleFonts.inter(
                              fontSize: 24.sp, fontWeight: FontWeight.w600),
                        ),
                      ],
                    ),
                  ],
                ),
                const Spacer(),
                Row(
                  children: [
                    state.incomeCart?.averageType == TrKeys.plus
                        ? Icon(
                            FlutterRemix.arrow_up_line,
                            color: AppColors.brandColor,
                            size: 18.r,
                          )
                        : Icon(
                            FlutterRemix.arrow_down_line,
                            color: AppColors.red,
                            size: 18.r,
                          ),
                    4.horizontalSpace,
                    Text(
                      "${state.incomeCart?.averagePercent?.ceil() ?? 0}%",
                      style: GoogleFonts.inter(
                          fontSize: 16.sp,
                          color: state.incomeCart?.averageType == TrKeys.plus
                              ? AppColors.brandColor
                              : AppColors.red),
                    ),
                  ],
                )
              ],
            ),
          ),
        )
      ],
    );
  }

  Widget _filter(IncomeState state, IncomeNotifier event) {
    return Row(
      children: [
        SvgPicture.asset(Assets.svgMenu),
        8.horizontalSpace,
        ...list.map(
          (e) => GestureDetector(
            onTap: () => event.changeIndex(e),
            child: AnimationButtonEffect(
              child: Container(
                padding: EdgeInsets.symmetric(vertical: 12.r, horizontal: 18.r),
                margin: EdgeInsets.only(right: 8.r),
                decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(10.r),
                    color: state.selectType == e
                        ? AppColors.brandColor
                        : AppColors.white),
                child: Text(
                  AppHelpers.getTranslation(e),
                  style: GoogleFonts.inter(fontSize: 14.sp),
                ),
              ),
            ),
          ),
        ),
        const Spacer(),
        InkWell(
          onTap: () {
            event
              ..fetchIncomeCarts()
              ..fetchIncomeCharts()
              ..fetchIncomeStatistic();
          },
          child: AnimationButtonEffect(
            child: Container(
                decoration: BoxDecoration(
                    color: AppColors.white,
                    borderRadius: BorderRadius.circular(10.r)),
                padding: EdgeInsets.all(10.r),
                child: const Icon(FlutterRemix.restart_line)),
          ),
        ),
        8.horizontalSpace,
        InkWell(
          onTap: () {
            AppHelpers.showAlertDialog(
                context: context,
                child: SizedBox(
                    width: MediaQuery.of(context).size.width / 3,
                    child: const FilterScreen()));
          },
          child: AnimationButtonEffect(
            child: Container(
              padding: EdgeInsets.symmetric(vertical: 10.r, horizontal: 16.r),
              decoration: BoxDecoration(
                color: AppColors.white,
                borderRadius: BorderRadius.circular(10.r),
              ),
              child: Row(
                children: [
                  const Icon(FlutterRemix.calendar_check_line),
                  16.horizontalSpace,
                  Text(
                    state.start == null
                        ? AppHelpers.getTranslation(TrKeys.startEnd)
                        : "${DateFormat("MMM d,yyyy").format(state.start ?? DateTime.now())} - ${DateFormat("MMM d,yyyy").format(state.end ?? DateTime.now())}",
                    style: GoogleFonts.inter(fontSize: 14.sp),
                  )
                ],
              ),
            ),
          ),
        )
      ],
    );
  }
}
