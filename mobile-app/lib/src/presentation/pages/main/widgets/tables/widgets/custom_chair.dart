import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../../../../core/constants/constants.dart';
import '../../../../../theme/app_colors.dart';

class CustomChair extends StatelessWidget {
  final ChairPosition chairPosition;
  final double borderRadiusSize;
  final double chairSpace;
  final double chairHeight;
  final double chairWidth;

  const CustomChair(
      {Key? key,
      required this.chairPosition,
      this.borderRadiusSize = 16,
      this.chairHeight = 24,
      this.chairWidth = 64,
      required this.chairSpace})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: chairPosition == ChairPosition.top ||
              chairPosition == ChairPosition.bottom
          ? EdgeInsets.only(left: chairSpace.r)
          : chairPosition == ChairPosition.left ||
                  chairPosition == ChairPosition.right
              ? EdgeInsets.only(top: chairSpace.r)
              : null,
      height: chairPosition == ChairPosition.top ||
              chairPosition == ChairPosition.bottom
          ? chairHeight.r
          : chairWidth.r,
      width: chairPosition == ChairPosition.top ||
              chairPosition == ChairPosition.bottom
          ? chairWidth.r
          : chairHeight.r,
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.only(
          topLeft: chairPosition == ChairPosition.top ||
                  chairPosition == ChairPosition.left
              ? Radius.circular(borderRadiusSize.r)
              : Radius.zero,
          topRight: chairPosition == ChairPosition.top ||
                  chairPosition == ChairPosition.right
              ? Radius.circular(borderRadiusSize.r)
              : Radius.zero,
          bottomLeft: chairPosition == ChairPosition.bottom ||
                  chairPosition == ChairPosition.left
              ? Radius.circular(borderRadiusSize.r)
              : Radius.zero,
          bottomRight: chairPosition == ChairPosition.bottom ||
                  chairPosition == ChairPosition.right
              ? Radius.circular(borderRadiusSize.r)
              : Radius.zero,
        ),
      ),
    );
  }
}

class VerticalChair extends StatelessWidget {
  final ChairPosition chairPosition;
  final double borderRadiusSize;
  final double chairSpace;
  final double chairHeight;

  const VerticalChair(
      {Key? key,
      required this.chairPosition,
      this.borderRadiusSize = 16,
      this.chairHeight = 24,
      required this.chairSpace})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: chairPosition == ChairPosition.top ||
              chairPosition == ChairPosition.bottom
          ? EdgeInsets.only(left: chairSpace.r)
          : chairPosition == ChairPosition.left ||
                  chairPosition == ChairPosition.right
              ? EdgeInsets.only(top: chairSpace.r)
              : null,
      height: chairPosition == ChairPosition.top ||
              chairPosition == ChairPosition.bottom
          ? chairHeight.r
          : null,
      width: chairPosition == ChairPosition.top ||
              chairPosition == ChairPosition.bottom
          ? null
          : chairHeight.r,
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.only(
          topLeft: chairPosition == ChairPosition.top ||
                  chairPosition == ChairPosition.left
              ? Radius.circular(borderRadiusSize.r)
              : Radius.zero,
          topRight: chairPosition == ChairPosition.top ||
                  chairPosition == ChairPosition.right
              ? Radius.circular(borderRadiusSize.r)
              : Radius.zero,
          bottomLeft: chairPosition == ChairPosition.bottom ||
                  chairPosition == ChairPosition.left
              ? Radius.circular(borderRadiusSize.r)
              : Radius.zero,
          bottomRight: chairPosition == ChairPosition.bottom ||
                  chairPosition == ChairPosition.right
              ? Radius.circular(borderRadiusSize.r)
              : Radius.zero,
        ),
      ),
    );
  }
}
