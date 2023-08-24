

class SaleCartResponse {
  double? deliveryFee;
  double? cash;
  double? other;

  SaleCartResponse({
    this.deliveryFee,
    this.cash,
    this.other,
  });

  SaleCartResponse copyWith({
    double? deliveryFee,
    double? cash,
    double? other,
  }) =>
      SaleCartResponse(
        deliveryFee: deliveryFee ?? this.deliveryFee,
        cash: cash ?? this.cash,
        other: other ?? this.other,
      );

  factory SaleCartResponse.fromJson(Map<String, dynamic> json) => SaleCartResponse(
    deliveryFee: json["delivery_fee"]?.toDouble(),
    cash: json["cash"]?.toDouble(),
    other: json["other"]?.toDouble(),
  );

  Map<String, dynamic> toJson() => {
    "delivery_fee": deliveryFee,
    "cash": cash,
    "other": other,
  };
}
