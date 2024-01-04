// **************************************************************************
// AutoRouteGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND

// **************************************************************************
// AutoRouteGenerator
// **************************************************************************
//
// ignore_for_file: type=lint

// ignore_for_file: no_leading_underscores_for_library_prefixes
import 'package:auto_route/auto_route.dart' as _i6;
import 'package:flutter/material.dart' as _i7;

import '../../models/data/address_data.dart' as _i8;
import '../../models/data/order_data.dart' as _i9;
import '../../presentation/pages/auth/pin_code/pin_code_page.dart' as _i2;
import '../../presentation/pages/main/widgets/order_detail/order_detail.dart'
    as _i5;
import '../../presentation/pages/main/widgets/orders_table/orders_table.dart'
    as _i4;
import '../../presentation/pages/main/widgets/right_side/address/select_address_page.dart'
    as _i3;
import '../../presentation/pages/pages.dart' as _i1;

class AppRouter extends _i6.RootStackRouter {
  AppRouter([_i7.GlobalKey<_i7.NavigatorState>? navigatorKey])
      : super(navigatorKey);

  @override
  final Map<String, _i6.PageFactory> pagesMap = {
    SplashRoute.name: (routeData) {
      return _i6.CustomPage<dynamic>(
        routeData: routeData,
        child: const _i1.SplashPage(),
        transitionsBuilder: _i6.TransitionsBuilders.fadeIn,
        opaque: true,
        barrierDismissible: false,
      );
    },
    LoginRoute.name: (routeData) {
      return _i6.CustomPage<dynamic>(
        routeData: routeData,
        child: const _i1.LoginPage(),
        transitionsBuilder: _i6.TransitionsBuilders.fadeIn,
        opaque: true,
        barrierDismissible: false,
      );
    },
    PinCodeRoute.name: (routeData) {
      final args = routeData.argsAs<PinCodeRouteArgs>();
      return _i6.CustomPage<dynamic>(
        routeData: routeData,
        child: _i2.PinCodePage(
          args.isNewPassword,
          key: args.key,
        ),
        transitionsBuilder: _i6.TransitionsBuilders.fadeIn,
        opaque: true,
        barrierDismissible: false,
      );
    },
    MainRoute.name: (routeData) {
      return _i6.CustomPage<dynamic>(
        routeData: routeData,
        child: const _i1.MainPage(),
        transitionsBuilder: _i6.TransitionsBuilders.fadeIn,
        opaque: true,
        barrierDismissible: false,
      );
    },
    SelectAddressRoute.name: (routeData) {
      final args = routeData.argsAs<SelectAddressRouteArgs>();
      return _i6.CustomPage<dynamic>(
        routeData: routeData,
        child: _i3.SelectAddressPage(
          key: args.key,
          onSelect: args.onSelect,
        ),
        transitionsBuilder: _i6.TransitionsBuilders.fadeIn,
        opaque: true,
        barrierDismissible: false,
      );
    },
    OrdersTablesRoute.name: (routeData) {
      return _i6.CustomPage<dynamic>(
        routeData: routeData,
        child: const _i4.OrdersTablesPage(),
        transitionsBuilder: _i6.TransitionsBuilders.fadeIn,
        opaque: true,
        barrierDismissible: false,
      );
    },
    OrderDetailRoute.name: (routeData) {
      final args = routeData.argsAs<OrderDetailRouteArgs>();
      return _i6.CustomPage<dynamic>(
        routeData: routeData,
        child: _i5.OrderDetailPage(
          key: args.key,
          order: args.order,
        ),
        transitionsBuilder: _i6.TransitionsBuilders.fadeIn,
        opaque: true,
        barrierDismissible: false,
      );
    },
  };

  @override
  List<_i6.RouteConfig> get routes => [
        _i6.RouteConfig(
          SplashRoute.name,
          path: '/',
        ),
        _i6.RouteConfig(
          LoginRoute.name,
          path: '/login',
        ),
        _i6.RouteConfig(
          PinCodeRoute.name,
          path: '/pin_code',
        ),
        _i6.RouteConfig(
          MainRoute.name,
          path: '/main',
        ),
        _i6.RouteConfig(
          SelectAddressRoute.name,
          path: '/select_address',
        ),
        _i6.RouteConfig(
          OrdersTablesRoute.name,
          path: '/order_table',
        ),
        _i6.RouteConfig(
          OrderDetailRoute.name,
          path: '/order_detail',
        ),
      ];
}

/// generated route for
/// [_i1.SplashPage]
class SplashRoute extends _i6.PageRouteInfo<void> {
  const SplashRoute()
      : super(
          SplashRoute.name,
          path: '/',
        );

  static const String name = 'SplashRoute';
}

/// generated route for
/// [_i1.LoginPage]
class LoginRoute extends _i6.PageRouteInfo<void> {
  const LoginRoute()
      : super(
          LoginRoute.name,
          path: '/login',
        );

  static const String name = 'LoginRoute';
}

/// generated route for
/// [_i2.PinCodePage]
class PinCodeRoute extends _i6.PageRouteInfo<PinCodeRouteArgs> {
  PinCodeRoute({
    required bool isNewPassword,
    _i7.Key? key,
  }) : super(
          PinCodeRoute.name,
          path: '/pin_code',
          args: PinCodeRouteArgs(
            isNewPassword: isNewPassword,
            key: key,
          ),
        );

  static const String name = 'PinCodeRoute';
}

class PinCodeRouteArgs {
  const PinCodeRouteArgs({
    required this.isNewPassword,
    this.key,
  });

  final bool isNewPassword;

  final _i7.Key? key;

  @override
  String toString() {
    return 'PinCodeRouteArgs{isNewPassword: $isNewPassword, key: $key}';
  }
}

/// generated route for
/// [_i1.MainPage]
class MainRoute extends _i6.PageRouteInfo<void> {
  const MainRoute()
      : super(
          MainRoute.name,
          path: '/main',
        );

  static const String name = 'MainRoute';
}

/// generated route for
/// [_i3.SelectAddressPage]
class SelectAddressRoute extends _i6.PageRouteInfo<SelectAddressRouteArgs> {
  SelectAddressRoute({
    _i7.Key? key,
    required void Function(_i8.AddressData) onSelect,
  }) : super(
          SelectAddressRoute.name,
          path: '/select_address',
          args: SelectAddressRouteArgs(
            key: key,
            onSelect: onSelect,
          ),
        );

  static const String name = 'SelectAddressRoute';
}

class SelectAddressRouteArgs {
  const SelectAddressRouteArgs({
    this.key,
    required this.onSelect,
  });

  final _i7.Key? key;

  final void Function(_i8.AddressData) onSelect;

  @override
  String toString() {
    return 'SelectAddressRouteArgs{key: $key, onSelect: $onSelect}';
  }
}

/// generated route for
/// [_i4.OrdersTablesPage]
class OrdersTablesRoute extends _i6.PageRouteInfo<void> {
  const OrdersTablesRoute()
      : super(
          OrdersTablesRoute.name,
          path: '/order_table',
        );

  static const String name = 'OrdersTablesRoute';
}

/// generated route for
/// [_i5.OrderDetailPage]
class OrderDetailRoute extends _i6.PageRouteInfo<OrderDetailRouteArgs> {
  OrderDetailRoute({
    _i7.Key? key,
    required _i9.OrderData order,
  }) : super(
          OrderDetailRoute.name,
          path: '/order_detail',
          args: OrderDetailRouteArgs(
            key: key,
            order: order,
          ),
        );

  static const String name = 'OrderDetailRoute';
}

class OrderDetailRouteArgs {
  const OrderDetailRouteArgs({
    this.key,
    required this.order,
  });

  final _i7.Key? key;

  final _i9.OrderData order;

  @override
  String toString() {
    return 'OrderDetailRouteArgs{key: $key, order: $order}';
  }
}
