// ignore_for_file: must_be_immutable

import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/canceled/canceled_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/cooking/cooking_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/delivered/delivered_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/widgets/board_top_bar.dart';
import 'package:drag_and_drop_lists/drag_and_drop_lists.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/utils.dart';
import '../../../../../../models/data/order_data.dart';
import '../../../../../theme/theme.dart';
import '../orders/accepted/accepted_orders_provider.dart';
import '../orders/new/new_orders_provider.dart';
import '../orders/on_a_way/on_a_way_orders_provider.dart';
import '../orders/ready/ready_orders_provider.dart';
import 'board_item.dart';

class BoardViewMode extends ConsumerStatefulWidget {
  List<OrderData> listAccepts;
  List<OrderData> listNew;
  List<OrderData> listCooking;
  List<OrderData> listOnAWay;
  List<OrderData> listReady;
  List<OrderData> listDelivered;
  List<OrderData> listCanceled;

  BoardViewMode({
    Key? key,
    required this.listAccepts,
    required this.listNew,
    required this.listCooking,
    required this.listOnAWay,
    required this.listReady,
    required this.listDelivered,
    required this.listCanceled,
  }) : super(key: key);

  @override
  ConsumerState<BoardViewMode> createState() => _BoardViewState();
}

class _BoardViewState extends ConsumerState<BoardViewMode> {
  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Expanded(
          child: DragAndDropLists(
            axis: Axis.horizontal,
            listWidth: (MediaQuery.of(context).size.width / 4) - 40.r,
            listPadding: EdgeInsets.all(12.r),
            listInnerDecoration: BoxDecoration(
              color: AppColors.transparent,
              borderRadius: BorderRadius.circular(10.r),
            ),
            listDragOnLongPress: true,
            disableScrolling: false,
            children: [
              buildList(OrderStatus.newOrder),
              buildList(OrderStatus.accepted),
              buildList(OrderStatus.cooking),
              buildList(OrderStatus.ready),
              buildList(OrderStatus.onAWay),
              buildList(OrderStatus.delivered),
              buildList(OrderStatus.canceled),
            ],
            itemDecorationWhileDragging: const BoxDecoration(
              color: AppColors.transparent,
            ),
            onItemReorder: onReorderListItem,
            onListReorder: onReorderList,
          ),
        )
      ],
    );
  }

  DragAndDropList buildList(OrderStatus orderStatus) {
    List<OrderData> list;
    String header;
    bool hasMore;
    VoidCallback onViewMore;
    Color itemColor;
    String count;
    bool isLoading;
    VoidCallback onRefresh;

    switch (orderStatus) {
      case OrderStatus.newOrder:
        list = widget.listNew;
        header = AppHelpers.getTranslation(TrKeys.newKey);
        hasMore = ref.watch(newOrdersProvider).hasMore;
        onViewMore =
            () => ref.read(newOrdersProvider.notifier).fetchNewOrders();
        itemColor = AppColors.blue;
        count = ref.watch(newOrdersProvider).totalCount.toString();
        isLoading = ref.watch(newOrdersProvider).isLoading;
        onRefresh = () {
          ref.read(newOrdersProvider.notifier).fetchNewOrders(isRefresh: true);
        };
        break;
      case OrderStatus.accepted:
        list = widget.listAccepts;
        header = AppHelpers.getTranslation(TrKeys.accepted);
        hasMore = ref.watch(acceptedOrdersProvider).hasMore;
        onViewMore = () =>
            ref.read(acceptedOrdersProvider.notifier).fetchAcceptedOrders();
        itemColor = AppColors.deepPurple;
        count = ref.watch(acceptedOrdersProvider).totalCount.toString();
        isLoading = ref.watch(acceptedOrdersProvider).isLoading;
        onRefresh = () {
          ref
              .read(acceptedOrdersProvider.notifier)
              .fetchAcceptedOrders(isRefresh: true);
        };
        break;
      case OrderStatus.cooking:
        list = widget.listCooking;
        header = AppHelpers.getTranslation(TrKeys.cooking);
        hasMore = ref.watch(cookingOrdersProvider).hasMore;
        onViewMore =
            () => ref.read(cookingOrdersProvider.notifier).fetchCookingOrders();
        itemColor = AppColors.rate;
        count = ref.watch(cookingOrdersProvider).totalCount.toString();
        isLoading = ref.watch(cookingOrdersProvider).isLoading;
        onRefresh = () {
          ref
              .read(cookingOrdersProvider.notifier)
              .fetchCookingOrders(isRefresh: true);
        };
        break;
      case OrderStatus.ready:
        list = widget.listReady;
        header = AppHelpers.getTranslation(TrKeys.ready);
        hasMore = ref.watch(readyOrdersProvider).hasMore;
        onViewMore =
            () => ref.read(readyOrdersProvider.notifier).fetchReadyOrders();
        itemColor = AppColors.revenueColor;
        count = ref.watch(readyOrdersProvider).totalCount.toString();
        isLoading = ref.watch(readyOrdersProvider).isLoading;
        onRefresh = () {
          ref
              .read(readyOrdersProvider.notifier)
              .fetchReadyOrders(isRefresh: true);
        };
        break;
      case OrderStatus.onAWay:
        list = widget.listOnAWay;
        header = AppHelpers.getTranslation(TrKeys.onAWay);
        hasMore = ref.watch(onAWayOrdersProvider).hasMore;
        onViewMore =
            () => ref.read(onAWayOrdersProvider.notifier).fetchOnAWayOrders();
        itemColor = AppColors.black;
        count = ref.watch(onAWayOrdersProvider).totalCount.toString();
        isLoading = ref.watch(onAWayOrdersProvider).isLoading;
        onRefresh = () {
          ref
              .read(onAWayOrdersProvider.notifier)
              .fetchOnAWayOrders(isRefresh: true);
        };

        break;
      case OrderStatus.delivered:
        list = widget.listDelivered;
        header = AppHelpers.getTranslation(TrKeys.delivered);
        hasMore = ref.watch(deliveredOrdersProvider).hasMore;
        onViewMore = () {
          ref.read(deliveredOrdersProvider.notifier).fetchDeliveredOrders();
        };
        itemColor = AppColors.brandColor;
        count = ref.watch(deliveredOrdersProvider).totalCount.toString();
        isLoading = ref.watch(deliveredOrdersProvider).isLoading;
        onRefresh = () {
          ref
              .read(deliveredOrdersProvider.notifier)
              .fetchDeliveredOrders(isRefresh: true);
        };

        break;
      case OrderStatus.canceled:
        list = widget.listCanceled;
        header = AppHelpers.getTranslation(TrKeys.canceled);
        hasMore = ref.watch(canceledOrdersProvider).hasMore;
        onViewMore = () {
          ref.read(canceledOrdersProvider.notifier).fetchCanceledOrders();
        };
        itemColor = AppColors.red;
        count = ref.watch(canceledOrdersProvider).totalCount.toString();
        isLoading = ref.watch(canceledOrdersProvider).isLoading;
        onRefresh = () {
          ref
              .read(canceledOrdersProvider.notifier)
              .fetchCanceledOrders(isRefresh: true);
        };
        break;

      default:
        list = [];
        header = '';
        hasMore = false;
        onViewMore = () {};
        itemColor = AppColors.transparent;
        count = "0";
        isLoading = false;
        onRefresh = () {};
        break;
    }

    return DragAndDropList(
      canDrag: false,
      decoration: const BoxDecoration(color: AppColors.mainBack),
      header: BoardTopBar(
          title: header,
          count: count,
          onTap: onRefresh,
          isLoading: isLoading,
          color: itemColor),
      children: BoardItem(
        list: list,
        context: context,
        hasMore: hasMore,
        onViewMore: onViewMore,
        isLoading: isLoading,
      ),
    );
  }

  void onReorderListItem(
    int oldItemIndex,
    int oldListIndex,
    int newItemIndex,
    int newListIndex,
  ) {
    if (newListIndex > oldListIndex) {
      switch (newListIndex) {
        case 1:
          {
            ref.read(acceptedOrdersProvider.notifier).addList(
                ref.watch(newOrdersProvider).orders[oldItemIndex], context);

            break;
          }
        case 2:
          {
            ref.read(cookingOrdersProvider.notifier).addList(
                oldListIndex == 0
                    ? ref.watch(newOrdersProvider).orders[oldItemIndex]
                    : ref.watch(acceptedOrdersProvider).orders[oldItemIndex],
                context);
            break;
          }
        case 3:
          {
            ref.read(readyOrdersProvider.notifier).addList(
                oldListIndex == 0
                    ? ref.watch(newOrdersProvider).orders[oldItemIndex]
                    : oldListIndex == 1
                        ? ref.watch(acceptedOrdersProvider).orders[oldItemIndex]
                        : ref.watch(cookingOrdersProvider).orders[oldItemIndex],
                context);
            break;
          }
        case 4:
          {
            ref.read(onAWayOrdersProvider.notifier).addList(
                oldListIndex == 0
                    ? ref.watch(newOrdersProvider).orders[oldItemIndex]
                    : oldListIndex == 1
                        ? ref.watch(acceptedOrdersProvider).orders[oldItemIndex]
                        : oldListIndex == 2
                            ? ref
                                .watch(cookingOrdersProvider)
                                .orders[oldItemIndex]
                            : ref
                                .watch(readyOrdersProvider)
                                .orders[oldItemIndex],
                context);
            break;
          }
        case 5:
          {
            ref.read(deliveredOrdersProvider.notifier).addList(
                oldListIndex == 0
                    ? ref.watch(newOrdersProvider).orders[oldItemIndex]
                    : oldListIndex == 1
                        ? ref.watch(acceptedOrdersProvider).orders[oldItemIndex]
                        : oldListIndex == 2
                            ? ref
                                .watch(cookingOrdersProvider)
                                .orders[oldItemIndex]
                            : oldListIndex == 3
                                ? ref
                                    .watch(readyOrdersProvider)
                                    .orders[oldItemIndex]
                                : ref
                                    .watch(onAWayOrdersProvider)
                                    .orders[oldItemIndex],
                context);
            break;
          }
        case 6:
          {
            ref.read(canceledOrdersProvider.notifier).addList(
                oldListIndex == 0
                    ? ref.watch(newOrdersProvider).orders[oldItemIndex]
                    : oldListIndex == 1
                        ? ref.watch(acceptedOrdersProvider).orders[oldItemIndex]
                        : oldListIndex == 2
                            ? ref
                                .watch(cookingOrdersProvider)
                                .orders[oldItemIndex]
                            : oldListIndex == 3
                                ? ref
                                    .watch(readyOrdersProvider)
                                    .orders[oldItemIndex]
                                : oldListIndex == 4
                                    ? ref
                                        .watch(onAWayOrdersProvider)
                                        .orders[oldItemIndex]
                                    : ref
                                        .watch(deliveredOrdersProvider)
                                        .orders[oldItemIndex],
                context);
            break;
          }
      }

      switch (oldListIndex) {
        case 0:
          {
            ref.read(newOrdersProvider.notifier).removeList(oldItemIndex);
            break;
          }
        case 1:
          {
            ref.read(acceptedOrdersProvider.notifier).removeList(oldItemIndex);
            break;
          }
        case 2:
          {
            ref.read(cookingOrdersProvider.notifier).removeList(oldItemIndex);
            break;
          }
        case 3:
          {
            ref.read(readyOrdersProvider.notifier).removeList(oldItemIndex);
            break;
          }
        case 4:
          {
            ref.read(onAWayOrdersProvider.notifier).removeList(oldItemIndex);
            break;
          }
        case 5:
          {
            ref.read(deliveredOrdersProvider.notifier).removeList(oldItemIndex);
            break;
          }
      }
    }
  }

  void onReorderList(
    int oldListIndex,
    int newListIndex,
  ) {}
}
