
import 'package:admin_desktop/src/presentation/components/buttons/animation_button_effect.dart';
import 'package:flutter/material.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../theme/app_colors.dart';


class PopButton extends StatelessWidget {


  const PopButton({Key? key,}) : super(key: key);

  @override
  Widget build(BuildContext context) {

    return AnimationButtonEffect(
      child: GestureDetector(
        onTap: context.popRoute,
        child: Container(
          width: 56.r,
          height: 56.r,
          decoration: BoxDecoration(
            color: AppColors.black,
            borderRadius: BorderRadius.circular(10.r),
          ),
          alignment: Alignment.center,
          child: Icon(
           FlutterRemix.arrow_left_s_line,
            color: AppColors.white,
            size: 20.r,
          ),
        ),
      ),
    );
  }
}
