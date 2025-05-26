import 'package:admin_desktop/src/presentation/components/shimmers/lines_shimmer.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/notifications/components/all_notifications_page.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/notifications/components/notification_count_container.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/notifications/components/view_more_button.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/notifications/riverpod/notification_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../core/constants/constants.dart';
import '../../../../../core/utils/app_helpers.dart';
import '../../../../theme/app_colors.dart';

class NotificationDialog extends ConsumerStatefulWidget {
  const NotificationDialog({super.key});

  @override
  ConsumerState<ConsumerStatefulWidget> createState() =>
      _NotificationDialogState();
}

class _NotificationDialogState extends ConsumerState<NotificationDialog>
    with SingleTickerProviderStateMixin {
  late TabController _controller;

  @override
  void initState() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (ref.read(notificationProvider).isFirstTimeNotification == false) {
        ref.read(notificationProvider.notifier)
          ..fetchAllTransactions()
          ..fetchNotificationsPaginate()
          ..changeFirst();
      }
    });

    _controller = TabController(length: 3, vsync: this);

    super.initState();
  }

  @override
  void dispose() {
    _controller.dispose();

    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(notificationProvider);
    final notifier = ref.read(notificationProvider.notifier);

    return Container(
      width: 446.w,
      decoration: BoxDecoration(borderRadius: BorderRadius.circular(10)),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 30),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            30.verticalSpace,
            Row(
              children: [
                Text(
                  AppHelpers.getTranslation(TrKeys.notifications),
                  style: GoogleFonts.inter(
                      fontWeight: FontWeight.w600,
                      fontSize: 22.sp,
                      color: AppColors.black),
                ),
                const Spacer(),
                IconButton(
                    onPressed: () {
                      Navigator.pop(context);
                    },
                    icon: const Icon(FlutterRemix.close_fill))
              ],
            ),
            17.verticalSpace,
            TabBar(
                isScrollable: true,
                unselectedLabelColor: AppColors.iconColor,
                labelPadding: const EdgeInsets.only(
                  left: 20,
                  right: 20,
                  bottom: 15,
                ),
                indicatorPadding: const EdgeInsets.symmetric(horizontal: 16),
                indicatorColor: AppColors.black,
                labelColor: AppColors.black,
                labelStyle: TextStyle(
                  fontWeight: FontWeight.w500,
                  fontSize: 16.sp,
                ),
                controller: _controller,
                tabs: [
                  Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        AppHelpers.getTranslation(TrKeys.all),
                      ),
                      8.horizontalSpace,
                      NotificationCountsContainer(
                        count: '${state.countOfNotifications?.notification}',
                      )
                    ],
                  ),
                  Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        AppHelpers.getTranslation(TrKeys.transactions),
                      ),
                    ],
                  ),
                  Text(
                    AppHelpers.getTranslation(TrKeys.messages),
                  )
                ]),
            Expanded(
              child: TabBarView(
                  physics: const NeverScrollableScrollPhysics(),
                  controller: _controller,
                  children: [
                    state.isNotificationLoading
                        ? const Center(
                            child: CircularProgressIndicator(
                            color: AppColors.black,
                          ))
                        : state.notifications.isNotEmpty
                            ? ListView(
                                children: [
                                  26.verticalSpace,
                                  ListView.builder(
                                    physics:
                                        const NeverScrollableScrollPhysics(),
                                    shrinkWrap: true,
                                    itemCount: state.notifications.length,
                                    itemBuilder:
                                        (BuildContext context, int index) {
                                      return AllNotificationsPage(index);
                                    },
                                  ),
                                  4.verticalSpace,
                                  state.isMoreNotificationLoading
                                      ? const LineShimmer(
                                          isActiveLine: true,
                                        )
                                      : state.hasMoreNotification
                                          ? ViewMoreButton(
                                              onTap: () {
                                                return notifier
                                                    .fetchNotificationsPaginate();
                                              },
                                            )
                                          : const SizedBox(),
                                  25.verticalSpace,
                                  if (state.notifications.isNotEmpty)
                                    Row(
                                      children: [
                                        const Icon(
                                            FlutterRemix.check_double_fill),
                                        5.horizontalSpace,
                                        TextButton(
                                          style: const ButtonStyle(
                                              overlayColor:
                                                  MaterialStatePropertyAll(
                                                      AppColors.brandColor)),
                                          onPressed: () {
                                            notifier.readAll(context);
                                          },
                                          child: Text(
                                            AppHelpers.getTranslation(
                                                TrKeys.readAll),
                                            style: GoogleFonts.inter(
                                                fontWeight: FontWeight.w500,
                                                fontSize: 14.sp,
                                                color: AppColors.black),
                                          ),
                                        ),
                                      ],
                                    ),
                                ],
                              )
                            : Center(
                                child: Text(
                                  AppHelpers.getTranslation(
                                      TrKeys.noNotification),
                                ),
                              ),
                    Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            AppHelpers.getTranslation(TrKeys.transactions),
                          ),
                          16.verticalSpace,
                          const Text(
                            "Coming soon",
                          ),
                        ],
                      ),
                    ),
                    Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            AppHelpers.getTranslation(TrKeys.messages),
                          ),
                          16.verticalSpace,
                          const Text(
                            "Coming soon",
                          ),
                        ],
                      ),
                    )
                  ]),
            )
          ],
        ),
      ),
    );
  }
}
