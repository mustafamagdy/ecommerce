// ignore_for_file: must_be_immutable

import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../../core/constants/constants.dart';
import '../../../../../../models/models.dart';
import '../../../../../theme/theme.dart';
import 'custom_chair.dart';

class CustomTable extends StatelessWidget {
  final TableModel tableModel;
  final double? tableHeight;
  final double? tableWidth;

  const CustomTable({
    Key? key,
    required this.tableModel,
    this.tableHeight,
    this.tableWidth,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    PositionModel positionModel = AppHelpers.fixedTable(tableModel.count);
    return ResponsiveTable(
      tableModel: tableModel,
      top: positionModel.top,
      left: positionModel.left,
      right: positionModel.right,
      bottom: positionModel.bottom,
      title: tableModel.title,
      tableHeight: tableHeight,
      tableWidth: tableWidth,
    );
  }
}

class ResponsiveTable extends StatelessWidget {
  final int top;
  final int left;
  final int right;
  final int bottom;
  final double chairHeight;
  final double chairWidth;
  final double chairSpace;
  final double chairWithTableSpace;
  final String title;
  final double? tableHeight;
  final double? tableWidth;
  final TableModel tableModel;

  const ResponsiveTable({
    Key? key,
    this.top = 0,
    this.left = 1,
    this.right = 1,
    this.bottom = 0,
    this.chairHeight = 24,
    this.chairWidth = 64,
    this.chairSpace = 8,
    this.chairWithTableSpace = 6,
    required this.title,
    this.tableHeight,
    this.tableWidth,
    required this.tableModel,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    int withCount = top > bottom ? top : bottom;
    int heightCount = left > right ? left : right;

    double width = withCount != 0 && withCount != 1
        ? (chairWidth * withCount) + ((withCount - 1) * chairSpace.r)
        : chairWidth * 1.4;
    double height = heightCount != 0 && heightCount != 1
        ? (chairWidth * heightCount) + ((heightCount - 1) * chairSpace.r)
        : chairWidth * 1.4;

    if (left != 0) {
      width += chairHeight + chairWithTableSpace;
    }
    if (right != 0) {
      width += chairHeight + chairWithTableSpace;
    }
    if (top != 0) {
      height += chairHeight;
    }
    if (bottom != 0) {
      height += chairHeight + chairWithTableSpace;
    }
    return SizedBox(
      width: tableWidth ?? width,
      height: tableHeight ?? height,
      child: Column(
        children: [
          SizedBox(
              height: top != 0 ? chairHeight.r : 0,
              width: double.infinity,
              child: Padding(
                padding: REdgeInsets.only(
                  left: left != 0
                      ? chairHeight + chairSpace * 2.5
                      : top == 1
                          ? chairSpace * 2.5
                          : 0,
                  right: right != 0
                      ? chairHeight + chairSpace * 2.5
                      : top == 1
                          ? chairSpace * 2.5
                          : 0,
                ),
                child: Row(
                  children: List.generate(
                      top,
                      (index) => Expanded(
                            child: VerticalChair(
                                chairPosition: ChairPosition.top,
                                chairSpace: index == 0 ? 0 : chairSpace),
                          )),
                ),
              )),
          Expanded(
            child: Row(
              children: [
                SizedBox(
                  height: left != 0
                      ? (chairWidth.r * left + (heightCount - 1) * chairSpace.r)
                      : 0,
                  width: left != 0 ? chairHeight.r : 0,
                  child: ListView.builder(
                    physics: const NeverScrollableScrollPhysics(),
                      itemCount: left,
                      shrinkWrap: true,
                      scrollDirection: Axis.vertical,
                      itemBuilder: (context, index) {
                        return CustomChair(
                            chairPosition: ChairPosition.left,
                            chairSpace: index == 0 ? 0 : chairSpace);
                      }),
                ),
                Expanded(
                  child: Padding(
                    padding: EdgeInsets.all(chairWithTableSpace),
                    child: Container(
                      height: double.infinity,
                      width: double.infinity,
                      decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(8.r)),
                      child: Center(
                        child: Container(
                          padding: REdgeInsets.all(14),
                          decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(10.r),
                              color:  tableModel.isActive
                                  ? AppColors.red
                                  :  AppColors.addButtonColor),
                          child: Text(
                            title,
                            style: GoogleFonts.inter(
                              color: tableModel.isActive
                                  ? AppColors.white
                                  :  AppColors.reviewText,
                              fontWeight: FontWeight.w500,
                              fontSize: 16.sp,
                              decoration: TextDecoration.none,
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
                SizedBox(
                  height: right != 0
                      ? (chairWidth.r * right +
                          (heightCount - 1) * chairSpace.r)
                      : 0,
                  width: right != 0 ? chairHeight.r : 0,
                  child: ListView.builder(

                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: right,
                      shrinkWrap: true,
                      scrollDirection: Axis.vertical,
                      itemBuilder: (context, index) {
                        return CustomChair(
                            chairPosition: ChairPosition.right,
                            chairSpace: index == 0 ? 0 : chairSpace);
                      }),
                ),
              ],
            ),
          ),
          SizedBox(
              height: bottom != 0 ? chairHeight.r : 0,
              width: double.infinity,
              child: Padding(
                padding: REdgeInsets.only(
                  left: left != 0
                      ? chairHeight + chairSpace * 2.5
                      : bottom == 1
                          ? chairSpace * 2.5
                          : 0,
                  right: right != 0
                      ? chairHeight + chairSpace * 2.5
                      : bottom == 1
                          ? chairSpace * 2.5
                          : 0,
                ),
                child: Row(
                  children: List.generate(
                      bottom,
                      (index) => Expanded(
                            child: VerticalChair(
                                chairPosition: ChairPosition.bottom,
                                chairSpace: index == 0 ? 0 : chairSpace),
                          )),
                ),
              )),
        ],
      ),
    );
  }
}
