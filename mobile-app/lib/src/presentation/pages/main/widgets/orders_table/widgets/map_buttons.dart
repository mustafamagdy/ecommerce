import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../../../components/buttons/animation_button_effect.dart';
import '../../../../../theme/theme.dart';

class MapButtons extends StatelessWidget {
  final VoidCallback zoomIn;
  final VoidCallback zoomOut;
  final VoidCallback navigate;

  const MapButtons(
      {Key? key,
      required this.zoomIn,
      required this.zoomOut,
      required this.navigate})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Container(
          height: 94.r,
          width: 50.r,
          decoration: BoxDecoration(
            boxShadow: const [
              BoxShadow(
                  color: AppColors.shadow, offset: Offset(0, 2), blurRadius: 2)
            ],
            borderRadius: BorderRadius.circular(5.r),
            color: AppColors.white,
          ),
          child: Column(
            children: [
              Expanded(
                child: AnimationButtonEffect(
                  child: GestureDetector(
                    onTap: zoomIn,
                    child: Icon(
                      Icons.add,
                      size: 24.r,
                      color: AppColors.black,
                    ),
                  ),
                ),
              ),
              Padding(
                padding: EdgeInsets.symmetric(horizontal: 6.r),
                child: const Divider(
                  color: AppColors.unselectedBottomBarItem,
                ),
              ),
              Expanded(
                child: AnimationButtonEffect(
                  child: GestureDetector(
                    onTap: zoomOut,
                    child: Icon(
                      Icons.remove,
                      size: 24.r,
                      color: AppColors.black,
                    ),
                  ),
                ),
              )
            ],
          ),
        ),
        12.verticalSpace,
        AnimationButtonEffect(
          child: InkWell(
            onTap: navigate,
            borderRadius: BorderRadius.circular(5.r),
            child: Container(
              height: 50.r,
              width: 50.r,
              decoration: BoxDecoration(
                boxShadow: const [
                  BoxShadow(
                    color: AppColors.shadow,
                    offset: Offset(0, 2),
                    blurRadius: 2,
                  )
                ],
                borderRadius: BorderRadius.circular(5.r),
                color: AppColors.white,
              ),
              child: Icon(FlutterRemix.navigation_line, size: 20.r),
            ),
          ),
        )
      ],
    );
  }
}
