import '../data/meta.dart';
import '../data/product_data.dart';

class ProductsPaginateResponse {
  ProductsPaginateResponse({
    List<ProductData>? data,
    Meta? meta,
  }) {
    _data = data;
    // _links = links;
    _meta = meta;
  }

  ProductsPaginateResponse.fromJson(dynamic json) {
    if (json['data'] != null) {
      _data = [];
      json['data'].forEach((v) {
        _data?.add(ProductData.fromJson(v));
      });
    }
    _meta = json['meta'] != null ? Meta.fromJson(json['meta']) : null;
  }

  List<ProductData>? _data;
  Meta? _meta;

  ProductsPaginateResponse copyWith({
    List<ProductData>? data,
    Meta? meta,
  }) =>
      ProductsPaginateResponse(
        data: data ?? _data,
        meta: meta ?? _meta,
      );

  List<ProductData>? get data => _data;

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