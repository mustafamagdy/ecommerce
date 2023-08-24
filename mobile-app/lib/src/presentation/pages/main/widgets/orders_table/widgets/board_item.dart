// ignore_for_file: non_constant_identifier_names

import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/widgets/drag_item.dart';
import 'package:drag_and_drop_lists/drag_and_drop_item.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/app_helpers.dart';
import '../../../../../../models/data/order_data.dart';
import '../../../../../theme/app_colors.dart';

BoardItem({
  required List<OrderData> list,
  required BuildContext context,
  required bool hasMore,
  required bool isLoading,
  required VoidCallback onViewMore,
}) {
  return list.isNotEmpty || isLoading
      ? [
          ...list
              .map((OrderData item) => DragAndDropItem(
                    canDrag: true,
                    child: DragItem(orderData: item),
            feedbackWidget: DragItem(orderData: item,isDrag: true,),
                  ))
              .toList(),
          if (isLoading)
            for (int i = 0; i < 3; i++)
              DragAndDropItem(
                canDrag: false,
                child: Container(
                  height: 344.r,
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(16.r),
                    color: AppColors.shimmerBase,
                  ),
                  margin: EdgeInsets.all(6.r),
                  child: const SizedBox(
                    width: double.infinity,
                  ),
                ),
              ),
          (hasMore
              ? DragAndDropItem(
                  child: Material(
                    borderRadius: BorderRadius.circular(10.r),
                    color: AppColors.transparent,
                    child: InkWell(
                      borderRadius: BorderRadius.circular(10.r),
                      onTap: () => onViewMore(),
                      child: Container(
                        margin:
                            EdgeInsets.only(right: 8.r, left: 8.r, top: 8.r),
                        height: 50.r,
                        decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(10.r),
                          border: Border.all(
                            color: AppColors.black.withOpacity(0.17),
                            width: 1.r,
                          ),
                        ),
                        alignment: Alignment.center,
                        child: Text(
                          AppHelpers.getTranslation(TrKeys.viewMore),
                          style: GoogleFonts.inter(
                            fontSize: 16.sp,
                            color: AppColors.black,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      ),
                    ),
                  ),
                )
              : DragAndDropItem(child: const SizedBox())),
          DragAndDropItem(
            canDrag: false,
            child: const SizedBox(height: 100),
          ),
        ]
      : [
          DragAndDropItem(
            canDrag: false,
            child: Center(
              child: Padding(
                padding: EdgeInsets.only(top: 16.r, bottom: 200.r),
                child: Text(
                  AppHelpers.getTranslation(TrKeys.emptyOrders),
                ),
              ),
            ),
          ),
        ];
}
