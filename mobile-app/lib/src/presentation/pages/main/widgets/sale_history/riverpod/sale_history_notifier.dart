import 'package:admin_desktop/src/models/response/sale_history_response.dart';
import 'package:admin_desktop/src/repository/settings_repository.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../../../core/utils/app_connectivity.dart';
import 'sale_history_state.dart';

class SaleHistoryNotifier extends StateNotifier<SaleHistoryState> {
  final SettingsRepository _settingsRepository;
  int driverPage = 0;
  int salePage = 0;
  int historyPage = 0;

  SaleHistoryNotifier(this._settingsRepository)
      : super(const SaleHistoryState());

  changeIndex(int index) {
    state = state.copyWith(selectIndex: index, hasMore: false);
    fetchSale();
  }

  fetchSaleCarts() async {
    final response = await _settingsRepository.getSaleCart();
    response.when(
      success: (data) async {
        state = state.copyWith(saleCart: data);
      },
      failure: (failure) {},
    );
  }

  fetchSale() async {
    final response =
        await _settingsRepository.getSaleHistory(state.selectIndex, 1);
    state = state.copyWith(
      isLoading: state.selectIndex == 0
          ? state.listDriver.isEmpty
          : state.selectIndex == 1
              ? state.listToday.isEmpty
              : state.listHistory.isEmpty,
    );
    response.when(
      success: (data) async {
        switch (state.selectIndex) {
          case 0:
            state = state.copyWith(
                isLoading: false,
                listDriver: data.list ?? [],
                hasMore: (data.list?.length ?? 0) == 10);
            break;
          case 1:
            state = state.copyWith(
                isLoading: false,
                listToday: data.list ?? [],
                hasMore: (data.list?.length ?? 0) == 10);
            break;
          case 2:
            state = state.copyWith(
                isLoading: false,
                listHistory: data.list ?? [],
                hasMore: (data.list?.length ?? 0) == 10);
            break;
        }
      },
      failure: (failure) {
        state = state.copyWith(isLoading: false);
      },
    );
  }

  

  Future<void> fetchSalePage({
    VoidCallback? checkYourNetwork,
  }) async {
    if (!state.hasMore) {
      return;
    }
    final connected = await AppConnectivity.connectivity();
    if (connected) {
      if (driverPage == 1 && salePage == 1 && historyPage == 1) {
        state = state.copyWith(
            isLoading: true, listDriver: [], listHistory: [], listToday: []);

        final response = await _settingsRepository.getSaleHistory(
            state.selectIndex,
            state.selectIndex == 0
                ? ++driverPage
                : state.selectIndex == 1
                    ? ++salePage
                    : ++historyPage);

        response.when(
          success: (data) {
            state = state.copyWith(
              listDriver: data.list ?? [],
              listHistory: data.list ?? [],
              listToday: data.list ?? [],
              isLoading: false,
            );
            switch (state.selectIndex) {
              case 0:
                List<SaleHistoryModel> list = List.from(state.listDriver);
                list.addAll(data.list ?? []);
                state = state.copyWith(
                    listDriver: list, hasMore: (data.list?.length ?? 0) == 10);
                break;
              case 1:
                List<SaleHistoryModel> list = List.from(state.listToday);
                list.addAll(data.list ?? []);
                state = state.copyWith(
                    listToday: list, hasMore: (data.list?.length ?? 0) == 10);
                break;
              case 2:
                List<SaleHistoryModel> list = List.from(state.listHistory);
                list.addAll(data.list ?? []);
                state = state.copyWith(
                    listHistory: list, hasMore: (data.list?.length ?? 0) == 10);
                break;
            }
          },
          failure: (failure) {
            state = state.copyWith(isLoading: false);
            debugPrint('==> get sales history failure: $failure');
          },
        );
      } else {
        state = state.copyWith(isMoreLoading: true);
        final response = await _settingsRepository.getSaleHistory(
            state.selectIndex,
            state.selectIndex == 0
                ? ++driverPage
                : state.selectIndex == 1
                    ? ++salePage
                    : ++historyPage);
        response.when(
          success: (data) async {
            switch (state.selectIndex) {
              case 0:
                List<SaleHistoryModel> list = List.from(state.listDriver);
                list.addAll(data.list ?? []);
                state = state.copyWith(
                    isMoreLoading: false,
                    listDriver: list,
                    hasMore: (data.list?.length ?? 0) == 10);
                break;
              case 1:
                List<SaleHistoryModel> list = List.from(state.listToday);
                list.addAll(data.list ?? []);
                state = state.copyWith(
                    isMoreLoading: false,
                    listToday: list,
                    hasMore: (data.list?.length ?? 0) == 10);
                break;
              case 2:
                List<SaleHistoryModel> list = List.from(state.listHistory);
                list.addAll(data.list ?? []);
                state = state.copyWith(
                    isMoreLoading: false,
                    listHistory: list,
                    hasMore: (data.list?.length ?? 0) == 10);
                break;
            }
          },
          failure: (failure) {
            state = state.copyWith(isMoreLoading: false);
            debugPrint('==> get users  failure: $failure');
          },
        );
      }
    } else {
      checkYourNetwork?.call();
    }
  }
}
