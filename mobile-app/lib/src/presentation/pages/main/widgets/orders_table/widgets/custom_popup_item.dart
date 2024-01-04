import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/presentation/components/components.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/accepted/accepted_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/canceled/canceled_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/delivered/delivered_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/new/new_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/on_a_way/on_a_way_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/ready/ready_orders_provider.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/utils.dart';
import '../../../../../theme/theme.dart';
import '../../../riverpod/provider/main_provider.dart';
import '../../order_detail/generate_check.dart';
import 'map_dialog.dart';

class CustomPopup extends ConsumerWidget {
  final OrderData orderData;
  final bool isLocation;
  final int? index;

  const CustomPopup({
    Key? key,
    required this.orderData,
    required this.isLocation,
    this.index,
  }) : super(key: key);

  @override
  Widget build(BuildContext context, ref) {
    return _CustomPopupItem(
      onLocation: () {
        AppHelpers.showAlertDialog(
            context: context, child: MapDialog(orderData: orderData));
      },
      onEdit: () => ref.read(mainProvider.notifier).setOrder(orderData),
      onDownload: () {
        showDialog(
            context: context,
            builder: (context) {
              return LayoutBuilder(builder: (context, constraints) {
                return SimpleDialog(
                  title: SizedBox(
                    height: constraints.maxHeight * 0.7,
                    width: constraints.maxWidth * 0.4,
                    child: GenerateCheckPage(orderData: orderData),
                  ),
                );
              });
            });
      },
      onDelete: () {
        Navigator.pop(context);
        showDialog(
            context: context,
            builder: (_) => AlertDialog(
                  titlePadding: const EdgeInsets.all(16),
                  actionsPadding: const EdgeInsets.all(16),
                  title: Text(
                    AppHelpers.getTranslation(TrKeys.deleteOrder),
                    style: GoogleFonts.inter(
                      fontSize: 18.sp,
                      color: AppColors.black,
                      fontWeight: FontWeight.w400,
                    ),
                  ),
                  actions: [
                    SizedBox(
                      width: 100,
                      child: ConfirmButton(
                          title: AppHelpers.getTranslation(TrKeys.no),
                          onTap: () {
                            Navigator.pop(context);
                          }),
                    ),
                    SizedBox(
                      width: 100,
                      child: ConfirmButton(
                          title: AppHelpers.getTranslation(TrKeys.yes),
                          onTap: () {
                            if (orderData.status == 'accepted') {
                              ref
                                  .read(acceptedOrdersProvider.notifier)
                                  .deleteOrder(
                                    context,
                                    orderId: orderData.id,
                                  );
                            } else if (orderData.status == 'ready') {
                              ref
                                  .read(readyOrdersProvider.notifier)
                                  .deleteOrder(
                                    context,
                                    orderId: orderData.id,
                                  );
                            } else if (orderData.status == 'on_a_way') {
                              ref
                                  .read(onAWayOrdersProvider.notifier)
                                  .deleteOrder(
                                    context,
                                    orderId: orderData.id,
                                  );
                            } else if (orderData.status == 'delivered') {
                              ref
                                  .read(deliveredOrdersProvider.notifier)
                                  .deleteOrder(
                                    context,
                                    orderId: orderData.id,
                                  );
                            } else if (orderData.status == 'canceled') {
                              ref
                                  .read(canceledOrdersProvider.notifier)
                                  .deleteOrder(
                                    context,
                                    orderId: orderData.id,
                                  );
                            } else {
                              ref.read(newOrdersProvider.notifier).deleteOrder(
                                    context,
                                    orderId: orderData.id,
                                  );
                            }
                            Navigator.pop(context);
                          }),
                    ),
                  ],
                ));
      },
      isLocation: isLocation,
    );
  }
}

class _CustomPopupItem extends StatelessWidget {
  final VoidCallback onLocation;
  final VoidCallback onEdit;
  final VoidCallback onDownload;
  final VoidCallback onDelete;
  final bool isLocation;

  const _CustomPopupItem({
    Key? key,
    required this.onLocation,
    required this.onEdit,
    required this.onDownload,
    required this.onDelete,
    required this.isLocation,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return PopupMenuButton(
      tooltip: "",
      splashRadius: 40.r,
      iconSize: 24.r,
      icon: const Icon(FlutterRemix.more_fill),
      itemBuilder: (c) {
        if (isLocation) {
          return [
            _buildPopupMenuItem(
              title: AppHelpers.getTranslation(TrKeys.locations),
              iconData: FlutterRemix.map_pin_range_line,
              onTap: () {
                c.popRoute();
                onLocation();
              },
            ),
            _buildPopupMenuItem(
              title: AppHelpers.getTranslation(TrKeys.editOrder),
              iconData: FlutterRemix.pencil_line,
              onTap: () {
                c.popRoute();
                onEdit();
              },
            ),
            _buildPopupMenuItem(
              title: AppHelpers.getTranslation(TrKeys.download),
              iconData: FlutterRemix.download_2_line,
              onTap: () {
                c.popRoute();
                onDownload();
              },
            ),
            _buildPopupMenuItem(
              title: AppHelpers.getTranslation(TrKeys.delete),
              iconData: FlutterRemix.delete_bin_line,
              onTap: () {
                c.popRoute();
                onDelete();
              },
            ),
          ];
        } else {
          return [
            _buildPopupMenuItem(
              title: AppHelpers.getTranslation(TrKeys.editOrder),
              iconData: FlutterRemix.pencil_line,
              onTap: () {
                c.popRoute();
                onEdit();
              },
            ),
            _buildPopupMenuItem(
              title: AppHelpers.getTranslation(TrKeys.download),
              iconData: FlutterRemix.download_2_line,
              onTap: () {
                c.popRoute();
                onDownload();
              },
            ),
            _buildPopupMenuItem(
              title: AppHelpers.getTranslation(TrKeys.delete),
              iconData: FlutterRemix.delete_bin_line,
              onTap: () {
                c.popRoute();
                onDelete();
              },
            ),
          ];
        }
      },
    );
  }

  PopupMenuItem _buildPopupMenuItem({
    required String title,
    required IconData iconData,
    required VoidCallback onTap,
  }) {
    return PopupMenuItem(
      child: GestureDetector(
        onTap: onTap,
        child: Row(
          children: [
            2.horizontalSpace,
            Icon(iconData, size: 24.r),
            8.horizontalSpace,
            Text(
              title,
              style: GoogleFonts.inter(
                fontSize: 16.sp,
                color: AppColors.black,
                fontWeight: FontWeight.w400,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
