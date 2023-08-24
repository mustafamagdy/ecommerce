import '../core/handlers/handlers.dart';
import '../models/models.dart';

abstract class CurrenciesRepository {
  Future<ApiResult<CurrenciesResponse>> getCurrencies();
}
