import '../data/meta.dart';
import '../data/shop_data.dart';

class ShopsPaginateResponse {
  ShopsPaginateResponse({List<ShopData>? data, Meta? meta}) {
    _data = data;
    _meta = meta;
  }

  ShopsPaginateResponse.fromJson(dynamic json) {
    if (json['data'] != null) {
      _data = [];
      json['data'].forEach((v) {
        _data?.add(ShopData.fromJson(v));
      });
    }
    _meta = json['meta'] != null ? Meta.fromJson(json['meta']) : null;
  }

  List<ShopData>? _data;
  Meta? _meta;

  ShopsPaginateResponse copyWith({List<ShopData>? data, Meta? meta}) =>
      ShopsPaginateResponse(data: data ?? _data, meta: meta ?? _meta);

  List<ShopData>? get data => _data;

  Meta? get meta => _meta;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    if (_data != null) {
      map['data'] = _data?.map((v) => v.toJson()).toList();
    }
    if (_meta != null) {
      map['meta'] = _meta?.toJson();
    }
    return map;
  }
}
