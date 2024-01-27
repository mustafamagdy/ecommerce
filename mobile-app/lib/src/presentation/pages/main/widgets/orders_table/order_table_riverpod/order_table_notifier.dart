import 'package:admin_desktop/generated/assets.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';

import '../../../../../../models/data/order_data.dart';
import 'order_table_state.dart';

class OrderTableNotifier extends StateNotifier<OrderTableState> {
  OrderTableNotifier() : super(const OrderTableState());

  void changeViewMode(int index) {
    state = state.copyWith(isListView: index == 0 ? false : true);
  }

  setTime(DateTime? start, DateTime? end) {
    state = state.copyWith(start: start, end: end);
  }

  changeFilter() {
    state = state.copyWith(showFilter: !state.showFilter);
  }

  void changeTabIndex(int index) {
    state = state
        .copyWith(selectTabIndex: index, isAllSelect: false, selectOrders: []);
  }

  // void setUsersQuery(BuildContext context, String query) {
  //   if (state.usersQuery == query) {
  //     return;
  //   }
  //   state = state.copyWith(usersQuery: query.trim());
  //
  //   if (_searchUsersTimer?.isActive ?? false) {
  //     _searchUsersTimer?.cancel();
  //   }
  //   _searchUsersTimer = Timer(
  //     const Duration(milliseconds: 500),
  //     () {
  //       state = state.copyWith(users: [], dropdownUsers: []);
  //       fetchUsers(
  //         checkYourNetwork: () {
  //           AppHelpers.showSnackBar(
  //             context,
  //             AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
  //           );
  //         },
  //       );
  //     },
  //   );
  // }

  void addSelectOrder({int? id, required int orderLength}) {
    List list = List.from(state.selectOrders);
    if (state.selectOrders.contains(id)) {
      list.remove(id);
    } else {
      list.add(id ?? 0);
    }
    state = state.copyWith(selectOrders: list);

    if (list.length == orderLength) {
      state = state.copyWith(isAllSelect: true);
    } else {
      state = state.copyWith(isAllSelect: false);
    }
  }

  void allSelectOrder(List<OrderData> orderList) {
    List list = [];
    if (!state.isAllSelect) {
      for (int i = 0; i < orderList.length; i++) {
        list.add(orderList[i].id);
      }
      state = state.copyWith(isAllSelect: true);
    } else {
      state = state.copyWith(isAllSelect: false);
    }
    state = state.copyWith(selectOrders: list);
  }

  void setMarkerIcon(LatLng latLng) async {
    state = state.copyWith(setOfMarker: {
      Marker(
        markerId: const MarkerId("1"),
        draggable: true,
        consumeTapEvents: true,
        flat: true,
        icon: await BitmapDescriptor.fromAssetImage(
            ImageConfiguration(size: Size(48.r, 75.r)), Assets.pngMarker),
        position: latLng,
      )
    });
  }
}
