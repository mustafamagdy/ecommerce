import 'package:admin_desktop/src/models/data/table_model.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../../../../core/constants/constants.dart';

part 'tables_state.freezed.dart';

@freezed
class TablesState with _$TablesState {
  const factory TablesState({
    @Default(false) bool isLoading,
    @Default(true) bool hasMore,
    @Default(0) int selectTabIndex,
    @Default(0) int selectFloor,
    @Default([TrKeys.oneFloor,TrKeys.twoFloor]) List<String> listFloor,
    @Default([]) List<TableModel?> tableList,
    @Default("") String addFloor,
  }) = _TablesState;

  const TablesState._();
}
