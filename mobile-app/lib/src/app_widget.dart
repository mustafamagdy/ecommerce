import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'core/routes/app_router.gr.dart';
import 'presentation/components/components.dart';
import 'presentation/theme/theme.dart';
import 'riverpod/provider/app_provider.dart';

class AppWidget extends ConsumerWidget {
  AppWidget({Key? key}) : super(key: key);

  final appRouter = AppRouter();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(appProvider);
    return ScreenUtilInit(
      designSize: const Size(1194, 900),
      builder: (context, child) {
        return MaterialApp.router(
          scrollBehavior: CustomScrollBehavior(),
          debugShowCheckedModeBanner: false,
          routerDelegate: appRouter.delegate(),
          routeInformationParser: appRouter.defaultRouteParser(),
          locale: Locale(state.lang),
          color: AppColors.white,
          builder: (context, child) => ScrollConfiguration(
            behavior: MyBehavior(),
            child: child ?? const SizedBox.shrink(),
          ),
        );
      },
    );
  }
}

class MyBehavior extends ScrollBehavior {
  @override
  Widget buildOverscrollIndicator(
      BuildContext context, Widget child, ScrollableDetails details) {
    return child;
  }
}