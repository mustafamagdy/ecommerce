// ignore_for_file: deprecated_member_use

import 'dart:async';

import 'package:admin_desktop/src/presentation/pages/main/riverpod/notifier/main_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/riverpod/state/main_state.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/customers/customers_page.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/customers/riverpod/notifier/customer_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/customers/riverpod/provider/customer_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/kitchen/kitchen_page.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/kitchen/riverpod/kitchen_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/notifications/components/notification_count_container.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/notifications/notification_dialog.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/canceled/canceled_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/cooking/cooking_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders/delivered/delivered_orders_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/notifications/riverpod/notification_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/post_page.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/tables/tables_page.dart';
import 'package:auto_route/auto_route.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:proste_indexed_stack/proste_indexed_stack.dart';
import 'package:slide_digital_clock/slide_digital_clock.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../../generated/assets.dart';
import '../../../core/constants/constants.dart';
import '../../../core/routes/app_router.gr.dart';
import '../../../core/utils/utils.dart';
import '../../components/components.dart';
import '../../theme/theme.dart';
import 'riverpod/provider/main_provider.dart';
import 'widgets/income/income_page.dart';
import 'widgets/orders_table/orders/accepted/accepted_orders_provider.dart';
import 'widgets/orders_table/orders/new/new_orders_provider.dart';
import 'widgets/orders_table/orders/on_a_way/on_a_way_orders_provider.dart';
import 'widgets/orders_table/orders/ready/ready_orders_provider.dart';
import 'widgets/orders_table/orders_table.dart';
import 'widgets/profile/edit_profile/edit_profile_page.dart';
import 'widgets/right_side/riverpod/provider/right_side_provider.dart';
import 'widgets/sale_history/sale_history.dart';
import 'dart:io' show Platform;

class MainPage extends ConsumerStatefulWidget {
  const MainPage({Key? key}) : super(key: key);

  @override
  ConsumerState<MainPage> createState() => _MainPageState();
}

class _MainPageState extends ConsumerState<MainPage>
    with SingleTickerProviderStateMixin {
  final user = LocalStorage.instance.getUser();

  late List<IndexedStackChild> list = [
    IndexedStackChild(child: const PostPage(), preload: true),
    IndexedStackChild(child: const OrdersTablesPage()),
    IndexedStackChild(child: const CustomersPage()),
    IndexedStackChild(child: const TablesPage()),
    IndexedStackChild(child: const SaleHistory()),
    IndexedStackChild(child: const InComePage()),
    IndexedStackChild(child: const ProfilePage()),
  ];

  late List<IndexedStackChild> listKitchen = [
    IndexedStackChild(child: const KitchenPage()),
    IndexedStackChild(child: const ProfilePage()),
  ];

  @override
  void initState() {
    super.initState();
    if (Platform.isAndroid || Platform.isIOS) {
      FirebaseMessaging.instance.requestPermission(
        sound: true,
        alert: true,
        badge: false,
      );
    }
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (user?.role == "seller") {
        ref.read(mainProvider.notifier)
          ..fetchProducts(
            checkYourNetwork: () {
              AppHelpers.showSnackBar(
                context,
                AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
              );
            },
          )
          ..fetchCategories(
            context: context,
            checkYourNetwork: () {
              AppHelpers.showSnackBar(
                context,
                AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
              );
            },
          )
          ..fetchUserDetail()
          ..changeIndex(0);
        ref.read(rightSideProvider.notifier).fetchUsers(
          checkYourNetwork: () {
            AppHelpers.showSnackBar(
              context,
              AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
            );
          },
        );
      } else {
        ref.read(mainProvider.notifier).fetchUserDetail();
      }

      Timer.periodic(
        AppConstants.refreshTime,
        (s) {
          ref.read(notificationProvider.notifier).fetchCount(context);
        },
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(mainProvider);
    final customerNotifier = ref.read(customerProvider.notifier);
    final notifier = ref.read(mainProvider.notifier);

    return SafeArea(
      child: KeyboardDismisser(
        child: Scaffold(
          resizeToAvoidBottomInset: false,
          appBar: customAppBar(notifier, customerNotifier),
          backgroundColor: AppColors.mainBack,
          body: Row(
            children: [
              user?.role == "seller"
                  ? bottomLeftNavigationBar(state)
                  : bottomLeftNavigationBarKitchen(state),
              Expanded(
                child: ProsteIndexedStack(
                  index: state.selectIndex,
                  children: user?.role == "seller" ? list : listKitchen,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  AppBar customAppBar(
      MainNotifier notifier, CustomerNotifier customerNotifier) {
    return AppBar(
      backgroundColor: AppColors.white,
      automaticallyImplyLeading: false,
      elevation: 0.5,
      title: IntrinsicHeight(
        child: Row(
          children: [
            16.horizontalSpace,
            SvgPicture.asset(Assets.svgLogo),
            12.horizontalSpace,
            Text(
              AppHelpers.getAppName() ?? "",
              style: GoogleFonts.inter(
                  color: AppColors.black, fontWeight: FontWeight.bold),
            ),
            const VerticalDivider(),
            30.horizontalSpace,
            Expanded(
              child: Row(
                children: [
                  Icon(
                    FlutterRemix.search_2_line,
                    size: 20.r,
                    color: AppColors.black,
                  ),
                  17.horizontalSpace,
                  Expanded(
                    flex: 2,
                    child: TextFormField(
                      onChanged: (value) {
                        if (user?.role == "seller") {
                          ref.watch(mainProvider).selectIndex == 2
                              ? customerNotifier.searchUsers(
                                  context, value.trim())
                              : notifier.setProductsQuery(
                                  context, value.trim());
                          if (ref.watch(mainProvider).selectIndex == 1) {
                            ref
                                .read(newOrdersProvider.notifier)
                                .setOrdersQuery(context, value.trim());
                            ref
                                .read(acceptedOrdersProvider.notifier)
                                .setOrdersQuery(context, value.trim());
                            ref
                                .read(readyOrdersProvider.notifier)
                                .setOrdersQuery(context, value.trim());
                            ref
                                .read(onAWayOrdersProvider.notifier)
                                .setOrdersQuery(context, value.trim());
                            ref
                                .read(deliveredOrdersProvider.notifier)
                                .setOrdersQuery(context, value.trim());
                            ref
                                .read(canceledOrdersProvider.notifier)
                                .setOrdersQuery(context, value.trim());
                          }
                        } else {
                          ref
                              .read(kitchenProvider.notifier)
                              .setOrdersQuery(context, value.trim());
                        }
                      },
                      cursorColor: AppColors.black,
                      cursorWidth: 1.r,
                      decoration: InputDecoration.collapsed(
                        hintText: ref.watch(mainProvider).selectIndex == 1
                            ? AppHelpers.getTranslation(TrKeys.searchOrders)
                            : ref.watch(mainProvider).selectIndex == 2
                                ? AppHelpers.getTranslation(
                                    TrKeys.searchCustomers)
                                : AppHelpers.getTranslation(
                                    TrKeys.searchProducts),
                        hintStyle: GoogleFonts.inter(
                          fontWeight: FontWeight.w500,
                          fontSize: 18.sp,
                          color: AppColors.searchHint.withOpacity(0.3),
                          letterSpacing: -14 * 0.02,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
            const VerticalDivider(),
            SizedBox(width: 120.w, child: DigitalClock()),
            const VerticalDivider(),
            IconButton(
                onPressed: () async {
                  await launch(
                  "${SecretVars.webUrl}/help",
                  forceSafariVC: true,
                  forceWebView: true,
                  enableJavaScript: true,
                  );
                },
                icon: const Icon(
                  FlutterRemix.question_line,
                  color: AppColors.black,
                )),
            // IconButton(
            //     onPressed: () {},
            //     icon: const Icon(
            //       FlutterRemix.settings_5_line,
            //       color: AppColors.black,
            //     )),
            IconButton(
                onPressed: () {
                  showDialog(
                      context: context,
                      builder: (_) => Row(
                            mainAxisAlignment: MainAxisAlignment.end,
                            children: const [
                              Dialog(
                                child: NotificationDialog(),
                              ),
                            ],
                          ));
                },
                icon: const Icon(
                  FlutterRemix.notification_2_line,
                  color: AppColors.black,
                )),
            NotificationCountsContainer(
                count:
                    '${ref.watch(notificationProvider).countOfNotifications?.notification ?? 0}')
          ],
        ),
      ),
    );
  }

  Container bottomLeftNavigationBar(MainState state) {
    return Container(
      height: double.infinity,
      width: 90.w,
      color: AppColors.white,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          24.verticalSpace,
          Container(
            decoration: BoxDecoration(
                color: state.selectIndex == 0
                    ? AppColors.brandColor
                    : AppColors.transparent,
                borderRadius: BorderRadius.circular(10.r)),
            child: IconButton(
                onPressed: () {
                  ref.read(mainProvider.notifier).changeIndex(0);
                },
                icon: Icon(
                  state.selectIndex == 0
                      ? FlutterRemix.home_smile_fill
                      : FlutterRemix.home_smile_line,
                  color: state.selectIndex == 0
                      ? AppColors.white
                      : AppColors.iconColor,
                )),
          ),
          28.verticalSpace,
          Container(
            decoration: BoxDecoration(
                color: state.selectIndex == 1
                    ? AppColors.brandColor
                    : AppColors.transparent,
                borderRadius: BorderRadius.circular(10.r)),
            child: IconButton(
                onPressed: () {
                  ref.read(mainProvider.notifier).changeIndex(1);
                },
                icon: Icon(
                  state.selectIndex == 1
                      ? FlutterRemix.shopping_bag_fill
                      : FlutterRemix.shopping_bag_line,
                  color: state.selectIndex == 1
                      ? AppColors.white
                      : AppColors.iconColor,
                )),
          ),
          28.verticalSpace,
          Container(
            decoration: BoxDecoration(
                color: state.selectIndex == 2
                    ? AppColors.brandColor
                    : AppColors.transparent,
                borderRadius: BorderRadius.circular(10.r)),
            child: IconButton(
                onPressed: () {
                  ref.read(mainProvider.notifier).changeIndex(2);
                },
                icon: Icon(
                  state.selectIndex == 2
                      ? FlutterRemix.user_3_fill
                      : FlutterRemix.user_3_line,
                  color: state.selectIndex == 2
                      ? AppColors.white
                      : AppColors.iconColor,
                )),
          ),
          28.verticalSpace,
          Container(
            decoration: BoxDecoration(
                color: state.selectIndex == 3
                    ? AppColors.brandColor
                    : AppColors.transparent,
                borderRadius: BorderRadius.circular(10.r)),
            child: IconButton(
              onPressed: () {
                ref.read(mainProvider.notifier).changeIndex(3);
              },
              icon: SvgPicture.asset(
                state.selectIndex == 3
                    ? Assets.svgSelectTable
                    : Assets.svgTable,
              ),
            ),
          ),
          28.verticalSpace,
          Container(
            decoration: BoxDecoration(
                color: state.selectIndex == 4
                    ? AppColors.brandColor
                    : AppColors.transparent,
                borderRadius: BorderRadius.circular(10.r)),
            child: IconButton(
                onPressed: () {
                  ref.read(mainProvider.notifier).changeIndex(4);
                },
                icon: Icon(
                  state.selectIndex == 4
                      ? FlutterRemix.money_dollar_circle_fill
                      : FlutterRemix.money_dollar_circle_line,
                  color: state.selectIndex == 4
                      ? AppColors.white
                      : AppColors.iconColor,
                )),
          ),
          28.verticalSpace,
          Container(
            decoration: BoxDecoration(
                color: state.selectIndex == 5
                    ? AppColors.brandColor
                    : AppColors.transparent,
                borderRadius: BorderRadius.circular(10.r)),
            child: IconButton(
                onPressed: () {
                  ref.read(mainProvider.notifier).changeIndex(5);
                },
                icon: Icon(
                  state.selectIndex == 5
                      ? FlutterRemix.pie_chart_fill
                      : FlutterRemix.pie_chart_line,
                  color: state.selectIndex == 5
                      ? AppColors.white
                      : AppColors.iconColor,
                )),
          ),
          const Spacer(),
          InkWell(
            onTap: () {
              ref.read(mainProvider.notifier).changeIndex(6);
            },
            child: Container(
              decoration: BoxDecoration(
                  border: Border.all(
                    color: state.selectIndex == 6
                        ? AppColors.brandColor
                        : AppColors.transparent,
                  ),
                  borderRadius: BorderRadius.circular(20.r)),
              child: CommonImage(
                  width: 40,
                  height: 40,
                  radius: 20,
                  imageUrl: LocalStorage.instance.getUser()?.img ?? ""),
            ),
          ),
          24.verticalSpace,
          IconButton(
              onPressed: () {
                context.replaceRoute(const LoginRoute());
                ref.read(newOrdersProvider.notifier).stopTimer();
                ref.read(acceptedOrdersProvider.notifier).stopTimer();
                ref.read(cookingOrdersProvider.notifier).stopTimer();
                ref.read(readyOrdersProvider.notifier).stopTimer();
                ref.read(onAWayOrdersProvider.notifier).stopTimer();
                ref.read(deliveredOrdersProvider.notifier).stopTimer();
                ref.read(canceledOrdersProvider.notifier).stopTimer();
                LocalStorage.instance.clearStore();
              },
              icon: const Icon(
                FlutterRemix.logout_circle_line,
                color: AppColors.iconColor,
              )),
          32.verticalSpace
        ],
      ),
    );
  }

  Container bottomLeftNavigationBarKitchen(MainState state) {
    return Container(
      height: double.infinity,
      width: 90.w,
      color: AppColors.white,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          28.verticalSpace,
          Container(
            decoration: BoxDecoration(
                color: state.selectIndex == 0
                    ? AppColors.brandColor
                    : AppColors.transparent,
                borderRadius: BorderRadius.circular(10.r)),
            child: IconButton(
              onPressed: () {
                ref.read(mainProvider.notifier).changeIndex(0);
              },
              icon: SvgPicture.asset(
                state.selectIndex == 0
                    ? Assets.svgSelectKitchen
                    : Assets.svgKitchen,
              ),
            ),
          ),
          const Spacer(),
          InkWell(
            onTap: () {
              ref.read(mainProvider.notifier).changeIndex(1);
            },
            child: Container(
              decoration: BoxDecoration(
                  border: Border.all(
                    color: state.selectIndex == 1
                        ? AppColors.brandColor
                        : AppColors.transparent,
                  ),
                  borderRadius: BorderRadius.circular(20.r)),
              child: CommonImage(
                  width: 40,
                  height: 40,
                  radius: 20,
                  imageUrl: LocalStorage.instance.getUser()?.img ?? ""),
            ),
          ),
          24.verticalSpace,
          IconButton(
              onPressed: () {
                context.replaceRoute(const LoginRoute());
                ref.read(kitchenProvider.notifier).stopTimer();
                LocalStorage.instance.clearStore();
              },
              icon: const Icon(
                FlutterRemix.logout_circle_line,
                color: AppColors.iconColor,
              )),
          32.verticalSpace
        ],
      ),
    );
  }
}
