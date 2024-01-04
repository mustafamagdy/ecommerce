import 'package:admin_desktop/src/models/response/income_cart_response.dart';
import 'package:admin_desktop/src/models/response/income_chart_response.dart';
import 'package:admin_desktop/src/models/response/income_statistic_response.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../../../../core/constants/constants.dart';

part 'income_state.freezed.dart';

@freezed
class IncomeState with _$IncomeState {
  const factory IncomeState({
    @Default(true) bool isLoading,
    @Default(TrKeys.week) String selectType,
    @Default(null) IncomeCartResponse? incomeCart,
    @Default(null) IncomeStatisticResponse? incomeStatistic,
    @Default([]) List<IncomeChartResponse>? incomeCharts,
    @Default([]) List<num> prices,
    @Default([]) List<DateTime> time,
    @Default(null) DateTime? start,
    @Default(null) DateTime? end,
  }) = _IncomeState;

  const IncomeState._();
}
