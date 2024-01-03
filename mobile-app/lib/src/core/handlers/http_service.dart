import 'package:admin_desktop/src/core/handlers/tenant_interceptor.dart';
import 'package:admin_desktop/src/core/utils/local_storage.dart';
import 'package:dio/dio.dart';

import '../constants/constants.dart';
import 'token_interceptor.dart';

class HttpService {

  Dio client({bool requireAuth = false}) => Dio(
    BaseOptions(
      baseUrl: SecretVars.baseUrl,
      connectTimeout: 60 * 1000,
      receiveTimeout: 60 * 1000,
      sendTimeout: 60 * 1000,
      headers: {
        'Accept': 'application/json',
        'Content-type': 'application/json',
      },
    ),
  )
    ..interceptors.add(TokenInterceptor(requireAuth: requireAuth))
    ..interceptors.add(TenantInterceptor(requireAuth: requireAuth))
    ..interceptors.add(LogInterceptor(responseBody: true,requestBody: true));
}
