import '../data/order_data.dart';

class SingleOrderResponse {
  SingleOrderResponse({OrderData? data}) {
    _data = data;
  }

  SingleOrderResponse.fromJson(dynamic json) {
    _data = json['data'] != null ? OrderData.fromJson(json['data']) : null;
  }

  OrderData? _data;

  SingleOrderResponse copyWith({OrderData? data}) =>
      SingleOrderResponse(data: data ?? _data);

  OrderData? get data => _data;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    if (_data != null) {
      map['data'] = _data?.toJson();
    }
    return map;
  }
}
