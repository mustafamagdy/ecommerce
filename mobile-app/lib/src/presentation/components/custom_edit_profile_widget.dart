import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../theme/app_colors.dart';
import 'common_image.dart';

class CustomEditWidget extends StatelessWidget {
  final bool isEmptyorNot;
  final bool isEmptyorNot2;
  final String image;
  final String localStoreImage;
  final String imagePath;
  final Function()? onthisTap;
  const CustomEditWidget({
    super.key,
    required this.isEmptyorNot,
    required this.image,
    required this.isEmptyorNot2,
    required this.imagePath,
    required this.localStoreImage,
    this.onthisTap,
  });

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Container(
          height: 108.r,
          width: 108.r,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(100.r),
            color: AppColors.shimmerBase,
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(100.r),
            child: (isEmptyorNot)
                ? CommonImage(
                    imageUrl: localStoreImage,
                    height: 108.r,
                    width: 108.r,
                    radius: 100.r)
                : isEmptyorNot2
                    ? Image.file(
                        File(imagePath),
                        width: 108.r,
                        height: 108.r,
                      )
                    : CommonImage(
                        imageUrl: image,
                        height: 108.r,
                        width: 108.r,
                        radius: 100.r),
          ),
        ),
        Positioned(
          right: 0,
          bottom: 0,
          child: GestureDetector(
            onTap: onthisTap,
            child: Container(
              width: 40.w,
              height: 40.h,
              decoration: const BoxDecoration(
                color: AppColors.editProfileCircle,
                shape: BoxShape.circle,
              ),
              child: const Icon(
                FlutterRemix.pencil_line,
                size: 14,
              ),
            ),
          ),
        ),
      ],
    );
  }
}
