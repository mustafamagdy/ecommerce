import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/presentation/components/buttons/animation_button_effect.dart';
import 'package:admin_desktop/src/presentation/pages/main/riverpod/provider/main_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/order_detail/order_detail.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/accepted/accepted_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/canceled/canceled_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/cooking/cooking_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/delivered/delivered_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/new/new_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/on_a_way/on_a_way_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/ready/ready_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/widgets/board_view.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/widgets/list_view.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/widgets/start_end_date.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/widgets/view_mode.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'order_table_riverpod/order_table_provider.dart';

class OrdersTablesPage extends ConsumerStatefulWidget {
  const OrdersTablesPage({Key? key}) : super(key: key);

  @override
  ConsumerState<OrdersTablesPage> createState() => _OrdersTablesState();
}

class _OrdersTablesState extends ConsumerState<OrdersTablesPage> {

  @override
  void initState() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(newOrdersProvider.notifier).fetchNewOrders(isRefresh: true);
      ref
          .read(acceptedOrdersProvider.notifier)
          .fetchAcceptedOrders(isRefresh: true);
      ref
          .read(onAWayOrdersProvider.notifier)
          .fetchOnAWayOrders(isRefresh: true);
      ref.read(readyOrdersProvider.notifier).fetchReadyOrders(isRefresh: true);
      ref
          .read(deliveredOrdersProvider.notifier)
          .fetchDeliveredOrders(isRefresh: true);
      ref
          .read(canceledOrdersProvider.notifier)
          .fetchCanceledOrders(isRefresh: true);
      ref
          .read(cookingOrdersProvider.notifier)
          .fetchCookingOrders(isRefresh: true);
    });
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final listAccepts = ref.watch(acceptedOrdersProvider).orders;
    final listNew = ref.watch(newOrdersProvider).orders;
    final listOnAWay = ref.watch(onAWayOrdersProvider).orders;
    final listReady = ref.watch(readyOrdersProvider).orders;
    final listDelivered = ref.watch(deliveredOrdersProvider).orders;
    final listCancel = ref.watch(canceledOrdersProvider).orders;
    final listCooking = ref.watch(cookingOrdersProvider).orders;
    final notifier = ref.read(orderTableProvider.notifier);
    final state = ref.watch(orderTableProvider);
    final stateMain = ref.watch(mainProvider);

    return stateMain.selectedOrder != null
        ? OrderDetailPage(order: stateMain.selectedOrder ?? OrderData())
        : SafeArea(
            child: Column(
              children: [
                Container(
                  padding: EdgeInsets.symmetric(
                      vertical: state.showFilter ? 20.r : 10.r,
                      horizontal: 16.r),
                  decoration: const BoxDecoration(color: AppColors.white),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Text(AppHelpers.getTranslation(TrKeys.order),
                              style: GoogleFonts.inter(
                                color: AppColors.black,
                                fontWeight: FontWeight.w600,
                                fontSize: 20.sp,
                              )),
                          const Spacer(),
                          IconButton(
                              onPressed: () => notifier.changeFilter(),
                              icon: state.showFilter
                                  ? const Icon(FlutterRemix.arrow_up_s_line)
                                  : const Icon(FlutterRemix.arrow_down_s_line))
                        ],
                      ),
                      Visibility(
                          visible: state.showFilter,
                          child: Column(
                            children: [
                              16.verticalSpace,
                              Row(
                                children: [
                                  InkWell(
                                    onTap: () {
                                      ref
                                          .read(orderTableProvider.notifier)
                                          .setTime(null, null);
                                      ref
                                          .read(newOrdersProvider.notifier)
                                          .fetchNewOrders(isRefresh: true);
                                      ref
                                          .read(cookingOrdersProvider.notifier)
                                          .fetchCookingOrders(isRefresh: true);
                                      ref
                                          .read(acceptedOrdersProvider.notifier)
                                          .fetchAcceptedOrders(isRefresh: true);
                                      ref
                                          .read(onAWayOrdersProvider.notifier)
                                          .fetchOnAWayOrders(isRefresh: true);
                                      ref
                                          .read(readyOrdersProvider.notifier)
                                          .fetchReadyOrders(isRefresh: true);
                                      ref
                                          .read(
                                              deliveredOrdersProvider.notifier)
                                          .fetchDeliveredOrders(
                                              isRefresh: true);
                                      ref
                                          .read(canceledOrdersProvider.notifier)
                                          .fetchCanceledOrders(isRefresh: true);
                                    },
                                    child: AnimationButtonEffect(
                                      child: Container(
                                          decoration: BoxDecoration(
                                              color: AppColors.white,
                                              borderRadius:
                                                  BorderRadius.circular(10.r),
                                              border: Border.all(
                                                  color: AppColors
                                                      .unselectedBottomBarBack)),
                                          padding: EdgeInsets.all(8.r),
                                          child: const Icon(
                                              FlutterRemix.restart_line)),
                                    ),
                                  ),
                                  16.horizontalSpace,
                                  StartEndDate(
                                    start: state.start,
                                    end: state.end,
                                  ),
                                  const Spacer(),
                                  ViewMode(
                                    title:
                                        AppHelpers.getTranslation(TrKeys.board),
                                    isActive: !state.isListView,
                                    icon: FlutterRemix.dashboard_line,
                                    onTap: () => notifier.changeViewMode(0),
                                  ),
                                  ViewMode(
                                    title:
                                        AppHelpers.getTranslation(TrKeys.list),
                                    isActive: state.isListView,
                                    isLeft: false,
                                    icon: FlutterRemix.menu_fill,
                                    onTap: () => notifier.changeViewMode(1),
                                  ),
                                ],
                              ),
                            ],
                          ))
                    ],
                  ),
                ),
                Expanded(
                  child: !state.isListView
                      ? BoardViewMode(
                          listAccepts: listAccepts,
                          listNew: listNew,
                          listOnAWay: listOnAWay,
                          listReady: listReady,
                          listCanceled: listCancel,
                          listDelivered: listDelivered,
                          listCooking: listCooking,
                        )
                      : ListViewMode(
                          listAccepts: listAccepts,
                          listNew: listNew,
                          listOnAWay: listOnAWay,
                          listReady: listReady,
                          listCanceled: listCancel,
                          listDelivered: listDelivered,
                          listCooking: listCooking,
                        ),
                ),
              ],
            ),
          );
  }
}
