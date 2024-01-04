import 'package:admin_desktop/src/models/data/tenant_data.dart';
import 'package:dio/dio.dart';

import '../utils/utils.dart';

class TenantInterceptor extends Interceptor {
  final bool requireAuth;

  TenantInterceptor({required this.requireAuth});

  @override
  void onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final TenantData tenant = LocalStorage.instance.getTenant()!;
    if (tenant != null) {
      options.headers.addAll({'tenant': tenant.id});
    }
    handler.next(options);
  }
}
