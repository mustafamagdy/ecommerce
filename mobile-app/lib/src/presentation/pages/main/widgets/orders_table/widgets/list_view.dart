import 'package:admin_desktop/generated/assets.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/canceled/canceled_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/cooking/cooking_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/delivered/delivered_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/new/new_orders_provider.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:lottie/lottie.dart';
import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/utils.dart';
import '../../../../../../models/data/order_data.dart';
import '../../../../../components/custom_checkbox.dart';
import '../../../riverpod/provider/main_provider.dart';
import '../order_table_riverpod/order_table_provider.dart';
import '../orders/accepted/accepted_orders_provider.dart';
import '../orders/on_a_way/on_a_way_orders_provider.dart';
import '../orders/ready/ready_orders_provider.dart';
import 'custom_popup_item.dart';

part 'list_item.dart';

part 'list_main_item.dart';

part 'list_top_bar.dart';

// ignore: must_be_immutable
class ListViewMode extends ConsumerWidget {
  List<OrderData> listAccepts;
  List<OrderData> listNew;
  List<OrderData> listOnAWay;
  List<OrderData> listReady;
  List<OrderData> listDelivered;
  List<OrderData> listCanceled;
  List<OrderData> listCooking;

  ListViewMode({
    Key? key,
    required this.listAccepts,
    required this.listNew,
    required this.listOnAWay,
    required this.listReady,
    required this.listDelivered,
    required this.listCanceled,
    required this.listCooking,
  }) : super(key: key);

  @override
  Widget build(BuildContext context, ref) {
    final notifier = ref.read(orderTableProvider.notifier);
    final state = ref.watch(orderTableProvider);
    return Padding(
      padding: EdgeInsets.symmetric(horizontal: 16.r, vertical: 20.r),
      child: Column(
        children: [
          SizedBox(
            height: 64.r,
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: [
                  ListTopBar(
                    title: AppHelpers.getTranslation(TrKeys.newKey),
                    count: ref.watch(newOrdersProvider).totalCount.toString(),
                    onRefresh: () {
                      ref
                          .read(newOrdersProvider.notifier)
                          .fetchNewOrders(isRefresh: true);
                    },
                    isLoading: ref.watch(newOrdersProvider).isLoading,
                    color: AppColors.blue,
                    isActive: state.selectTabIndex == 0,
                    onTap: () => notifier.changeTabIndex(0),
                  ),
                  8.r.horizontalSpace,
                  ListTopBar(
                    title: AppHelpers.getTranslation(TrKeys.accepted),
                    count: ref.watch(acceptedOrdersProvider).totalCount.toString(),
                    onRefresh: () {
                      ref
                          .read(acceptedOrdersProvider.notifier)
                          .fetchAcceptedOrders(isRefresh: true);
                    },
                    isLoading: ref.watch(acceptedOrdersProvider).isLoading,
                    color: AppColors.deepPurple,
                    isActive: state.selectTabIndex == 1,
                    onTap: () => notifier.changeTabIndex(1),
                  ),
                  8.r.horizontalSpace,

                  ListTopBar(
                    title: AppHelpers.getTranslation(TrKeys.cooking),
                    count: ref.watch(cookingOrdersProvider).totalCount.toString(),
                    onRefresh: () {
                      ref
                          .read(cookingOrdersProvider.notifier)
                          .fetchCookingOrders(isRefresh: true);
                    },
                    isLoading: ref.watch(cookingOrdersProvider).isLoading,
                    color: AppColors.rate,
                    isActive: state.selectTabIndex == 2,
                    onTap: () => notifier.changeTabIndex(2),
                  ),
                  8.r.horizontalSpace,

                  ListTopBar(
                    title: AppHelpers.getTranslation(TrKeys.ready),
                    count: ref.watch(readyOrdersProvider).totalCount.toString(),
                    onRefresh: () {
                      ref
                          .read(readyOrdersProvider.notifier)
                          .fetchReadyOrders(isRefresh: true);
                    },
                    isLoading: ref.watch(readyOrdersProvider).isLoading,
                    color: AppColors.revenueColor,
                    isActive: state.selectTabIndex == 3,
                    onTap: () => notifier.changeTabIndex(3),
                  ),
                  8.r.horizontalSpace,

                  ListTopBar(
                    title: AppHelpers.getTranslation(TrKeys.onAWay),
                    count: ref.watch(onAWayOrdersProvider).totalCount.toString(),
                    onRefresh: () {
                      ref
                          .read(onAWayOrdersProvider.notifier)
                          .fetchOnAWayOrders(isRefresh: true);
                    },
                    isLoading: ref.watch(onAWayOrdersProvider).isLoading,
                    color: AppColors.black,
                    isActive: state.selectTabIndex == 4,
                    onTap: () => notifier.changeTabIndex(4),
                  ),
                  8.r.horizontalSpace,

                  ListTopBar(
                    title: AppHelpers.getTranslation(TrKeys.delivered),
                    count: ref.watch(deliveredOrdersProvider).totalCount.toString(),
                    onRefresh: () {
                      ref
                          .read(deliveredOrdersProvider.notifier)
                          .fetchDeliveredOrders(isRefresh: true);
                    },
                    isLoading: ref.watch(deliveredOrdersProvider).isLoading,
                    color: AppColors.brandColor,
                    isActive: state.selectTabIndex == 5,
                    onTap: () => notifier.changeTabIndex(5),
                  ),
                  8.r.horizontalSpace,

                  ListTopBar(
                    title: AppHelpers.getTranslation(TrKeys.canceled),
                    count: ref.watch(canceledOrdersProvider).totalCount.toString(),
                    onRefresh: () {
                      ref
                          .read(canceledOrdersProvider.notifier)
                          .fetchCanceledOrders(isRefresh: true);
                    },
                    isLoading: ref.watch(canceledOrdersProvider).isLoading,
                    color: AppColors.red,
                    isActive: state.selectTabIndex == 6,
                    onTap: () => notifier.changeTabIndex(6),
                  ),
                ],
              ),
            ),
          ),
          Expanded(
            child: state.selectTabIndex == 0
                ? ListMainItem(
                    orderList: listNew,
                    color: AppColors.blue,
                    hasMore: ref.watch(newOrdersProvider).hasMore,
                    onViewMore: () {
                      ref.read(newOrdersProvider.notifier).fetchNewOrders();
                    },
                    isLoading: ref.watch(newOrdersProvider).isLoading,
                  )
                : state.selectTabIndex == 1
                    ? ListMainItem(
                        orderList: listAccepts,
                        color: AppColors.deepPurple,
                        hasMore: ref.watch(acceptedOrdersProvider).hasMore,
                        onViewMore: () {
                          ref
                              .read(acceptedOrdersProvider.notifier)
                              .fetchAcceptedOrders();
                        },
                        isLoading: ref.watch(acceptedOrdersProvider).isLoading,
                      )
                    : state.selectTabIndex == 2
                        ? ListMainItem(
                            orderList: listCooking,
                            color: AppColors.rate,
                            hasMore: ref.watch(cookingOrdersProvider).hasMore,
                            onViewMore: () {
                              ref
                                  .read(cookingOrdersProvider.notifier)
                                  .fetchCookingOrders();
                            }, isLoading: ref.watch(cookingOrdersProvider).isLoading,
                          )
                        :state.selectTabIndex == 3
                        ? ListMainItem(
                            orderList: listReady,
                            color: AppColors.revenueColor,
                            hasMore: ref.watch(readyOrdersProvider).hasMore,
                            onViewMore: () {
                              ref
                                  .read(readyOrdersProvider.notifier)
                                  .fetchReadyOrders();
                            }, isLoading: ref.watch(readyOrdersProvider).isLoading,
                          )
                        : state.selectTabIndex == 4
                            ? ListMainItem(
                                orderList: listOnAWay,
                                color: AppColors.black,
                                hasMore:
                                    ref.watch(onAWayOrdersProvider).hasMore,
                                onViewMore: () {
                                  ref
                                      .read(onAWayOrdersProvider.notifier)
                                      .fetchOnAWayOrders();
                                }, isLoading:  ref.watch(onAWayOrdersProvider).isLoading,
                              )
                            : state.selectTabIndex == 5
                                ? ListMainItem(
                                    orderList: listDelivered,
                                    color: AppColors.brandColor,
                                    hasMore: ref
                                        .watch(deliveredOrdersProvider)
                                        .hasMore,
                                    onViewMore: () {
                                      ref
                                          .read(
                                              deliveredOrdersProvider.notifier)
                                          .fetchDeliveredOrders();
                                    },
                                    isLoading: ref
                                        .read(deliveredOrdersProvider)
                                        .isLoading,
                                  )
                                : ListMainItem(
                                    orderList: listCanceled,
                                    color: AppColors.red,
                                    hasMore: ref
                                        .watch(canceledOrdersProvider)
                                        .hasMore,
                                    onViewMore: () {
                                      ref
                                          .read(canceledOrdersProvider.notifier)
                                          .fetchCanceledOrders();
                                    },
                                    isLoading: ref
                                        .read(canceledOrdersProvider)
                                        .isLoading,
                                  ),
          ),
        ],
      ),
    );
  }
}
