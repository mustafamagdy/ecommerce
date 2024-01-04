import 'package:admin_desktop/src/models/data/order_data.dart';

class OrdersPaginateResponse {
  OrdersPaginateResponse({OrderResponseData? data}) {
    _data = data;
  }

  OrdersPaginateResponse.fromJson(dynamic json) {
    _data =
        json['data'] != null ? OrderResponseData.fromJson(json['data']) : null;
  }

  OrderResponseData? _data;

  OrdersPaginateResponse copyWith({OrderResponseData? data}) =>
      OrdersPaginateResponse(data: data ?? _data);

  OrderResponseData? get data => _data;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    if (_data != null) {
      map['data'] = _data?.toJson();
    }
    return map;
  }
}

class OrderKitchenResponseData {
  OrderKitchenResponseData({
    List<OrderData>? orders,
  }) {
    _orders = orders;
  }

  OrderKitchenResponseData.fromJson(dynamic json) {
    if (json['data'] != null) {
      _orders = [];
      json['data'].forEach((v) {
        _orders?.add(OrderData.fromJson(v));
      });
    }
  }

  List<OrderData>? _orders;

  OrderResponseData copyWith({
    List<OrderData>? orders,
  }) =>
      OrderResponseData(
        orders: orders ?? _orders,
      );

  List<OrderData>? get orders => _orders;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    if (_orders != null) {
      map['data'] = _orders?.map((v) => v.toJson()).toList();
    }
    return map;
  }
}

class OrderResponseData {
  OrderResponseData({
    OrdersStatistic? statistic,
    List<OrderData>? orders,
  }) {
    _statistic = statistic;
    _orders = orders;
  }

  OrderResponseData.fromJson(dynamic json) {
    _statistic = json['statistic'] != null
        ? OrdersStatistic.fromJson(json['statistic'])
        : null;
    if (json['orders'] != null) {
      _orders = [];
      json['orders'].forEach((v) {
        _orders?.add(OrderData.fromJson(v));
      });
    }
  }

  OrdersStatistic? _statistic;
  List<OrderData>? _orders;

  OrderResponseData copyWith({
    OrdersStatistic? statistic,
    List<OrderData>? orders,
  }) =>
      OrderResponseData(
        statistic: statistic ?? _statistic,
        orders: orders ?? _orders,
      );

  OrdersStatistic? get statistic => _statistic;

  List<OrderData>? get orders => _orders;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    if (_statistic != null) {
      map['statistic'] = _statistic?.toJson();
    }
    if (_orders != null) {
      map['orders'] = _orders?.map((v) => v.toJson()).toList();
    }
    return map;
  }
}

class OrdersStatistic {
  OrdersStatistic({
    int? progressOrdersCount,
    int? deliveredOrdersCount,
    int? cancelOrdersCount,
    int? newOrdersCount,
    int? acceptedOrdersCount,
    int? cookingOrdersCount,
    int? readyOrdersCount,
    int? onAWayOrdersCount,
    int? ordersCount,
    num? totalPrice,
    int? todayCount,
  }) {
    _progressOrdersCount = progressOrdersCount;
    _deliveredOrdersCount = deliveredOrdersCount;
    _cancelOrdersCount = cancelOrdersCount;
    _newOrdersCount = newOrdersCount;
    _cookingOrdersCount = cookingOrdersCount;
    _acceptedOrdersCount = acceptedOrdersCount;
    _readyOrdersCount = readyOrdersCount;
    _onAWayOrdersCount = onAWayOrdersCount;
    _ordersCount = ordersCount;
    _totalPrice = totalPrice;
    _todayCount = todayCount;
  }

  OrdersStatistic.fromJson(dynamic json) {
    _progressOrdersCount = json['progress_orders_count'];
    _deliveredOrdersCount = json['delivered_orders_count'];
    _cancelOrdersCount = json['cancel_orders_count'];
    _newOrdersCount = json['new_orders_count'];
    _acceptedOrdersCount = json['accepted_orders_count'];
    _readyOrdersCount = json['ready_orders_count'];
    _cookingOrdersCount = json['cooking_orders_count'];
    _onAWayOrdersCount = json['on_a_way_orders_count'];
    _ordersCount = json['orders_count'];
    _totalPrice = json['total_price'];
    _todayCount = json['today_count'];
  }

  int? _progressOrdersCount;
  int? _deliveredOrdersCount;
  int? _cancelOrdersCount;
  int? _newOrdersCount;
  int? _acceptedOrdersCount;
  int? _readyOrdersCount;
  int? _cookingOrdersCount;
  int? _onAWayOrdersCount;
  int? _ordersCount;
  num? _totalPrice;
  int? _todayCount;

  OrdersStatistic copyWith({
    int? progressOrdersCount,
    int? deliveredOrdersCount,
    int? cancelOrdersCount,
    int? newOrdersCount,
    int? acceptedOrdersCount,
    int? readyOrdersCount,
    int? cookingOrdersCount,
    int? onAWayOrdersCount,
    int? ordersCount,
    num? totalPrice,
    int? todayCount,
  }) =>
      OrdersStatistic(
        progressOrdersCount: progressOrdersCount ?? _progressOrdersCount,
        deliveredOrdersCount: deliveredOrdersCount ?? _deliveredOrdersCount,
        cancelOrdersCount: cancelOrdersCount ?? _cancelOrdersCount,
        newOrdersCount: newOrdersCount ?? _newOrdersCount,
        cookingOrdersCount: cookingOrdersCount ?? _cookingOrdersCount,
        acceptedOrdersCount: acceptedOrdersCount ?? _acceptedOrdersCount,
        readyOrdersCount: readyOrdersCount ?? _readyOrdersCount,
        onAWayOrdersCount: onAWayOrdersCount ?? _onAWayOrdersCount,
        ordersCount: ordersCount ?? _ordersCount,
        totalPrice: totalPrice ?? _totalPrice,
        todayCount: todayCount ?? _todayCount,
      );

  int? get progressOrdersCount => _progressOrdersCount;

  int? get deliveredOrdersCount => _deliveredOrdersCount;

  int? get cancelOrdersCount => _cancelOrdersCount;

  int? get newOrdersCount => _newOrdersCount;

  int? get cookingOrdersCount => _cookingOrdersCount;

  int? get acceptedOrdersCount => _acceptedOrdersCount;

  int? get readyOrdersCount => _readyOrdersCount;

  int? get onAWayOrdersCount => _onAWayOrdersCount;

  int? get ordersCount => _ordersCount;

  num? get totalPrice => _totalPrice;

  int? get todayCount => _todayCount;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    map['progress_orders_count'] = _progressOrdersCount;
    map['delivered_orders_count'] = _deliveredOrdersCount;
    map['cancel_orders_count'] = _cancelOrdersCount;
    map['new_orders_count'] = _newOrdersCount;
    map['accepted_orders_count'] = _acceptedOrdersCount;
    map['ready_orders_count'] = _readyOrdersCount;
    map['on_a_way_orders_count'] = _onAWayOrdersCount;
    map['orders_count'] = _ordersCount;
    map['total_price'] = _totalPrice;
    map['today_count'] = _todayCount;
    return map;
  }
}
