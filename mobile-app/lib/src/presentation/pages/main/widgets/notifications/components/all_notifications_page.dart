import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/core/utils/local_storage.dart';
import 'package:admin_desktop/src/presentation/pages/main/riverpod/provider/main_provider.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../components/common_image.dart';
import '../../../../../theme/app_colors.dart';
import '../riverpod/notification_provider.dart';

class AllNotificationsPage extends ConsumerWidget {
  final int index;

  const AllNotificationsPage(this.index, {super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(notificationProvider);
    final notifier = ref.read(notificationProvider.notifier);
    return Column(
      children: [
        8.verticalSpace,
        GestureDetector(
          onTap: () {
            if (LocalStorage.instance.getUser()?.role == "seller") {
              if(state.notifications[index].orderData != null){
                context.popRoute();
                if (state.notifications[index].readAt == null) {
                  notifier.readOne(
                      index: index, context, id: state.notifications[index].id);
                }
                ref
                    .read(mainProvider.notifier)
                    .setOrder(state.notifications[index].orderData);
                ref.read(mainProvider.notifier).changeIndex(1);
              }
              else{
                AppHelpers.showAlertDialog(context: context, child: Container(
                  width: 400.r,
                  height: 120.r,
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(10.r),
                    color: AppColors.white
                  ),
                  padding: EdgeInsets.all(16.r),
                  child:  Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      if(state.notifications[index].orderData != null)
                        CommonImage(
                        radius: 100,
                        imageUrl: state.notifications[index].client?.img,
                        height: 56,
                        width: 56,
                      ),
                      6.horizontalSpace,
                        Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          if(state.notifications[index].orderData != null)
                          Row(
                            children: [
                              Text(
                                '${state.notifications[index].client?.firstname ?? ''} ${state.notifications[index].client?.lastname?.substring(0, 1) ?? ''}.',
                                style: GoogleFonts.inter(
                                    fontWeight: FontWeight.w700,
                                    fontSize: 16.sp,
                                    color: AppColors.black),
                              ),
                              15.horizontalSpace,
                              Container(
                                height: 8.r,
                                width: 8.r,
                                decoration: BoxDecoration(
                                    shape: BoxShape.circle,
                                    color: state.notifications[index].readAt == null
                                        ? AppColors.brandColor
                                        : AppColors.transparent),
                              )
                            ],
                          ),
                          2.verticalSpace,
                          SizedBox(
                            width: 300.r,
                            child: Text(
                              '${state.notifications[index].body}',
                              overflow: TextOverflow.ellipsis,
                              style: GoogleFonts.inter(
                                  fontWeight: FontWeight.w400,
                                  fontSize: 14.sp,
                                  color: AppColors.black),
                              maxLines: 4,
                            ),
                          ),
                          8.verticalSpace,
                          Text(
                            '${state.notifications[index].createdAt}'
                                .substring(0, 16),
                            style: GoogleFonts.inter(
                                fontWeight: FontWeight.w400,
                                fontSize: 12.sp,
                                color: AppColors.iconColor),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),);
                if (state.notifications[index].readAt == null) {
                  notifier.readOne(
                      index: index, context, id: state.notifications[index].id);
                }
              }

            }
          },
          child: Container(
            color: AppColors.transparent,
            child:  Row(
              children: [
                if(state.notifications[index].orderData  != null)
                  CommonImage(
                    radius: 100,
                    imageUrl: state.notifications[index].client?.img,
                    height: 56,
                    width: 56,
                  ),
                6.horizontalSpace,
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    if(state.notifications[index].orderData != null)
                      Row(
                        children: [
                          Text(
                            '${state.notifications[index].client?.firstname ?? ''} ${state.notifications[index].client?.lastname?.substring(0, 1) ?? ''}.',
                            style: GoogleFonts.inter(
                                fontWeight: FontWeight.w700,
                                fontSize: 16.sp,
                                color: AppColors.black),
                          ),
                          15.horizontalSpace,
                          Container(
                            height: 8.r,
                            width: 8.r,
                            decoration: BoxDecoration(
                                shape: BoxShape.circle,
                                color: state.notifications[index].readAt == null
                                    ? AppColors.brandColor
                                    : AppColors.transparent),
                          )
                        ],
                      ),
                    2.verticalSpace,
                    SizedBox(
                      width: 300,
                      child: Text(
                        '${state.notifications[index].body}',
                        overflow: TextOverflow.ellipsis,
                        maxLines: 1,
                        style: GoogleFonts.inter(
                            fontWeight: FontWeight.w400,
                            fontSize: 14.sp,
                            color: AppColors.black),
                      ),
                    ),
                    8.verticalSpace,
                    Text(
                      '${state.notifications[index].createdAt}'
                          .substring(0, 16),
                      style: GoogleFonts.inter(
                          fontWeight: FontWeight.w400,
                          fontSize: 12.sp,
                          color: AppColors.iconColor),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
        8.verticalSpace,
        Row(
          children: [
            Expanded(
              child: Divider(
                color: AppColors.black.withOpacity(0.3),
              ),
            ),
          ],
        ),
      ],
    );
  }
}
