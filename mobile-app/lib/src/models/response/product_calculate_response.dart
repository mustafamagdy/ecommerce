
import 'package:admin_desktop/src/models/data/product_data.dart';
import 'package:admin_desktop/src/models/data/shop_data.dart';

class ProductCalculateResponse {
  ProductCalculateResponse({
    this.timestamp,
    this.status,
    this.message,
    this.data,
  });

  DateTime? timestamp;
  bool? status;
  String? message;
  ProductCalculateResponseData? data;

  ProductCalculateResponse copyWith({
    DateTime? timestamp,
    bool? status,
    String? message,
    ProductCalculateResponseData? data,
  }) =>
      ProductCalculateResponse(
        timestamp: timestamp ?? this.timestamp,
        status: status ?? this.status,
        message: message ?? this.message,
        data: data ?? this.data,
      );

  factory ProductCalculateResponse.fromJson(Map<String, dynamic> json) => ProductCalculateResponse(
    timestamp: json["timestamp"] == null ? null : DateTime.parse(json["timestamp"]),
    status: json["status"],
    message: json["message"],
    data: json["data"] == null ? null : ProductCalculateResponseData.fromJson(json["data"]),
  );

  Map<String, dynamic> toJson() => {
    "timestamp": timestamp?.toIso8601String(),
    "status": status,
    "message": message,
    "data": data?.toJson(),
  };
}

class ProductCalculateResponseData {
  ProductCalculateResponseData({
    this.status,
    this.code,
    this.data,
  });

  bool? status;
  String? code;
  PriceDate? data;

  ProductCalculateResponseData copyWith({
    bool? status,
    String? code,
    PriceDate? data,
  }) =>
      ProductCalculateResponseData(
        status: status ?? this.status,
        code: code ?? this.code,
        data: data ?? this.data,
      );

  factory ProductCalculateResponseData.fromJson(Map<String, dynamic> json) => ProductCalculateResponseData(
    status: json["status"],
    code: json["code"],
    data: json["data"] == null ? null : PriceDate.fromJson(json["data"]),
  );

  Map<String, dynamic> toJson() => {
    "status": status,
    "code": code,
    "data": data?.toJson(),
  };
}

class PriceDate {
  PriceDate({
    this.stocks,
    this.totalTax,
    this.price,
    this.totalShopTax,
    this.totalPrice,
    this.totalDiscount,
    this.deliveryFee,
    this.rate,
    this.couponPrice,
    this.shop,
    this.km,
  });

  List<ProductData>? stocks;
  num? totalTax;
  num? price;
  num? totalShopTax;
  num? totalPrice;
  num? totalDiscount;
  num? deliveryFee;
  num? rate;
  num? couponPrice;
  ShopData? shop;
  double? km;

  PriceDate copyWith({
    List<ProductData>? stocks,
    num? totalTax,
    num? price,
    num? totalShopTax,
    num? totalPrice,
    num? totalDiscount,
    num? deliveryFee,
    num? rate,
    num? couponPrice,
    ShopData? shop,
    double? km,
  }) =>
      PriceDate(
        stocks: stocks ?? this.stocks,
        totalTax: totalTax ?? this.totalTax,
        price: price ?? this.price,
        totalShopTax: totalShopTax ?? this.totalShopTax,
        totalPrice: totalPrice ?? this.totalPrice,
        totalDiscount: totalDiscount ?? this.totalDiscount,
        deliveryFee: deliveryFee ?? this.deliveryFee,
        rate: rate ?? this.rate,
        couponPrice: couponPrice ?? this.couponPrice,
        shop: shop ?? this.shop,
        km: km ?? this.km,
      );

  factory PriceDate.fromJson(Map<String, dynamic> json) {
    return PriceDate(
    stocks: json["stocks"] == null ? [] : List<ProductData>.from(json["stocks"]!.map((x) => ProductData.fromJson(x))),
    totalTax: json["total_tax"]?.toDouble(),
    price: json["price"],
    totalShopTax: json["total_shop_tax"]?.toDouble(),
    totalPrice: json["total_price"]?.toDouble(),
    totalDiscount: json["total_discount"],
    deliveryFee: json["delivery_fee"],
    rate: json["rate"],
    couponPrice: json["coupon_price"],
    shop: json["shop"] == null ? null : ShopData.fromJson(json["shop"]),
    km: json["km"]?.toDouble(),
  );
  }

  Map<String, dynamic> toJson() => {
    "stocks": stocks == null ? [] : List<dynamic>.from(stocks!.map((x) => x.toJson())),
    "total_tax": totalTax,
    "price": price,
    "total_shop_tax": totalShopTax,
    "total_price": totalPrice,
    "total_discount": totalDiscount,
    "delivery_fee": deliveryFee,
    "rate": rate,
    "coupon_price": couponPrice,
    "shop": shop?.toJson(),
    "km": km,
  };
}








