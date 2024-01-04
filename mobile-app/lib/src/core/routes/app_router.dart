import 'package:admin_desktop/src/presentation/pages/auth/pin_code/pin_code_page.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/order_detail/order_detail.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/orders_table/orders_table.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/right_side/address/select_address_page.dart';
import 'package:auto_route/auto_route.dart';

import '../../presentation/pages/pages.dart';

@MaterialAutoRouter(
  replaceInRouteName: 'Page,Route',
  routes: [
    CustomRoute(
      path: '/',
      page: SplashPage,
      transitionsBuilder: TransitionsBuilders.fadeIn,
    ),
    CustomRoute(
      path: '/login',
      page: LoginPage,
      transitionsBuilder: TransitionsBuilders.fadeIn,
    ),
    CustomRoute(
      path: '/pin_code',
      page: PinCodePage,
      transitionsBuilder: TransitionsBuilders.fadeIn,
    ),
    CustomRoute(
      path: '/main',
      page: MainPage,
      transitionsBuilder: TransitionsBuilders.fadeIn,
    ),
    CustomRoute(
      path: '/select_address',
      page: SelectAddressPage,
      transitionsBuilder: TransitionsBuilders.fadeIn,
    ),
    CustomRoute(
      path: '/order_table',
      page: OrdersTablesPage,
      transitionsBuilder: TransitionsBuilders.fadeIn,
    ),
    CustomRoute(
      path: '/order_detail',
      page: OrderDetailPage,
      transitionsBuilder: TransitionsBuilders.fadeIn,
    ),
  ],
)
class $AppRouter {}
