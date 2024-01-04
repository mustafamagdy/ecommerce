

import 'package:admin_desktop/src/models/response/orders_paginate_response.dart';
import 'package:admin_desktop/src/models/response/single_order_response.dart';
import 'package:flutter/material.dart';

import '../../core/constants/constants.dart';
import '../../core/di/injection.dart';
import '../../core/handlers/handlers.dart';
import '../../core/utils/local_storage.dart';
import '../../models/models.dart';
import '../repository.dart';

class OrdersRepositoryImpl extends OrdersRepository {
  @override
  Future<ApiResult<CreateOrderResponse>> createOrder(
      OrderBodyData orderBody) async {
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.post(
        '/api/v1/dashboard/seller/orders',
        data: orderBody.toJson(),
      );

      return ApiResult.success(
        data: CreateOrderResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> order create failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<OrderKitchenResponseData>> getKitchenOrders({
    String? status,
    int? page,
    DateTime? from,
    DateTime? to,
    String? search,
  }) async {
    final data = {
      if (page != null) 'page': page,
      if (status != null && TrKeys.all != status)
        'status': TrKeys.newKey == status
            ? "accepted"
            : TrKeys.done == status
                ? "ready"
                : TrKeys.cancel == status
                    ? "canceled"
                    : status,
      if (TrKeys.all == status)
        'statuses[0]': "accepted",
      if (TrKeys.all == status)
        'statuses[1]': "ready",
      if (TrKeys.all == status)
        'statuses[2]': "cooking",
      if (search != null) 'search': search,
      'perPage': 6,
      "empty-cook": 1,
      'lang': LocalStorage.instance.getActiveLocale(),
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/cook/orders/paginate',
        queryParameters: data,
      );
      return ApiResult.success(
        data: OrderKitchenResponseData.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get order $status failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<OrdersPaginateResponse>> getOrders({
    OrderStatus? status,
    int? page,
    DateTime? from,
    DateTime? to,
    String? search,
  }) async {
    String? statusText;
    switch (status) {
      case OrderStatus.accepted:
        statusText = 'accepted';
        break;
      case OrderStatus.ready:
        statusText = 'ready';
        break;
      case OrderStatus.onAWay:
        statusText = 'on_a_way';
        break;
      case OrderStatus.delivered:
        statusText = 'delivered';
        break;
      case OrderStatus.canceled:
        statusText = 'canceled';
        break;
      case OrderStatus.newOrder:
        statusText = 'new';
        break;
      case OrderStatus.cooking:
        statusText = 'cooking';
        break;
      default:
        statusText = null;
        break;
    }
    final data = {
      if (page != null) 'page': page,
      if (statusText != null) 'status': statusText,
      if (from != null)
        "date_from": from.toString().substring(0, from.toString().indexOf(" ")),
      if (to != null)
        "date_to": to.toString().substring(0, to.toString().indexOf(" ")),
      if (search != null) 'search': search,
      'perPage': to == null ? 7 : 15,
      'lang': LocalStorage.instance.getActiveLocale(),
    };
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final response = await client.get(
        '/api/v1/dashboard/seller/orders/paginate',
        queryParameters: data,
      );
      return ApiResult.success(
        data: OrdersPaginateResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get order $status failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<dynamic>> updateOrderStatus({
    required OrderStatus status,
    int? orderId,
  }) async {
    String? statusText;
    switch (status) {
      case OrderStatus.newOrder:
        statusText = 'new';
        break;
      case OrderStatus.accepted:
        statusText = 'accepted';
        break;
      case OrderStatus.ready:
        statusText = 'ready';
        break;
      case OrderStatus.onAWay:
        statusText = 'on_a_way';
        break;
      case OrderStatus.delivered:
        statusText = 'delivered';
        break;
      case OrderStatus.canceled:
        statusText = 'canceled';
        break;
      case OrderStatus.cooking:
        statusText = 'cooking';
        break;
    }

    final data = {'status': statusText};
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      await client.post(
        '/api/v1/dashboard/seller/order/$orderId/status',
        data: data,
      );
      return const ApiResult.success(
        data: null,
      );
    } catch (e) {
      debugPrint('==> update order status failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<dynamic>> updateOrderStatusKitchen({
    required OrderStatus status,
    int? orderId,
  }) async {
    String? statusText;
    switch (status) {
      case OrderStatus.newOrder:
        statusText = 'new';
        break;
      case OrderStatus.accepted:
        statusText = 'accepted';
        break;
      case OrderStatus.cooking:
        statusText = 'cooking';
        break;
      case OrderStatus.ready:
        statusText = 'ready';
        break;
      case OrderStatus.onAWay:
        statusText = 'on_a_way';
        break;
      case OrderStatus.delivered:
        statusText = 'delivered';
        break;
      case OrderStatus.canceled:
        statusText = 'canceled';
        break;
    }

    final data = {'status': statusText};
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      await client.post(
        '/api/v1/dashboard/cook/orders/$orderId/status/update',
        data: data,
      );
      return const ApiResult.success(
        data: null,
      );
    } catch (e) {
      debugPrint('==> update order status failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<SingleOrderResponse>> getOrderDetails({int? orderId}) async {
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final data = {
        'lang': LocalStorage.instance.getActiveLocale(),
      };
      final response = await client.get(
          '/api/v1/dashboard/seller/orders/$orderId',
          queryParameters: data);
      return ApiResult.success(
        data: SingleOrderResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get order details failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<SingleOrderResponse>> getOrderDetailsKitchen(
      {int? orderId}) async {
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final data = {
        'lang': LocalStorage.instance.getActiveLocale(),
      };
      final response = await client
          .get('/api/v1/dashboard/cook/orders/$orderId', queryParameters: data);
      return ApiResult.success(
        data: SingleOrderResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get order details failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult<dynamic>> setDeliverMan(
      {required int orderId, required int deliverymanId}) async {
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      final data = {
        'deliveryman': deliverymanId,
      };
      final response = await client.post(
          '/api/v1/dashboard/seller/order/$orderId/deliveryman',
          data: data);
      return ApiResult.success(
        data: SingleOrderResponse.fromJson(response.data),
      );
    } catch (e) {
      debugPrint('==> get order details failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }

  @override
  Future<ApiResult> deleteOrder({required int orderId}) async {
    final data = {'ids[0]': orderId};
    try {
      final client = inject<HttpService>().client(requireAuth: true);
      await client.delete(
        '/api/v1/dashboard/seller/orders/delete',
        queryParameters: data,
      );
      return const ApiResult.success(
        data: null,
      );
    } catch (e) {
      debugPrint('==> update order status failure: $e');
      return ApiResult.failure(error: NetworkExceptions.getDioException(e));
    }
  }
}
