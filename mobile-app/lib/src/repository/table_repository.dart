import 'package:admin_desktop/src/models/response/table_status_response.dart';
import '../core/handlers/handlers.dart';

abstract class TableRepository {
  Future<ApiResult<TableResponse>> getTableInfo( {int? page});
}
