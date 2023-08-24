import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_staggered_animations/flutter_staggered_animations.dart';

import '../../theme/theme.dart';
import '../physics/bouncing_physics.dart';

class ProductGridListShimmer extends StatelessWidget {
  final int itemCount;
  final int verticalPadding;

  const ProductGridListShimmer({
    Key? key,
    this.itemCount = 12,
    this.verticalPadding = 8,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return AnimationLimiter(
      child: GridView.builder(
        shrinkWrap: true,
        itemCount: itemCount,
        primary: false,
        gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
          childAspectRatio: 227 / 246,
          mainAxisSpacing: 10.r,
          crossAxisSpacing: 10.r,
          crossAxisCount: 4,
        ),
        padding: REdgeInsets.symmetric(vertical: verticalPadding.r),
        physics: const CustomBouncingScrollPhysics(),
        itemBuilder: (context, index) {
          return AnimationConfiguration.staggeredGrid(
            columnCount: itemCount,
            position: index,
            duration: const Duration(milliseconds: 375),
            child: ScaleAnimation(
              scale: 0.5,
              child: FadeInAnimation(
                child: Container(
                  width: 227.r,
                  height: 246.r,
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(8.r),
                    color:  AppColors.shimmerBase,
                  ),
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}