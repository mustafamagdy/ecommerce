import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../../../models/models.dart';
import 'tables_state.dart';

class TablesNotifier extends StateNotifier<TablesState> {
  TablesNotifier() : super(const TablesState());

  changeActive(int index,TableModel? tableModel) {
    List<TableModel> list = List.from(state.tableList);
    list.removeAt(index);
    list.insert(
        index,
        TableModel(
            title: tableModel?.title ?? "",
            count: tableModel?.count ?? 0,
            tax: tableModel?.tax ?? 0,
            zona: tableModel?.zona ?? "",
            isActive: true));
    state = state.copyWith(tableList: list);
  }

  changeIndex(int index) {
    state = state.copyWith(selectTabIndex: index);
  }

  changeFloor(int index) {
    state = state.copyWith(selectFloor: index);
  }

  setFloor(String floor) {
    state = state.copyWith(addFloor: floor);
  }

  addTable(TableModel tableModel) {
    List<TableModel> list = List.from(state.tableList);
    list.add(tableModel);
    state = state.copyWith(tableList: list);
  }

  deleteTable(int index) {
    List<TableModel> list = List.from(state.tableList);
    list.removeAt(index);
    state = state.copyWith(tableList: list);
  }
}
