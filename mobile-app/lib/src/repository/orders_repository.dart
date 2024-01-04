import 'package:admin_desktop/src/models/response/orders_paginate_response.dart';
import 'package:admin_desktop/src/models/response/single_order_response.dart';

import '../core/constants/app_constants.dart';
import '../core/handlers/handlers.dart';
import '../models/models.dart';

abstract class OrdersRepository {
  Future<ApiResult<CreateOrderResponse>> createOrder(OrderBodyData orderBody);

  Future<ApiResult<OrdersPaginateResponse>> getOrders({
    OrderStatus? status,
    int? page,
    DateTime? from,
    DateTime? to,
    String? search,
  });

  Future<ApiResult<dynamic>> updateOrderStatus({
    required OrderStatus status,
    int? orderId,
  });

  Future<ApiResult<dynamic>> updateOrderStatusKitchen({
    required OrderStatus status,
    int? orderId,
  });

  Future<ApiResult<SingleOrderResponse>> getOrderDetails({int? orderId});

  Future<ApiResult<SingleOrderResponse>> getOrderDetailsKitchen({int? orderId});

  Future<ApiResult<dynamic>> setDeliverMan({required int orderId,required int deliverymanId});

  Future<ApiResult<dynamic>> deleteOrder({required int orderId});

  Future<ApiResult<OrderKitchenResponseData>> getKitchenOrders({
    String? status,
    int? page,
    String? search,
  });

}
