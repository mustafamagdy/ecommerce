import 'dart:async';

import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/repository/repository.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'kitchen_state.dart';

class KitchenNotifier extends StateNotifier<KitchenState> {
  final OrdersRepository _ordersRepository;

  KitchenNotifier(this._ordersRepository) : super(const KitchenState());
  int _page = 0;
  Timer? _searchProductsTimer;
  Timer? _refreshTime;

  changeType(String type) {
    state = state.copyWith(selectType: type, orders: []);
    fetchOrders(isRefresh: true);
  }

  selectIndex(int index) async {
    state =
        state.copyWith(selectIndex: index, selectOrder: state.orders[index]);
    final response = await _ordersRepository.getOrderDetailsKitchen(
        orderId: state.selectOrder?.id);
    response.when(
        success: (data) {
          state = state.copyWith(selectOrder: data.data);
        },
        failure: (e) {});
  }

  void setOrdersQuery(BuildContext context, String query) {
    if (state.query == query) {
      return;
    }
    state = state.copyWith(query: query.trim());
    if (state.query.isNotEmpty) {
      if (_searchProductsTimer?.isActive ?? false) {
        _searchProductsTimer?.cancel();
      }
      _searchProductsTimer = Timer(
        const Duration(milliseconds: 500),
        () {
          state = state.copyWith(hasMore: true, orders: []);
          _page = 0;
          fetchOrders(
            isRefresh: true,
            checkYourNetwork: () {
              AppHelpers.showSnackBar(
                context,
                AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
              );
            },
          );
        },
      );
    } else {
      if (_searchProductsTimer?.isActive ?? false) {
        _searchProductsTimer?.cancel();
      }
      _searchProductsTimer = Timer(
        const Duration(milliseconds: 500),
        () {
          state = state.copyWith(hasMore: true, orders: []);
          _page = 0;
          fetchOrders(
              isRefresh: true,
              checkYourNetwork: () {
                AppHelpers.showSnackBar(
                  context,
                  AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
                );
              });
        },
      );
    }
  }

  Future<void> fetchOrders({
    bool isRefresh = false,
    VoidCallback? checkYourNetwork,
  }) async {
    if (isRefresh) {
      _refreshTime?.cancel();
      _page = 0;
      state = state.copyWith(hasMore: true, orders: []);
    }
    if (!state.hasMore) {
      return;
    }
    state = state.copyWith(isLoading: true);
    final response = await _ordersRepository.getKitchenOrders(
      status: state.selectType,
      page: ++_page,
      search: state.query.isEmpty ? null : state.query,
    );
    response.when(
      success: (data) {
        List<OrderData> orders =
            isRefresh || state.query.isNotEmpty ? [] : List.from(state.orders);
        final List<OrderData> newOrders = data.orders ?? [];
        for (OrderData element in data.orders ?? []) {
          if (!orders.map((item) => item.id).contains(element.id)) {
            orders.add(element);
          }
        }
        state = state.copyWith(hasMore: newOrders.length >= 6);
        if (_page == 1 && !isRefresh) {
          state = state.copyWith(
            isLoading: false,
            orders: orders,
          );
        } else {
          state = state.copyWith(
            isLoading: false,
            orders: orders,
          );
        }
        if (isRefresh && (data.orders?.isNotEmpty ?? false)) {
          selectIndex(0);
          _refreshTime = Timer.periodic(AppConstants.refreshTime, (s) async {
            final response = await _ordersRepository.getKitchenOrders(
              status: state.selectType,
              page: 1,
              search: state.query.isEmpty ? null : state.query,
            );
            response.when(
                success: (data) {
                  bool isAdd = false;
                  List<OrderData> orders = List.from(state.orders);
                  for (OrderData element in data.orders ?? []) {
                    if (!orders.map((item) => item.id).contains(element.id)) {
                      orders.insert(0, element);
                      isAdd = true;
                    }
                  }
                  state = state.copyWith(orders: orders);
                  if (isAdd) {
                    selectIndex(0);
                  }
                },
                failure: (f) {});
          });
        } else if (isRefresh && (data.orders?.isEmpty ?? true)) {
          state = state.copyWith(selectOrder: null);
        }
      },
      failure: (failure) {
        _page--;
        if (_page == 0) {
          state = state.copyWith(isLoading: false);
        }
      },
    );
  }

  Future<void> changeStatus({String? status}) async {
    OrderData? newOrder = state.selectOrder?.copyWith(
      status: status ??
          AppHelpers.getOrderStatusText(
            AppHelpers.getOrderStatus(state.selectOrder?.status,
                isNextStatus: true),
          ),
    );
    state = state.copyWith(selectOrder: newOrder);

    List<OrderData> orders = List.from(state.orders);

    for (int i = 0; i < orders.length; i++) {
      if (orders[i].id == state.selectOrder?.id) {
        orders.removeAt(i);
        orders.insert(i, newOrder ?? OrderData());
      }
    }

    state = state.copyWith(orders: orders);

    await _ordersRepository.updateOrderStatusKitchen(
        status: AppHelpers.getOrderStatus(state.selectOrder?.status),
        orderId: state.selectOrder?.id);
  }

  void stopTimer(){
    _refreshTime?.cancel();
  }
}
