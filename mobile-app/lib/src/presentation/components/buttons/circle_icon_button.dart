import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

class CircleIconButton extends StatelessWidget {
  final Color backgroundColor;
  final Color iconColor;
  final IconData iconData;
  final int size;
  final Function() onTap;

  const CircleIconButton({
    Key? key,
    required this.backgroundColor,
    required this.iconData,
    required this.iconColor,
    required this.onTap,
    this.size = 40,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Material(
      borderRadius: BorderRadius.circular(50.r),
      color: backgroundColor,
      child: InkWell(
        borderRadius: BorderRadius.circular(50.r),
        onTap: onTap,
        child: Container(
          width: size.r,
          height: size.r,
          alignment: Alignment.center,
          child: Icon(
            iconData,
            size: (size / 2).r,
            color: iconColor,
          ),
        ),
      ),
    );
  }
}
