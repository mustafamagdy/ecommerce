import '../core/handlers/handlers.dart';
import '../models/models.dart';

abstract class AuthRepository {
  Future<ApiResult<LoginResponse>> login({
    required String email,
    required String password,
  });

  Future<ApiResult<void>> updateFirebaseToken(String? token);
}
